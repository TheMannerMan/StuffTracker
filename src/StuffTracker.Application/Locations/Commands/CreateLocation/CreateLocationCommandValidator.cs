using FluentValidation;

using StuffTracker.Domain.Constants;
using StuffTracker.Domain.Enums;
using StuffTracker.Domain.Repositories;


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
            .WithMessage("Invalid location type. Please choose from the valid location types.");

       RuleFor(x => x.ParentId)
       .NotEmpty().WithMessage("ParentId is required.");

    }
}
