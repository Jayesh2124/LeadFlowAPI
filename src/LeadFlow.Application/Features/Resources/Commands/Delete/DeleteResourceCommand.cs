using LeadFlow.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LeadFlow.Application.Features.Resources.Commands.Delete;

public record DeleteResourceCommand(Guid Id) : IRequest;

public class DeleteResourceCommandHandler : IRequestHandler<DeleteResourceCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public DeleteResourceCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task Handle(DeleteResourceCommand request, CancellationToken cancellationToken)
    {
        var resource = await _context.Resources
            .FirstOrDefaultAsync(r => r.Id == request.Id && !r.IsDeleted, cancellationToken);

        if (resource is null)
            throw new Exception("Resource not found.");

        // Authorization: Admin OR Owner
        if (!_currentUserService.IsAdmin && resource.UserId != _currentUserService.UserId)
            throw new UnauthorizedAccessException("You do not have permission to delete this resource.");

        resource.SoftDelete();

        await _context.SaveChangesAsync(cancellationToken);
    }
}
