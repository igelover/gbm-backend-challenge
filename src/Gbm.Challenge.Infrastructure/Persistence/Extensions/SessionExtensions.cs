using Gbm.Challenge.Domain.Identity;
using Microsoft.EntityFrameworkCore;

namespace Gbm.Challenge.Infrastructure.Persistance.Extensions;

public static class SessionExtensions
{
    public static async Task<IQueryable<Session>> DeactivateSessions(this IQueryable<Session> sessions, string clientName)
    {
        var activeSessions = sessions.Where(s => s.ClientName == clientName && s.Active);
        await foreach (var session in activeSessions.AsAsyncEnumerable())
        {
            session.Active = false;
        }

        return sessions;
    }
}