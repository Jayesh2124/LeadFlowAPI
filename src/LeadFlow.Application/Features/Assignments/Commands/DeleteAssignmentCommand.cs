using FluentValidation;
using LeadFlow.Application.Common.Interfaces;
using LeadFlow.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LeadFlow.Application.Features.Assignments.Commands;

public record DeleteAssignmentCommand(Guid Id) : IRequest;

public class DeleteAssignmentCommandHandler : IRequestHandler<DeleteAssignmentCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteAssignmentCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteAssignmentCommand request, CancellationToken cancellationToken)
    {
        var assignment = await _context.ResourceAssignments
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

        if (assignment == null)
            throw new Exception("ResourceAssignment not found");

        _context.ResourceAssignments.Remove(assignment);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
