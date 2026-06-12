using CivicFlow.Application.DTOs;
using FluentValidation;

namespace CivicFlow.Application.Validators;

public class CreateFacilityValidator : AbstractValidator<CreateFacilityRequest>
{
    public CreateFacilityValidator()
    {
        RuleFor(x => x.LegalName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.DbaName).MaximumLength(200).When(x => x.DbaName is not null);
        RuleFor(x => x.Address).NotEmpty().MaximumLength(300);
        RuleFor(x => x.City).NotEmpty().MaximumLength(100);
        RuleFor(x => x.State).NotEmpty().MaximumLength(50);
        RuleFor(x => x.ZipCode).NotEmpty().MaximumLength(20).Matches(@"^\d{5}(-\d{4})?$").WithMessage("ZipCode must be in 5-digit or ZIP+4 format");
        RuleFor(x => x.County).NotEmpty().MaximumLength(100);
    }
}

public class UpdateFacilityValidator : AbstractValidator<UpdateFacilityRequest>
{
    public UpdateFacilityValidator()
    {
        RuleFor(x => x.LegalName).MaximumLength(200).When(x => x.LegalName is not null);
        RuleFor(x => x.DbaName).MaximumLength(200).When(x => x.DbaName is not null);
        RuleFor(x => x.Address).MaximumLength(300).When(x => x.Address is not null);
        RuleFor(x => x.City).MaximumLength(100).When(x => x.City is not null);
        RuleFor(x => x.State).MaximumLength(50).When(x => x.State is not null);
        RuleFor(x => x.ZipCode).MaximumLength(20).Matches(@"^\d{5}(-\d{4})?$").When(x => x.ZipCode is not null);
        RuleFor(x => x.County).MaximumLength(100).When(x => x.County is not null);
    }
}
