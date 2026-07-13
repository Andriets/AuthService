using AuthService.Web.Core.Interfaces;
using FluentValidation;

namespace AuthService.Web.Features.Auth.Activate;

public class ActivateValidator : AbstractValidator<ActivateRequest>
{
    public ActivateValidator(IMessageService messages, IPasswordService passwordService)
    {
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage(messages.FieldRequired("Token"));

        RuleFor(x => x.Username)
            .NotEmpty().WithMessage(messages.FieldRequired("Username"))
            .MaximumLength(30).WithMessage(messages.FieldMaxLength("Username", 30))
            .Matches("^[a-zA-Z0-9]+$").WithMessage(messages.FieldInvalidFormat("Username"));

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage(messages.FieldRequired("Password"))
            .Must(passwordService.MeetsComplexity).WithMessage(messages.PasswordComplexityError());
    }
}
