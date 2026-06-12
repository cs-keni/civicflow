using CivicFlow.Application.DTOs;
using FluentValidation;

namespace CivicFlow.Application.Validators;

public class CreateInspectionValidator : AbstractValidator<CreateInspectionRequest>
{
    public CreateInspectionValidator()
    {
        RuleFor(x => x.PermitApplicationId).GreaterThan(0);
        RuleFor(x => x.FacilityId).GreaterThan(0);
        RuleFor(x => x.InspectorId).NotEmpty();
        RuleFor(x => x.ScheduledDate).GreaterThan(DateTime.UtcNow.AddHours(-1))
            .WithMessage("ScheduledDate must be in the future");
    }
}

public class CompleteInspectionValidator : AbstractValidator<CompleteInspectionRequest>
{
    public CompleteInspectionValidator()
    {
        RuleFor(x => x.FieldNotes).NotEmpty().MinimumLength(20).MaximumLength(8000);
        RuleFor(x => x.CompletedDate).LessThanOrEqualTo(DateTime.UtcNow.AddMinutes(5))
            .WithMessage("CompletedDate cannot be in the future");
    }
}
