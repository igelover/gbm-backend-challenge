using Gbm.Challenge.Application.Contracts.Persistence;
using Gbm.Challenge.Domain.Entities;
using Gbm.Challenge.Domain.Identity;
using Gbm.Challenge.Infrastructure.Persistance.Extensions;
using Gbm.Challenge.Infrastructure.Persistence;

namespace Gbm.Challenge.Infrastructure.Repositories;

public class SessionRepository : RepositoryBase<Session>, ISessionRepository
{
    public SessionRepository(DataContext dbContext) : base(dbContext)
    {
    }

    public async Task CreateSessionAsync(Client client, Session newSession)
    {
        await _dbContext.Sessions.DeactivateSessions(client.Name);
        await _dbContext.Sessions.AddAsync(newSession);
        await _dbContext.SaveChangesAsync();
    }

}