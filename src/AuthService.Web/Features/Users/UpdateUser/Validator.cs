using AuthService.Web.Core.Interfaces;
using FluentValidation;

namespace AuthService.Web.Features.Users.UpdateUser;

public class UpdateUserValidator : AbstractValidator<UpdateUserRequest>
{
    public UpdateUserValidator(IMessageService messages)
    {
        When(x => x.Email.HasValue, () =>
            RuleFor(x => x.Email.Value!)
                .NotEmpty().WithMessage(messages.FieldRequired("Email"))
                .MaximumLength(255).WithMessage(messages.FieldMaxLength("Email", 255))
                .EmailAddress().WithMessage(messages.FieldInvalidEmail("Email")));

        When(x => x.FirstName.HasValue, () =>
            RuleFor(x => x.FirstName.Value)
                .MaximumLength(100).WithMessage(messages.FieldMaxLength("First name", 100)));

        When(x => x.LastName.HasValue, () =>
            RuleFor(x => x.LastName.Value)
                .MaximumLength(100).WithMessage(messages.FieldMaxLength("Last name", 100)));
    }
}
