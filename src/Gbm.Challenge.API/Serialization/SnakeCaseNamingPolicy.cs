using System.Text.Json;

namespace Gbm.Challenge.API.Serialization;

public class SnakeCaseNamingPolicy : JsonNamingPolicy
{
    public override string ConvertName(string name) => name.ToSnakeCase();
}