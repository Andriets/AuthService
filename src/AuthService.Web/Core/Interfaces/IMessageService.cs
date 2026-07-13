namespace AuthService.Web.Core.Interfaces;

public interface IMessageService
{
    string FieldRequired(string fieldName);
    string FieldMaxLength(string fieldName, int maxLength);
    string FieldInvalidEmail(string fieldName);
    string ResourceAlreadyExists(string resource, string field);
    string FieldInvalidFormat(string fieldName);
    string PasswordComplexityError();
    string PasswordRecentlyUsed();
    string ErrorBadRequest();
    string ErrorValidationFailed();
    string ErrorInternalServer();
}
