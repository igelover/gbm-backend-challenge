using Gbm.Challenge.Application.Contracts.Persistence;
using Gbm.Challenge.Infrastructure.Persistence;
using Gbm.Challenge.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Gbm.Challenge.Infrastructure;

public static class InfrastructureServiceRegistration
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<DataContext>(options => options.UseSqlServer(configuration.GetConnectionString("GbmConnectionString")));

        services.AddScoped(typeof(IAsyncRepository<>), typeof(RepositoryBase<>));
        services.AddScoped<IAccountRepository, AccountRepository>();

        return services;
    }
}
