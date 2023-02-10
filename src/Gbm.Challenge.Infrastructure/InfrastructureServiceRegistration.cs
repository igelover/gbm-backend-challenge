using Gbm.Challenge.Application.Contracts.Identity;
using Gbm.Challenge.Application.Contracts.Persistence;
using Gbm.Challenge.Domain.Common;
using Gbm.Challenge.Infrastructure.Identity;
using Gbm.Challenge.Infrastructure.Persistence;
using Gbm.Challenge.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;

namespace Gbm.Challenge.Infrastructure;

public static class InfrastructureServiceRegistration
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<DataContext>(options => options.UseSqlServer(configuration.GetConnectionString("GbmConnectionString")));

        services.AddTransient<IJwtService, JwtService>();
        services.AddTransient<IAuthorizationHandler, TokenAuthorizationHandler>();

        services.AddScoped(typeof(IAsyncRepository<>), typeof(RepositoryBase<>));
        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<IClientRepository, ClientRepository>();
        services.AddScoped<ISessionRepository, SessionRepository>();

        return services;
    }

    public static void AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var issuer = configuration["Jwt:Issuer"];
        var audience = configuration["Jwt:Audience"];
        var signingKey = configuration["Jwt:Key"];

        services.AddAuthorization();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = false;
                    options.SaveToken = true;
                    options.TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidAudience = audience,
                        ValidIssuer = issuer,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
                        RoleClaimType = ClaimTypes.Role,
                        NameClaimType = ClaimTypes.NameIdentifier
                    };
                });

        services.AddAuthorization(options =>
        {
            options.AddPolicy("ChallengeApiAdmin", policy =>
                policy.RequireRole(DomainConstants.JwtScopeAdminRole)
            );

            options.AddPolicy("ChallengeApiUser", policy =>
            {
                policy.RequireRole(DomainConstants.JwtScopeUserRole);
                policy.AddRequirements(new ValidTokenRequirement());
            });
        });

        services.AddHttpContextAccessor();
    }

    public static void EnsureDatabaseSetup(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;
        var db = services.GetRequiredService<DataContext>();
        db.Database.EnsureCreated();
        DataSeeder.Seed(db);
    }
}
