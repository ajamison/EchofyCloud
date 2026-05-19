using Echofy.Application.DTOs;
using FluentValidation;

namespace Echofy.Application.Validators;

public class CreateLeadValidator : AbstractValidator<LeadDto>
{
    public CreateLeadValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.EstimatedValue).GreaterThanOrEqualTo(0);
    }
}
