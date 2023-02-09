using Gbm.Challenge.Domain.Entities;

namespace Gbm.Challenge.Application.Contracts.Persistence;

public interface IAccountRepository : IAsyncRepository<InvestmentAccount>
{
    Task<InvestmentAccount?> GetByIdAsync(int id, bool includeOrders);
}