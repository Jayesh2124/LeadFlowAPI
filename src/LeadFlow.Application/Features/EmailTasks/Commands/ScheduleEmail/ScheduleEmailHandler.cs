using FluentValidation;
using Hangfire;
using LeadFlow.Application.Common.Interfaces;
using LeadFlow.Application.Common.Models;
using LeadFlow.Domain.Entities;
using LeadFlow.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LeadFlow.Application.Features.EmailTasks.Commands.ScheduleEmail;

public record ScheduleEmailCommand(
    Guid LeadId,
    Guid TemplateId,
    DateTime? ScheduledAt,
    bool ApplySystemFollowups = true
) : IRequest<Result<Guid>>;

public class ScheduleEmailValidator : AbstractValidator<ScheduleEmailCommand>
{
    public ScheduleEmailValidator()
    {
        RuleFor(x => x.LeadId).NotEmpty();
        RuleFor(x => x.TemplateId).NotEmpty();
        RuleFor(x => x.ScheduledAt)
            .GreaterThan(DateTime.UtcNow)
            .When(x => x.ScheduledAt.HasValue)
            .WithMessage("Scheduled time must be in the future.");
    }
}

public class ScheduleEmailHandler(
    IApplicationDbContext db,
    ICurrentUserService currentUser,
    IBackgroundJobClient hangfire)
    : IRequestHandler<ScheduleEmailCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(ScheduleEmailCommand cmd, CancellationToken ct)
    {
        // 1. Authorize lead
        var lead = await db.Leads
            .FirstOrDefaultAsync(l => l.Id == cmd.LeadId, ct);
        if (lead is null) return Result<Guid>.Failure("Lead not found.");

        // 2. Authorize template
        var template = await db.EmailTemplates
            .FirstOrDefaultAsync(t => t.Id == cmd.TemplateId, ct);
        if (template is null) return Result<Guid>.Failure("Template not found.");

        // 3. SMTP check
        var smtpSettings = await db.UserSmtpSettings
            .FirstOrDefaultAsync(s => s.UserId == currentUser.UserId, ct);
        if (smtpSettings is null) return Result<Guid>.Failure("No verified SMTP settings. Please configure SMTP first.");

        // Resolve schedule time, default to now if not provided
        var scheduledTime = cmd.ScheduledAt?.ToUniversalTime() ?? DateTime.UtcNow;

        // 4. Idempotency check
        var idempotencyKey = $"{currentUser.UserId}:{cmd.LeadId}:{cmd.TemplateId}:{scheduledTime:yyyyMMddHHmm}";
        var alreadyExists = await db.EmailTasks
            .AnyAsync(t => t.IdempotencyKey == idempotencyKey
                        && t.Status != Domain.Enums.EmailTaskStatus.Cancelled, ct);
        if (alreadyExists) return Result<Guid>.Failure("Duplicate: email already scheduled for this time slot.");

        // 5. Load system settings
        var settings = await db.SystemSettings.FirstOrDefaultAsync(ct)
                       ?? LeadFlow.Domain.Entities.SystemSettings.CreateDefault();


        // 6. Render template
        var vars = BuildVars(lead, smtpSettings);
        var subject = Render(template.Subject, vars);
        var body    = Render(template.Body, vars);

        // 7. Create task
        var task = EmailTask.Create(
            currentUser.UserId, lead.Id, template.Id,
            subject, body, scheduledTime,
            maxAttempts: settings.DefaultMaxRetries);

        // 8. Attach followup rules
        if (cmd.ApplySystemFollowups && settings.AutoFollowup)
        {
            foreach (var rule in settings.FollowupRules.Where(r => r.Enabled).OrderBy(r => r.Order))
            {
                task.Followups.Add(EmailFollowup.Create(
                    task.Id, rule.TemplateId ?? template.Id, rule.DelayDays, rule.Condition, rule.Order));
            }
        }

        db.EmailTasks.Add(task);
        template.IncrementUsage();
        await db.SaveChangesAsync(ct);

        // 9. Enqueue Hangfire job
        var delay = scheduledTime - DateTime.UtcNow;
        var jobId = delay > TimeSpan.Zero
            ? hangfire.Schedule<IEmailTaskProcessor>(
                p => p.ProcessAsync(task.Id, CancellationToken.None), delay)
            : hangfire.Enqueue<IEmailTaskProcessor>(
                p => p.ProcessAsync(task.Id, CancellationToken.None));

        task.SetHangfireJobId(jobId);
        await db.SaveChangesAsync(ct);

        return Result<Guid>.Success(task.Id);
    }

    private static Dictionary<string, string> BuildVars(Lead lead, UserSmtpSettings smtpSettings) => new()
    {
        ["FirstName"] = lead.FirstName ?? "",
        ["LastName"]  = lead.LastName ?? "",
        ["FullName"]  = lead.FullName ?? "",
        ["Email"]     = lead.Email ?? "",
        ["Company"]   = lead.Company ?? "",
        ["Position"]  = lead.Position ?? "",
        ["Phone"]     = lead.Phone ?? "",
        ["SenderName"] = smtpSettings.FromName ?? "",
        ["SenderEmail"] = smtpSettings.FromEmail ?? "",
    };

    private static string Render(string template, Dictionary<string, string> vars)
        => vars.Aggregate(template ?? "", (current, kv) =>
               current.Replace($"{{{{{kv.Key}}}}}", kv.Value ?? ""));
}
