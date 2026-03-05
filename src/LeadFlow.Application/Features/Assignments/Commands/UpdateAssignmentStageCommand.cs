using FluentValidation;
using LeadFlow.Application.Common.Interfaces;
using LeadFlow.Domain.Entities;
using LeadFlow.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LeadFlow.Application.Features.Assignments.Commands;

public record UpdateAssignmentStageCommand(Guid Id, AssignmentStage NewStage) : IRequest;

public class UpdateAssignmentStageCommandValidator : AbstractValidator<UpdateAssignmentStageCommand>
{
    public UpdateAssignmentStageCommandValidator()
    {
        RuleFor(v => v.Id).NotEmpty();
        RuleFor(v => v.NewStage).IsInEnum();
    }
}

public class UpdateAssignmentStageCommandHandler : IRequestHandler<UpdateAssignmentStageCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public UpdateAssignmentStageCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task Handle(UpdateAssignmentStageCommand request, CancellationToken cancellationToken)
    {
        var assignment = await _context.ResourceAssignments
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

        if (assignment == null)
            throw new Exception("ResourceAssignment not found");

        var currentUserId = _currentUserService.UserId;
        var previousStage = assignment.Stage.ToString();

        // Update stage
        assignment.UpdateStage(request.NewStage, assignment.Status);

        // Record stage history
        var stageHistory = AssignmentStageHistory.Create(
            assignment.Id,
            previousStage,
            request.NewStage.ToString(),
            currentUserId);

        _context.AssignmentStageHistories.Add(stageHistory);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
