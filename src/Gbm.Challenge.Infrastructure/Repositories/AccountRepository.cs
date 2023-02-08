using Gbm.Challenge.Application.Contracts.Persistence;
using Gbm.Challenge.Domain.Entities;
using Gbm.Challenge.Infrastructure.Persistence;

namespace Gbm.Challenge.Infrastructure.Repositories;

public class AccountRepository : RepositoryBase<InvestmentAccount>, IAccountRepository
{
	public AccountRepository(DataContext dbContext) : base(dbContext)
	{
    }
}