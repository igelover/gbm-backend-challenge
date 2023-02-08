namespace Gbm.Challenge.Domain.Entities;

public class Order : EntityBase
{
    public long Timestamp { get; set; }
    public string IssuerName { get; set; } = string.Empty;
    public int TotalShares { get; set; }
    public decimal SharePrice { get; set; }
}