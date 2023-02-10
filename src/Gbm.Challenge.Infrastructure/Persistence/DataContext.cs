using Gbm.Challenge.Domain.Entities;
using Gbm.Challenge.Domain.Identity;
using Microsoft.EntityFrameworkCore;

namespace Gbm.Challenge.Infrastructure.Persistence;

public class DataContext : DbContext
{
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Session> Sessions => Set<Session>();
    public DbSet<InvestmentAccount> Accounts => Set<InvestmentAccount>();
    public DbSet<Order> Orders => Set<Order>();

    public DataContext(DbContextOptions<DataContext> options) : base(options)
    {
    }
}