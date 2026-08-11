using FluentValidation;
using HouseOfNaksh.Api.Contracts;

namespace HouseOfNaksh.Api.Validators;

public class CreateProductRequestValidator : AbstractValidator<CreateProductRequest>
{
    public CreateProductRequestValidator()
    {
        RuleFor(x => x.Sku)
            .NotEmpty().WithMessage("SKU is required")
            .MaximumLength(50)
            .Matches("^[A-Z0-9-]+$")
            .WithMessage("SKU must contain only uppercase letters, digits and hyphens");

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Description)
            .MaximumLength(2000);

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be greater than zero")
            .LessThanOrEqualTo(10_000_000);

        RuleFor(x => x.StockQuantity)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.Category)
            .MaximumLength(100);
    }
}
