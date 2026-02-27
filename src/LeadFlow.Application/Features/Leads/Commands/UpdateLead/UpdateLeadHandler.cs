using LeadFlow.Application.Common.Interfaces;
using LeadFlow.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LeadFlow.Application.Features.Leads.Commands.UpdateLead;

public record UpdateLeadCommand(
    Guid Id, string FirstName, string LastName, string Email,
    string? Phone, string Company, string? Position,
    string Status, string Source, string? Notes,
    List<string> Tags) : IRequest<Result>;

public class UpdateLeadHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    : IRequestHandler<UpdateLeadCommand, Result>
{
    public async Task<Result> Handle(UpdateLeadCommand cmd, CancellationToken ct)
    {
        var lead = await db.Leads
            .FirstOrDefaultAsync(l => l.Id == cmd.Id && l.UserId == currentUser.UserId, ct);
        if (lead is null) return Result.Failure("Lead not found.");

        lead.Update(cmd.FirstName, cmd.LastName, cmd.Email, cmd.Phone, cmd.Company,
            cmd.Position, cmd.Status, cmd.Source, cmd.Notes, cmd.Tags ?? []);

        await db.SaveChangesAsync(ct);
        return Result.Success();
    }
}
