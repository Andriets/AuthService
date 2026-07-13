using AuthService.Web.Core.Interfaces;
using FluentValidation;

namespace AuthService.Web.Features.Auth.ResetPassword;

public class ResetPasswordValidator : AbstractValidator<ResetPasswordRequest>
{
    public ResetPasswordValidator(IMessageService messages, IPasswordService passwordService)
    {
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage(messages.FieldRequired("Token"));

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage(messages.FieldRequired("New password"))
            .Must(passwordService.MeetsComplexity).WithMessage(messages.PasswordComplexityError());
    }
}
