using Gbm.Challenge.Application.Contracts.Persistence;
using Gbm.Challenge.Domain.Entities;
using Gbm.Challenge.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Gbm.Challenge.Infrastructure.Repositories;

public class ClientRepository : RepositoryBase<Client>, IClientRepository
{
    public ClientRepository(DataContext dbContext) : base(dbContext)
    {
    }

    public async Task<Client?> GetByNameAndKey(string name, string key)
    {
        return await _dbContext.Clients.FirstOrDefaultAsync(c => c.Name == name && c.ApiKey == key);
    }
}