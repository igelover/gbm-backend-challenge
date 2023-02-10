using Gbm.Challenge.Domain.Entities;

namespace Gbm.Challenge.Domain.Identity;

public class Session : EntityBase
{
    public Client Client { get; set; } = new();
    public string ClientName { get; set; } = string.Empty;
    public DateTimeOffset CreateDate { get; set; } = DateTimeOffset.Now;
    public string TokenId { get; set; } = string.Empty;
    public bool Active { get; set; } = true;
}