using Echofy.Application.DTOs;
using FluentValidation;

namespace Echofy.Application.Validators;

public class CreateProductValidator : AbstractValidator<ProductDto>
{
    public CreateProductValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(300);
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
        RuleFor(x => x.StockQuantity).GreaterThanOrEqualTo(0);
    }
}
