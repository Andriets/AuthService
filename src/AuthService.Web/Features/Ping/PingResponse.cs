namespace AuthService.Web.Features.Ping;

public record PingResponse(PingStatus Status, DateTimeOffset Timestamp, string DatabaseServer, string DatabaseName);
