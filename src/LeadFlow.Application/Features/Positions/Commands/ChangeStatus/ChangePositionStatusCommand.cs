using FluentValidation;
using LeadFlow.Application.Common.Interfaces;
using LeadFlow.Application.Features.Positions.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LeadFlow.Application.Features.Positions.Commands.ChangeStatus;

public record ChangePositionStatusCommand(Guid Id, ChangePositionStatusRequest Request) : IRequest;

public class ChangePositionStatusValidator : AbstractValidator<ChangePositionStatusCommand>
{
    public ChangePositionStatusValidator()
    {
        RuleFor(v => v.Id)
            .NotEmpty().WithMessage("Position Id is required.");

        RuleFor(v => v.Request.Status)
            .IsInEnum().WithMessage("Invalid position status.");
    }
}

public class ChangePositionStatusCommandHandler : IRequestHandler<ChangePositionStatusCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public ChangePositionStatusCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task Handle(ChangePositionStatusCommand request, CancellationToken cancellationToken)
    {
        var position = await _context.OpportunityPositions
            .Include(p => p.Opportunity)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (position is null)
            throw new Exception("Position not found.");

        // Authorization: Admin OR Opportunity Owner
        if (!_currentUserService.IsAdmin && position.Opportunity.OwnerUserId != _currentUserService.UserId)
            throw new UnauthorizedAccessException("You do not have permission to change the status of this position.");

        // Business Rule: Closed is a terminal state
        if (position.Status == Domain.Enums.PositionStatus.Closed && !_currentUserService.IsAdmin)
            throw new Exception("Cannot change status of a Closed position. Contact an administrator.");

        position.UpdateStatus(request.Request.Status);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
