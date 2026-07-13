using AuthService.Web.Core.Interfaces;
using FluentValidation;

namespace AuthService.Web.Features.Users.InviteUser;

public class InviteUserValidator : AbstractValidator<InviteUserRequest>
{
    public InviteUserValidator(IMessageService messages)
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage(messages.FieldRequired("Email"))
            .MaximumLength(255).WithMessage(messages.FieldMaxLength("Email", 255))
            .EmailAddress().WithMessage(messages.FieldInvalidEmail("Email"));

        RuleFor(x => x.FirstName)
            .MaximumLength(100).WithMessage(messages.FieldMaxLength("First name", 100));

        RuleFor(x => x.LastName)
            .MaximumLength(100).WithMessage(messages.FieldMaxLength("Last name", 100));
    }
}
