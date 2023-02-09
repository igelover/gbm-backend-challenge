using Gbm.Challenge.Application.Contracts.Persistence;
using Gbm.Challenge.Domain.Entities;
using Gbm.Challenge.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Gbm.Challenge.Infrastructure.Repositories;

public class AccountRepository : RepositoryBase<InvestmentAccount>, IAccountRepository
{
    public AccountRepository(DataContext dbContext) : base(dbContext)
    {
    }

    public async Task<InvestmentAccount?> GetByIdAsync(int id, bool includeOrders)
    {
        if (!includeOrders)
        {
            return await GetByIdAsync(id);
        }

        return await _dbContext.Accounts
                               .Include(a => a.Orders)
                               .FirstOrDefaultAsync(a => a.Id == id);
    }
}