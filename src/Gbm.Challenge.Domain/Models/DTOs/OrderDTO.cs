using Gbm.Challenge.Domain.Common;
using Gbm.Challenge.Domain.Models.CustomConverters;
using System.Text.Json.Serialization;

namespace Gbm.Challenge.Domain.Models.DTOs;

public class OrderDTO
{
    // Added to convert UNIX timestamp into DateTime and viceversa
    [JsonConverter(typeof(DateTimeJsonConverter))]
    public DateTime Timestamp { get; set; }
    public OperationType Operation { get; set; }
    public string IssuerName { get; set; } = string.Empty;
    public int TotalShares { get; set; }
    public decimal SharePrice { get; set; }
}