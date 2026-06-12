using CivicFlow.Application.DTOs;
using FluentValidation;

namespace CivicFlow.Application.Validators;

public class CreateViolationValidator : AbstractValidator<CreateViolationRequest>
{
    public CreateViolationValidator()
    {
        RuleFor(x => x.InspectionId).GreaterThan(0);
        RuleFor(x => x.FacilityId).GreaterThan(0);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Description).NotEmpty().MinimumLength(20).MaximumLength(2000);
        RuleFor(x => x.RegulatoryBasis).MaximumLength(500).When(x => x.RegulatoryBasis is not null);
        RuleFor(x => x.DueDate).GreaterThan(DateTime.UtcNow).When(x => x.DueDate.HasValue);
    }
}

public class UpdateViolationStatusValidator : AbstractValidator<UpdateViolationStatusRequest>
{
    public UpdateViolationStatusValidator()
    {
        RuleFor(x => x.Notes).MaximumLength(2000).When(x => x.Notes is not null);
    }
}

public class LoginValidator : AbstractValidator<DTOs.LoginRequest>
{
    public LoginValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
    }
}
