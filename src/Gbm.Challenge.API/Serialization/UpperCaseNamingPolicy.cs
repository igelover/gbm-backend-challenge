using System.Text.Json;

namespace Gbm.Challenge.API.Serialization;

public class UpperCaseNamingPolicy : JsonNamingPolicy
{
    public override string ConvertName(string name) => name.ToUpper();
}