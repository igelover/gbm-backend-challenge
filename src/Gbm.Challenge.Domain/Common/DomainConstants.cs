namespace Gbm.Challenge.Domain.Common;

public static class DomainConstants
{
    public const string JwtScopeApiKey = "ApiKey";
    public const string JwtScopeAdminRole = "ChallengeApiAdmin";
    public const string JwtScopeUserRole = "ChallengeApiUser";

    public const string InsufficientBalance = "INSUFFICIENT_BALANCE";
    public const string InsufficientStocks = "INSUFFICIENT_STOCKS";
    public const string DuplicatedOperation = "DUPLICATED_OPERATION";
    public const string ClosedMarket = "CLOSED_MARKET";
    public const string InvalidOperation = "INVALID_OPERATION";
}