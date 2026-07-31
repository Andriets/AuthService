namespace AuthService.Web.Core.Interfaces;

public interface IMessageService
{
    string FieldRequired(string fieldName);
    string FieldMaxLength(string fieldName, int maxLength);
    string FieldInvalidEmail(string fieldName);
    string ResourceAlreadyExists(string resource, string field);
    string ResourceAlreadyExistsInOrganization(string resource, string field);
    string FieldInvalidFormat(string fieldName);
    string PasswordComplexityError();
    string PasswordRecentlyUsed();
    string ErrorBadRequest();
    string ErrorValidationFailed();
    string ErrorInternalServer();

    string AuthInvalidCredentials();
    string AuthAccountLocked();
    string AuthAccountNotActivated();
    string AuthAccountDisabled();
    string AuthInvalidRefreshToken();
    string AuthRefreshTokenExpired();
    string AuthRefreshTokenNotFound();
    string AuthInvalidResetToken();
    string AuthPasswordResetSuccess();
    string AuthInvalidInvitationToken();
    string AuthAccountActivatedSuccess();
    string AuthPasswordResetLinkSent();
    string AuthUsernameAlreadyTaken(string username);
    string UserCannotDeleteOwnAccount();
}
