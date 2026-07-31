using System.Text.Json.Serialization;

namespace AuthService.Web.Features.Ping;

[JsonConverter(typeof(JsonStringEnumConverter<PingStatus>))]
public enum PingStatus
{
    Healthy,
    DatabaseUnavailable
}
