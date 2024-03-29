﻿namespace Gbm.Challenge.Domain.Models.DTOs;

public class AccountDTO
{
    public int Id { get; set; }
    public decimal Cash { get; set; }
    public IEnumerable<IssuerDTO> Issuers { get; set; } = new List<IssuerDTO>();
}