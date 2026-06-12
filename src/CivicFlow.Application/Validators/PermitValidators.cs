using CivicFlow.Application.DTOs;
using FluentValidation;

namespace CivicFlow.Application.Validators;

public class CreatePermitApplicationValidator : AbstractValidator<CreatePermitApplicationRequest>
{
    public CreatePermitApplicationValidator()
    {
        RuleFor(x => x.FacilityId).GreaterThan(0);
        RuleFor(x => x.Description).NotEmpty().MinimumLength(20).MaximumLength(2000)
            .WithMessage("Description must be between 20 and 2000 characters");
        RuleFor(x => x.ProjectDetails).NotEmpty().MinimumLength(50).MaximumLength(4000)
            .WithMessage("Project details must be between 50 and 4000 characters");
        RuleFor(x => x.EstimatedCost).GreaterThanOrEqualTo(0).When(x => x.EstimatedCost.HasValue);
    }
}

public class UpdatePermitApplicationValidator : AbstractValidator<UpdatePermitApplicationRequest>
{
    public UpdatePermitApplicationValidator()
    {
        RuleFor(x => x.Description).MinimumLength(20).MaximumLength(2000).When(x => x.Description is not null);
        RuleFor(x => x.ProjectDetails).MinimumLength(50).MaximumLength(4000).When(x => x.ProjectDetails is not null);
        RuleFor(x => x.EstimatedCost).GreaterThanOrEqualTo(0).When(x => x.EstimatedCost.HasValue);
    }
}

public class CreateReviewCommentValidator : AbstractValidator<CreateReviewCommentRequest>
{
    public CreateReviewCommentValidator()
    {
        RuleFor(x => x.Content).NotEmpty().MinimumLength(5).MaximumLength(4000);
    }
}

public class ReviewActionValidator : AbstractValidator<ReviewActionRequest>
{
    public ReviewActionValidator()
    {
        RuleFor(x => x.Notes).MaximumLength(1000).When(x => x.Notes is not null);
    }
}
