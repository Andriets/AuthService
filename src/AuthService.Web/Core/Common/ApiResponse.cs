namespace AuthService.Web.Core.Common;

public record ApiResponse<T>(T Data, DateTimeOffset Timestamp);
