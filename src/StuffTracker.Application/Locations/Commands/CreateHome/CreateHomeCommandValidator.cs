using System;
using System.Collections.Generic;
using System.Text;
using FluentValidation;

namespace StuffTracker.Application.Locations.Commands.CreateHome;

public class CreateHomeCommandValidator : AbstractValidator<CreateHomeCommand>
{
    public CreateHomeCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100).WithMessage("Name cannot exceed 100 characters.");
        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters.");
        //TODO: Can I make a constant so the max length is not hard coded in multiple places? These max lenghts rules are also in the LocationConfiguration class in the infrastructure layer.
        //TODO: Maybe I can create a static class with constants for these max lengths and use them in both places?
    }
}
