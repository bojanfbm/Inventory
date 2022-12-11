using Api.Models.Request;
using FluentValidation;

namespace Api.Validators;

public class ItemCreateRequestValidator : AbstractValidator<ItemCreateRequest>
{
    public ItemCreateRequestValidator()
    {
        RuleFor(x => x.CompanyName ).NotEmpty().WithMessage(x => $"The field {nameof(x.CompanyName)} is required.");
        RuleFor(x => x.CompanyPrefix).NotEmpty().WithMessage(x => $"The field {nameof(x.CompanyPrefix)} is required.");
        RuleFor(x => x.ItemName).NotEmpty().WithMessage(x => $"The field {nameof(x.ItemName)} is required.");
        RuleFor(x => x.ItemReference).NotEmpty().WithMessage(x => $"The field {x.ItemReference} is required.");
    }
}