using System.Globalization;
using System.Resources;
using AuthService.Web.Core.Interfaces;

namespace AuthService.Web.Core.Services;

public class MessageService : IMessageService
{
    private static readonly ResourceManager _rm =
        new("AuthService.Web.Resources.Messages", typeof(MessageService).Assembly);

    public string FieldRequired(string fieldName) =>
        string.Format(CultureInfo.CurrentCulture, _rm.GetString("Field_Required", CultureInfo.CurrentCulture)!, fieldName);

    public string FieldMaxLength(string fieldName, int maxLength) =>
        string.Format(CultureInfo.CurrentCulture, _rm.GetString("Field_MaxLength", CultureInfo.CurrentCulture)!, fieldName, maxLength);

    public string FieldInvalidEmail(string fieldName) =>
        string.Format(CultureInfo.CurrentCulture, _rm.GetString("Field_InvalidEmail", CultureInfo.CurrentCulture)!, fieldName);

    public string ResourceAlreadyExists(string resource, string field) =>
        string.Format(CultureInfo.CurrentCulture, _rm.GetString("Resource_AlreadyExists", CultureInfo.CurrentCulture)!, resource, field);

    public string ResourceAlreadyExistsInOrganization(string resource, string field) =>
        string.Format(CultureInfo.CurrentCulture, _rm.GetString("Resource_AlreadyExistsInOrganization", CultureInfo.CurrentCulture)!, resource, field);

    public string FieldInvalidFormat(string fieldName) =>
        string.Format(CultureInfo.CurrentCulture, _rm.GetString("Field_InvalidFormat", CultureInfo.CurrentCulture)!, fieldName);

    public string PasswordComplexityError() =>
        _rm.GetString("Password_Complexity", CultureInfo.CurrentCulture)!;

    public string PasswordRecentlyUsed() =>
        _rm.GetString("Password_RecentlyUsed", CultureInfo.CurrentCulture)!;

    public string ErrorBadRequest() =>
        _rm.GetString("Error_BadRequest", CultureInfo.CurrentCulture)!;

    public string ErrorValidationFailed() =>
        _rm.GetString("Error_ValidationFailed", CultureInfo.CurrentCulture)!;

    public string ErrorInternalServer() =>
        _rm.GetString("Error_InternalServer", CultureInfo.CurrentCulture)!;

    public string AuthInvalidCredentials() =>
        _rm.GetString("Auth_InvalidCredentials", CultureInfo.CurrentCulture)!;

    public string AuthAccountLocked() =>
        _rm.GetString("Auth_AccountLocked", CultureInfo.CurrentCulture)!;

    public string AuthAccountNotActivated() =>
        _rm.GetString("Auth_AccountNotActivated", CultureInfo.CurrentCulture)!;

    public string AuthAccountDisabled() =>
        _rm.GetString("Auth_AccountDisabled", CultureInfo.CurrentCulture)!;

    public string AuthInvalidRefreshToken() =>
        _rm.GetString("Auth_InvalidRefreshToken", CultureInfo.CurrentCulture)!;

    public string AuthRefreshTokenExpired() =>
        _rm.GetString("Auth_RefreshTokenExpired", CultureInfo.CurrentCulture)!;

    public string AuthRefreshTokenNotFound() =>
        _rm.GetString("Auth_RefreshTokenNotFound", CultureInfo.CurrentCulture)!;

    public string AuthInvalidResetToken() =>
        _rm.GetString("Auth_InvalidResetToken", CultureInfo.CurrentCulture)!;

    public string AuthPasswordResetSuccess() =>
        _rm.GetString("Auth_PasswordResetSuccess", CultureInfo.CurrentCulture)!;

    public string AuthInvalidInvitationToken() =>
        _rm.GetString("Auth_InvalidInvitationToken", CultureInfo.CurrentCulture)!;

    public string AuthAccountActivatedSuccess() =>
        _rm.GetString("Auth_AccountActivatedSuccess", CultureInfo.CurrentCulture)!;

    public string AuthPasswordResetLinkSent() =>
        _rm.GetString("Auth_PasswordResetLinkSent", CultureInfo.CurrentCulture)!;

    public string AuthUsernameAlreadyTaken(string username) =>
        string.Format(CultureInfo.CurrentCulture, _rm.GetString("Auth_UsernameAlreadyTaken", CultureInfo.CurrentCulture)!, username);

    public string UserCannotDeleteOwnAccount() =>
        _rm.GetString("User_CannotDeleteOwnAccount", CultureInfo.CurrentCulture)!;
}
