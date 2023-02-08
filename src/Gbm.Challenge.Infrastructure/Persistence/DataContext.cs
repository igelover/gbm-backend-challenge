using Gbm.Challenge.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Gbm.Challenge.Infrastructure.Persistence;

public class DataContext : DbContext
{
    public DbSet<InvestmentAccount> Accounts => Set<InvestmentAccount>();
    public DbSet<Order> Orders => Set<Order>();

    public DataContext(DbContextOptions<DataContext> options) : base(options)
    {
    }
}