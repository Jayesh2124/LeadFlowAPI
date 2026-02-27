using LeadFlow.Application.Common.Interfaces;
using LeadFlow.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LeadFlow.Application.Features.Templates.Commands.UpdateTemplate;

public record UpdateTemplateCommand(
    Guid Id,
    string Name,
    string Subject,
    string Body,
    string Status,
    List<string>? Attachments = null
) : IRequest<Result>;

public class UpdateTemplateHandler(IApplicationDbContext db)
    : IRequestHandler<UpdateTemplateCommand, Result>
{
    public async Task<Result> Handle(UpdateTemplateCommand cmd, CancellationToken ct)
    {
        var template = await db.EmailTemplates.FindAsync(new object[] { cmd.Id }, ct);
        if (template is null) return Result.Failure("Template not found");

        template.Update(cmd.Name, cmd.Subject, cmd.Body, cmd.Attachments);
        template.SetActive(cmd.Status.ToLower() == "active");
        
        await db.SaveChangesAsync(ct);

        return Result.Success();
    }
}
