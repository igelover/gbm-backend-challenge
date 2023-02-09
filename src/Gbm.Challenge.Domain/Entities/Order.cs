using Gbm.Challenge.Domain.Common;

namespace Gbm.Challenge.Domain.Entities;

public class Order : EntityBase
{
    public int AccountId { get; set; }
    public InvestmentAccount Account { get; set; } = new InvestmentAccount();
    public DateTime Timestamp { get; set; }
    public OperationType Operation { get; set; }
    public string IssuerName { get; set; } = string.Empty;
    public int TotalShares { get; set; }
    public decimal SharePrice { get; set; }

    public decimal TotalOrderAmount
    {
        get { return TotalShares * SharePrice; }
    }
}