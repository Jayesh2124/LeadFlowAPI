using LeadFlow.Application.Common.Interfaces;
using LeadFlow.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LeadFlow.Application.Features.Leads.Commands.DeleteLead;

public record DeleteLeadCommand(Guid Id) : IRequest<Result>;

public class DeleteLeadHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    : IRequestHandler<DeleteLeadCommand, Result>
{
    public async Task<Result> Handle(DeleteLeadCommand cmd, CancellationToken ct)
    {
        var query = db.Leads.AsQueryable();
        
        // Non-admins can only delete their own leads
        if (!currentUser.IsAdmin)
            query = query.Where(l => l.UserId == currentUser.UserId);

        var lead = await query.FirstOrDefaultAsync(l => l.Id == cmd.Id, ct);
        if (lead is null) return Result.Failure("Lead not found.");

        lead.Deactivate();
        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}
