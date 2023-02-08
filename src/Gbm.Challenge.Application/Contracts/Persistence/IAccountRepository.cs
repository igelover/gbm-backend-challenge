using Gbm.Challenge.Domain.Entities;

namespace Gbm.Challenge.Application.Contracts.Persistence;

public interface IAccountRepository : IAsyncRepository<InvestmentAccount>
{
}