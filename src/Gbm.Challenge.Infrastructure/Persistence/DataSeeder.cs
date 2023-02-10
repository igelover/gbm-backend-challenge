using Gbm.Challenge.Domain.Entities;

namespace Gbm.Challenge.Infrastructure.Persistence;

public static class DataSeeder
{
    public static void Seed(DataContext dbContext)
    {
        if (!dbContext.Clients.Any())
        {
            var testData = new List<Client>()
            {
                new Client()
                {
                    Name = "client1",
                    ApiKey = "apikey-ABC-123-XYZ-001"
                },
                new Client()
                {
                    Name = "client2",
                    ApiKey = "apikey-ABC-123-XYZ-002"
                },
                new Client()
                {
                    Name = "client3",
                    ApiKey = "apikey-ABC-123-XYZ-003"
                },
            };

            dbContext.Clients.AddRange(testData);
            dbContext.SaveChanges();
        }
    }
}