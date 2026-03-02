using LeadFlow.Application.Common.Interfaces;
using LeadFlow.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LeadFlow.Application.Features.Leads.Queries.GetLeads;

public record GetLeadsQuery(
    string? Search = null,
    string? Status = null,
    int Page = 1,
    int PageSize = 20) : IRequest<Result<PagedLeadsResult>>;

public record LeadDto(Guid Id, string FirstName, string LastName, string Email,
    string? Phone, string Company, string? Position, string Status,
    string Source, string? Notes, List<string> Tags, bool IsActive,
    DateTime CreatedAt, DateTime? UpdatedAt, string Country, string? City,
    string? State, string? Address, string? ZipCode, string? Website,
    List<string> Technologies);

public record PagedLeadsResult(List<LeadDto> Items, int Total, int Page, int PageSize);

public class GetLeadsHandler(IApplicationDbContext db, ICurrentUserService currentUser)
    : IRequestHandler<GetLeadsQuery, Result<PagedLeadsResult>>
{
    public async Task<Result<PagedLeadsResult>> Handle(GetLeadsQuery q, CancellationToken ct)
    {
        var query = db.Leads.AsQueryable();

        if (currentUser.IsAdmin)
        {
            // Admin sees everything
        }
        else
        {
            // User sees only own active leads
            query = query.Where(l => l.UserId == currentUser.UserId && l.IsActive);
        }

        if (!string.IsNullOrWhiteSpace(q.Status))
            query = query.Where(l => l.Status == q.Status);

        if (!string.IsNullOrWhiteSpace(q.Search))
        {
            var s = q.Search.ToLower();
            query = query.Where(l =>
                l.FirstName.ToLower().Contains(s) ||
                l.LastName.ToLower().Contains(s) ||
                l.Email.ToLower().Contains(s) ||
                l.Company.ToLower().Contains(s));
        }

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(l => l.CreatedAt)
            .Skip((q.Page - 1) * q.PageSize)
            .Take(q.PageSize)
            .Select(l => new LeadDto(l.Id, l.FirstName, l.LastName, l.Email,
                l.Phone, l.Company, l.Position, l.Status, l.Source,
                l.Notes, l.Tags, l.IsActive, l.CreatedAt, l.UpdatedAt,
                l.Country, l.City, l.State, l.Address, l.ZipCode, l.Website,
                l.Technologies))
            .ToListAsync(ct);

        return Result<PagedLeadsResult>.Success(new PagedLeadsResult(items, total, q.Page, q.PageSize));
    }
}
