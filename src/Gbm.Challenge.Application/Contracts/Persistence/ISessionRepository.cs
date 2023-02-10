using Gbm.Challenge.Domain.Entities;
using Gbm.Challenge.Domain.Identity;

namespace Gbm.Challenge.Application.Contracts.Persistence;

public interface ISessionRepository : IAsyncRepository<Session>
{
    Task CreateSessionAsync(Client client, Session newSession);
}