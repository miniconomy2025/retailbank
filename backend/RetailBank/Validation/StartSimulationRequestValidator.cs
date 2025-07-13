using FluentValidation;
using RetailBank.Models.Dtos;

namespace RetailBank.Validation;

public class StartSimulationRequestValidator : AbstractValidator<StartSimulationRequest>
{
    public StartSimulationRequestValidator()
    {
        RuleFor(req => req.EpochStartTime)
            .GreaterThan(1ul)
            .WithMessage("Epoch start time must be positive!");
    }
}
