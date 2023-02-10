using Gbm.Challenge.Domain.Entities;

namespace Gbm.Challenge.Application.Contracts.Persistence;

public interface IClientRepository : IAsyncRepository<Client>
{
    Task<Client?> GetByNameAndKey(string name, string key);
}