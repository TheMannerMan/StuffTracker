using FluentValidation;
using StuffTracker.Domain.Constants;


namespace StuffTracker.Application.Locations.Commands.CreateHome;

public class CreateHomeCommandValidator : AbstractValidator<CreateHomeCommand>
{
    public CreateHomeCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(LocationConstants.LocationNameMaxLength).WithMessage($"Name cannot exceed {LocationConstants.LocationNameMaxLength} characters.");
        RuleFor(x => x.Description)
            .MaximumLength(LocationConstants.LocationDescriptionMaxLength).WithMessage($"Description cannot exceed {LocationConstants.LocationDescriptionMaxLength} characters.");
    }
}
