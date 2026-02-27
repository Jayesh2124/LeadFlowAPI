using System.Security.Claims;
using LeadFlow.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace LeadFlow.Infrastructure.Services;

public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    private ClaimsPrincipal? User => httpContextAccessor.HttpContext?.User;

    public Guid UserId
    {
        get
        {
            var sub = User?.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)
                   ?? User?.FindFirstValue(ClaimTypes.NameIdentifier)
                   ?? throw new UnauthorizedAccessException("User is not authenticated.");
            return Guid.Parse(sub);
        }
    }

    public string Role
        => User?.FindFirstValue(ClaimTypes.Role) ?? "User";

    public bool IsAdmin => Role.Equals("Admin", StringComparison.OrdinalIgnoreCase);
}
