using Microsoft.Extensions.Logging;

namespace AuthService.Web.Features.Users;

public static partial class UsersEndpointLogs
{
    [LoggerMessage(Level = LogLevel.Information, Message = "User {UserId} invited by {InvitedByUserId}")]
    public static partial void UserInvited(this ILogger logger, Guid userId, Guid invitedByUserId);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Invite rejected by {InvitedByUserId}: email already exists in organization")]
    public static partial void InviteConflict(this ILogger logger, Guid invitedByUserId);

    [LoggerMessage(Level = LogLevel.Information, Message = "User {UserId} updated by {UpdatedByUserId}")]
    public static partial void UserUpdated(this ILogger logger, Guid userId, Guid updatedByUserId);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Update of user {UserId} by {UpdatedByUserId} rejected: email already exists in organization")]
    public static partial void UpdateConflict(this ILogger logger, Guid userId, Guid updatedByUserId);

    [LoggerMessage(Level = LogLevel.Information, Message = "User {UserId} deleted by {DeletedByUserId}")]
    public static partial void UserDeleted(this ILogger logger, Guid userId, Guid deletedByUserId);

    [LoggerMessage(Level = LogLevel.Warning, Message = "User {UserId} blocked from deleting their own account")]
    public static partial void DeleteBlockedSelfDelete(this ILogger logger, Guid userId);

    [LoggerMessage(Level = LogLevel.Information, Message = "Password reset triggered for user {TargetUserId} by {TriggeredByUserId}")]
    public static partial void AdminPasswordResetTriggered(this ILogger logger, Guid targetUserId, Guid triggeredByUserId);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Listed {Count} users for tenant {TenantId}")]
    public static partial void UsersListed(this ILogger logger, Guid tenantId, int count);

    [LoggerMessage(Level = LogLevel.Debug, Message = "Listed {Count} users across all organizations")]
    public static partial void AllUsersListed(this ILogger logger, int count);
}
