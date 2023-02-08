namespace Gbm.Challenge.Application.Models.DTOs;

public class IssuerDTO
{
    public string IssuerName { get; set; } = string.Empty;
    public int TotalShares { get; set; }
    public decimal SharePrice { get; set; }
}