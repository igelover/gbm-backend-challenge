using Gbm.Challenge.Domain.Timestamp;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Gbm.Challenge.Domain.Models.CustomConverters;

public class DateTimeJsonConverter : JsonConverter<DateTime>
{
    public override DateTime Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options) =>
            Epoch.FromUnix(reader.GetInt64());

    public override void Write(
        Utf8JsonWriter writer,
        DateTime dateTimeValue,
        JsonSerializerOptions options) =>
            writer.WriteStringValue(Epoch.ToUnix(dateTimeValue).ToString());
}