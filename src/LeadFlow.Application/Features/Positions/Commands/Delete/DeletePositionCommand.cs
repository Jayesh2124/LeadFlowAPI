using LeadFlow.Application.Common.Interfaces;
using LeadFlow.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LeadFlow.Application.Features.Positions.Commands.Delete;

public record DeletePositionCommand(Guid Id) : IRequest;

public class DeletePositionCommandHandler : IRequestHandler<DeletePositionCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public DeletePositionCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task Handle(DeletePositionCommand request, CancellationToken cancellationToken)
    {
        var position = await _context.OpportunityPositions
            .Include(p => p.Opportunity)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (position is null)
            throw new Exception("Position not found.");

        // Authorization: Admin OR Opportunity Owner
        if (!_currentUserService.IsAdmin && position.Opportunity.OwnerUserId != _currentUserService.UserId)
            throw new UnauthorizedAccessException("You do not have permission to delete this position.");

        // Business Rule: Cannot delete a Filled position
        if (position.Status == PositionStatus.Filled)
            throw new Exception("Cannot delete a position that is already Filled.");

        _context.OpportunityPositions.Remove(position);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
