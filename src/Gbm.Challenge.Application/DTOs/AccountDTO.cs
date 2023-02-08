using Gbm.Challenge.Application.DTOs;

namespace Gbm.Challenge.Application.DTOs;

public class AccountDTO
{
    public int Id { get; set; }
    public decimal Cash { get; set; }
    public List<IssuerDTO> Issuers { get; set; } = new List<IssuerDTO>();
}