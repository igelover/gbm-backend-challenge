using Gbm.Challenge.Domain.Common;

namespace Gbm.Challenge.Domain.Entities;

public class InvestmentAccount : EntityBase
{
    public decimal Cash { get; set; }
    public IList<Order> Orders { get; set; } = new List<Order>();

    public int GetStocksCount(string issuer)
    {
        return Orders.Count(o => o.IssuerName == issuer && o.Operation == OperationType.Buy) -
            Orders.Count(o => o.IssuerName == issuer && o.Operation == OperationType.Sell);
    }
}