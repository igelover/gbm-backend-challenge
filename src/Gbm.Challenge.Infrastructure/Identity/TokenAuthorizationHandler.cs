using Gbm.Challenge.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;

namespace Gbm.Challenge.Infrastructure.Identity;

public class TokenAuthorizationHandler : AuthorizationHandler<ValidTokenRequirement>
{
    private readonly DataContext _dbContext;

    public TokenAuthorizationHandler(DataContext dbContext)
    {
        _dbContext = dbContext;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
        ValidTokenRequirement requirement)
    {
        var tokenId = context.User.Claims.FirstOrDefault(claim => claim.Type == JwtRegisteredClaimNames.Jti)?.Value;
        var clientName = context.User.Identity?.Name;

        var isTokenValid = await _dbContext.Sessions.AnyAsync(
            s => s.ClientName == clientName
                            && s.TokenId == tokenId
                            && s.Active);

        if (isTokenValid)
        {
            context.Succeed(requirement);
        }
    }
}