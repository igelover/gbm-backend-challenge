namespace Gbm.Challenge.Domain.Entities;

public class Client : EntityBase
{
    public string Name { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
}