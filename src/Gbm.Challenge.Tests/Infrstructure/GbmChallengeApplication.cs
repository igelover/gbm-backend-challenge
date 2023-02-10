using Gbm.Challenge.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace Gbm.Challenge.Tests.Infrastructure;

internal class GbmChallengeApplication<T> : WebApplicationFactory<T> where T : class
{
    private SqliteConnection? _connection;

    protected override IHost CreateHost(IHostBuilder builder)
    {
        _connection = new SqliteConnection("Filename=:memory:");
        _connection.Open();

        builder.ConfigureServices(services =>
        {
            services.RemoveAll(typeof(DbContextOptions<DataContext>));
            services.AddDbContext<DataContext>(options => { options.UseSqlite(_connection); });
        });

        return base.CreateHost(builder);
    }

    protected override void Dispose(bool disposing)
    {
        _connection?.Dispose();
        base.Dispose(disposing);
    }
}