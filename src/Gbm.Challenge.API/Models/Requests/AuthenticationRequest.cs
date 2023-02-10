namespace Gbm.Challenge.API.Models.Requests;

public class AuthenticationRequest
{
    public string ClientName { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
}