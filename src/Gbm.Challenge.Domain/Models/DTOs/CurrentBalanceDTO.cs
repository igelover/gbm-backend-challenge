namespace Gbm.Challenge.Domain.Models.DTOs;

public class CurrentBalanceDTO
{
    public decimal Cash { get; set; }
    public IEnumerable<IssuerDTO> Issuers { get; set; } = new List<IssuerDTO>();

    public IEnumerable<string> BusinessErrors { get; set; } = new List<string>();
}