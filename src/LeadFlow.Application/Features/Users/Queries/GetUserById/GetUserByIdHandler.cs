using System;
using LeadFlow.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LeadFlow.Application.Features.Users.Queries.GetUserById;

public class GetUserByIdQuery : IRequest<UserDetailDto?>
{
    public Guid Id { get; }
    public GetUserByIdQuery(Guid id) => Id = id;
}

public record UserDetailDto(
    Guid     Id,
    string   Name,
    string   Email,
    string   Role,
    bool     IsActive,
    DateTime CreatedAt,
    SmtpSummary? Smtp
);

public record SmtpSummary(
    string Host,
    int    Port,
    string Username,
    string FromEmail,
    string FromName,
    bool   EnableSsl,
    bool   IsVerified
);

public class GetUserByIdHandler(IApplicationDbContext db)
    : IRequestHandler<GetUserByIdQuery, UserDetailDto?>
{
    public async Task<UserDetailDto?> Handle(GetUserByIdQuery q, CancellationToken ct)
    {
        var user = await db.Users
            .Include(u => u.SmtpSettings)
            .FirstOrDefaultAsync(u => u.Id == q.Id, ct);

        if (user is null) return null;

        var smtp = user.SmtpSettings is { } s
            ? new SmtpSummary(s.Host, s.Port, s.Username, s.FromEmail, s.FromName, s.EnableSsl, s.IsVerified)
            : null;

        return new UserDetailDto(
            user.Id, user.Name, user.Email, user.Role,
            user.IsActive, user.CreatedAt, smtp);
    }
}

