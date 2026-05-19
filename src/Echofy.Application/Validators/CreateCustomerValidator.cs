using Echofy.Application.DTOs;
using FluentValidation;

namespace Echofy.Application.Validators;

public class CreateCustomerValidator : AbstractValidator<CustomerDto>
{
    public CreateCustomerValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.Phone).MaximumLength(50);
        RuleFor(x => x.Country).MaximumLength(100);
    }
}
