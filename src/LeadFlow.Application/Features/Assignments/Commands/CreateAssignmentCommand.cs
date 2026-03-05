using FluentValidation;
using LeadFlow.Application.Common.Interfaces;
using LeadFlow.Domain.Entities;
using LeadFlow.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LeadFlow.Application.Features.Assignments.Commands;

public record CreateAssignmentCommand(Guid PositionId, Guid ResourceId, string? Notes) : IRequest<Guid>;

public class CreateAssignmentCommandValidator : AbstractValidator<CreateAssignmentCommand>
{
    public CreateAssignmentCommandValidator()
    {
        RuleFor(v => v.PositionId).NotEmpty();
        RuleFor(v => v.ResourceId).NotEmpty();
    }
}

public class CreateAssignmentCommandHandler : IRequestHandler<CreateAssignmentCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public CreateAssignmentCommandHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Guid> Handle(CreateAssignmentCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId;

        // Verify Position exists
        var positionExists = await _context.OpportunityPositions
            .AnyAsync(p => p.Id == request.PositionId, cancellationToken);
        if (!positionExists)
            throw new Exception("OpportunityPosition not found");

        // Verify Resource exists
        var resourceExists = await _context.Resources
            .AnyAsync(r => r.Id == request.ResourceId, cancellationToken);
        if (!resourceExists)
            throw new Exception("Resource not found");

        // Check for duplicate assignment
        var existingAssignment = await _context.ResourceAssignments
            .AnyAsync(a => a.PositionId == request.PositionId && a.ResourceId == request.ResourceId, cancellationToken);
        
        if (existingAssignment)
            throw new InvalidOperationException("This resource is already assigned to this position.");

        // Create assignment (Default stage is set inside the Create method)
        var assignment = ResourceAssignment.Create(
            request.PositionId,
            request.ResourceId,
            currentUserId,
            "Active",
            request.Notes);

        _context.ResourceAssignments.Add(assignment);

        // Initial stage history
        var stageHistory = AssignmentStageHistory.Create(
            assignment.Id,
            null,
            AssignmentStage.Applied.ToString(),
            currentUserId);

        _context.AssignmentStageHistories.Add(stageHistory);

        await _context.SaveChangesAsync(cancellationToken);

        return assignment.Id;
    }
}
