using AuthService.Web.Core.Interfaces;
using FluentValidation;

namespace AuthService.Web.Features.Auth.Register;

public class RegisterValidator : AbstractValidator<RegisterRequest>
{
    public RegisterValidator(IMessageService messages, IPasswordService passwordService)
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage(messages.FieldRequired("Username"))
            .MaximumLength(30).WithMessage(messages.FieldMaxLength("Username", 30))
            .Matches("^[a-zA-Z0-9]+$").WithMessage(messages.FieldInvalidFormat("Username"));

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage(messages.FieldRequired("Email"))
            .MaximumLength(255).WithMessage(messages.FieldMaxLength("Email", 255))
            .EmailAddress().WithMessage(messages.FieldInvalidEmail("Email"));

        RuleFor(x => x.FirstName)
            .MaximumLength(100).WithMessage(messages.FieldMaxLength("First name", 100));

        RuleFor(x => x.LastName)
            .MaximumLength(100).WithMessage(messages.FieldMaxLength("Last name", 100));

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage(messages.FieldRequired("Password"))
            .Must(passwordService.MeetsComplexity).WithMessage(messages.PasswordComplexityError());

        RuleFor(x => x.OrganizationName)
            .NotEmpty().WithMessage(messages.FieldRequired("Organization name"))
            .MaximumLength(200).WithMessage(messages.FieldMaxLength("Organization name", 200));
    }
}
