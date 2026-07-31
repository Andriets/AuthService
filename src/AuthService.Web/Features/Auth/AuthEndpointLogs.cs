using Microsoft.Extensions.Logging;

namespace AuthService.Web.Features.Auth;

public static partial class AuthEndpointLogs
{
    [LoggerMessage(Level = LogLevel.Information, Message = "User {UserId} registered a new organization {TenantId}")]
    public static partial void UserRegistered(this ILogger logger, Guid userId, Guid tenantId);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Registration rejected: username already taken")]
    public static partial void UsernameConflict(this ILogger logger);

    [LoggerMessage(Level = LogLevel.Information, Message = "User {UserId} signed in")]
    public static partial void UserSignedIn(this ILogger logger, Guid userId);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Sign-in failed: unknown username")]
    public static partial void SignInFailedUnknownUser(this ILogger logger);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Sign-in failed for user {UserId}: invalid password ({FailedAttempts} failed attempts)")]
    public static partial void SignInFailedInvalidPassword(this ILogger logger, Guid userId, int failedAttempts);

    [LoggerMessage(Level = LogLevel.Warning, Message = "User {UserId} account locked after too many failed sign-in attempts")]
    public static partial void AccountLocked(this ILogger logger, Guid userId);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Auth request blocked for user {UserId}: {Reason}")]
    public static partial void AuthBlockedByAccountState(this ILogger logger, Guid userId, string reason);

    [LoggerMessage(Level = LogLevel.Information, Message = "User {UserId} refreshed their token")]
    public static partial void TokenRefreshed(this ILogger logger, Guid userId);

    [LoggerMessage(Level = LogLevel.Warning, Message = "{TokenType} token rejected: {Reason}")]
    public static partial void TokenRejected(this ILogger logger, string tokenType, string reason);

    [LoggerMessage(Level = LogLevel.Warning, Message = "{TokenType} token rejected for user {UserId}: {Reason}")]
    public static partial void TokenRejectedForUser(this ILogger logger, Guid userId, string tokenType, string reason);

    [LoggerMessage(Level = LogLevel.Information, Message = "User {UserId} signed out")]
    public static partial void UserSignedOut(this ILogger logger, Guid userId);

    [LoggerMessage(Level = LogLevel.Information, Message = "User {UserId} activated their account")]
    public static partial void AccountActivated(this ILogger logger, Guid userId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Password reset requested for user {UserId}")]
    public static partial void PasswordResetRequested(this ILogger logger, Guid userId);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Password reset requested for an unknown username")]
    public static partial void PasswordResetRequestedForUnknownUser(this ILogger logger);

    [LoggerMessage(Level = LogLevel.Information, Message = "Password reset completed for user {UserId}")]
    public static partial void PasswordResetCompleted(this ILogger logger, Guid userId);
}
