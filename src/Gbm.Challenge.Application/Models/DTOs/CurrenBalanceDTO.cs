namespace Gbm.Challenge.Application.Models.DTOs;

public class CurrentBalanceDTO
{
    public decimal Cash { get; set; }
    public List<IssuerDTO> Issuers { get; set; } = new List<IssuerDTO>();

    public List<string> BusinessErrors { get; set; } = new List<string>();
}