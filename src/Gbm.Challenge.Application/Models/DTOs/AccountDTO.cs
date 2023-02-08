namespace Gbm.Challenge.Application.Models.DTOs;

public class AccountDTO
{
    public int Id { get; set; }
    public decimal Cash { get; set; }
    public List<IssuerDTO> Issuers { get; set; } = new List<IssuerDTO>();
}