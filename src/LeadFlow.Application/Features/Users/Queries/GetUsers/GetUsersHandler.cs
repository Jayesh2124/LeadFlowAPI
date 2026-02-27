using LeadFlow.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LeadFlow.Application.Features.Users.Queries.GetUsers;

public record GetUsersQuery(
    string? Search    = null,
    string? Role      = null,
    bool?   IsActive  = null,
    int     Page      = 1,
    int     PageSize  = 20
) : IRequest<GetUsersResult>;

public record UserDto(
    Guid     Id,
    string   Name,
    string   Email,
    string   Role,
    bool     IsActive,
    DateTime CreatedAt,
    bool     HasSmtp
);

public record GetUsersResult(List<UserDto> Items, int TotalCount, int Page, int PageSize);

public class GetUsersHandler(IApplicationDbContext db)
    : IRequestHandler<GetUsersQuery, GetUsersResult>
{
    public async Task<GetUsersResult> Handle(GetUsersQuery q, CancellationToken ct)
    {
        var query = db.Users
            .Include(u => u.SmtpSettings)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(q.Search))
            query = query.Where(u =>
                u.Name.Contains(q.Search) || u.Email.Contains(q.Search));

        if (!string.IsNullOrWhiteSpace(q.Role))
            query = query.Where(u => u.Role == q.Role);

        if (q.IsActive.HasValue)
            query = query.Where(u => u.IsActive == q.IsActive.Value);

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderBy(u => u.Name)
            .Skip((q.Page - 1) * q.PageSize)
            .Take(q.PageSize)
            .Select(u => new UserDto(
                u.Id, u.Name, u.Email, u.Role, u.IsActive, u.CreatedAt,
                u.SmtpSettings != null && u.SmtpSettings.IsVerified))
            .ToListAsync(ct);

        return new GetUsersResult(items, total, q.Page, q.PageSize);
    }
}
