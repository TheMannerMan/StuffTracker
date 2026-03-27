using FluentValidation;

using StuffTracker.Domain.Constants;


namespace StuffTracker.Application.Locations.Commands.CreateLocation;

public class CreateLocationCommandValidator : AbstractValidator<CreateLocationCommand>
{
    public CreateLocationCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(LocationConstants.LocationNameMaxLength).WithMessage($"Name cannot exceed {LocationConstants.LocationNameMaxLength} characters.");

        RuleFor(x => x.Description)
            .MaximumLength(LocationConstants.LocationDescriptionMaxLength).WithMessage($"Description cannot exceed {LocationConstants.LocationDescriptionMaxLength} characters.");

        RuleFor(x => x.LocationType)
            .Must(LocationConstants.ValidLocationTypesToBeCreated.Contains)
            .WithMessage("LocationType must be 'Room' or 'Storage'.");

        RuleFor(x => x.ParentId)
            .NotEmpty().WithMessage("ParentId is required.");
    }
}
