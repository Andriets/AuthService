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
}
