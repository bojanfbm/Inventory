using Api.Models.Request;
using FluentValidation;

namespace Api.Validators;

public class InventoryCreateRequestValidator : AbstractValidator<InventoryCreateRequest>
{
    public InventoryCreateRequestValidator()
    {
        RuleFor(x => x.InventoryId ).NotEmpty().WithMessage(x => $"The field {nameof(x.InventoryId)} is required.");
        RuleFor(x => x.InventoryId).Matches(@"^[0-9a-zA-Z ]+$").WithMessage(x => $"Only alphanumeric characters allowed for the field {nameof(x.InventoryId)}.");
        RuleFor(x => x.InventoryId).MaximumLength(32).WithMessage(x => $"Maximum length for the field {nameof(x.InventoryId)} is 32 characters.");

        RuleFor(x => x.Tags).NotEmpty().WithMessage(x => $"The field {nameof(x.Tags)} is required.");
        RuleFor(x => x.InventoryDate).NotEmpty().WithMessage(x => $"The field {nameof(x.InventoryDate)} is required.");
        RuleFor(x => x.InventoryLocation).NotEmpty().WithMessage(x => $"The field {nameof(x.InventoryLocation)} is required.");

            
    }
}