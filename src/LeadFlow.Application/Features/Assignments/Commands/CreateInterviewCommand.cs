using FluentValidation;
using LeadFlow.Application.Common.Interfaces;
using LeadFlow.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Hangfire;

namespace LeadFlow.Application.Features.Assignments.Commands;

public record CreateInterviewCommand(
    Guid AssignmentId, 
    string InterviewStage, 
    string? InterviewerName,
    string? InterviewerEmail, 
    DateTime ScheduledAt,
    string? EmailBody) : IRequest<Guid>;

public class CreateInterviewCommandValidator : AbstractValidator<CreateInterviewCommand>
{
    public CreateInterviewCommandValidator()
    {
        RuleFor(v => v.AssignmentId).NotEmpty();
        RuleFor(v => v.InterviewStage).NotEmpty().MaximumLength(50);
        RuleFor(v => v.InterviewerName).MaximumLength(200);
        RuleFor(v => v.InterviewerEmail).EmailAddress().When(v => !string.IsNullOrEmpty(v.InterviewerEmail));
        RuleFor(v => v.ScheduledAt).NotEmpty();
    }
}

public class CreateInterviewCommandHandler(
    IApplicationDbContext context,
    IBackgroundJobClient hangfire,
    ICurrentUserService currentUser) : IRequestHandler<CreateInterviewCommand, Guid>
{
    public async Task<Guid> Handle(CreateInterviewCommand request, CancellationToken cancellationToken)
    {
        var assignmentExists = await context.ResourceAssignments
            .AnyAsync(a => a.Id == request.AssignmentId, cancellationToken);

        if (!assignmentExists)
            throw new Exception("ResourceAssignment not found");

        var interview = AssignmentInterview.Create(
            request.AssignmentId,
            request.InterviewStage,
            request.ScheduledAt,
            "Scheduled",
            request.InterviewerName,
            request.InterviewerEmail);

        context.AssignmentInterviews.Add(interview);

        await context.SaveChangesAsync(cancellationToken);

        // Schedule email to both Interviewer and Candidate
        hangfire.Enqueue<IInterviewEmailService>(service =>
            service.SendInterviewEmailsAsync(interview.Id, currentUser.UserId, request.EmailBody, CancellationToken.None));

        return interview.Id;
    }
}
