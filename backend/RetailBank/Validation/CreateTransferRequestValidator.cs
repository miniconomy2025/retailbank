using FluentValidation;
using RetailBank.Models.Dtos;

namespace RetailBank.Validation;

public class CreateTransferRequestValidator : AbstractValidator<CreateTransferRequest>
{
    public CreateTransferRequestValidator()
    {
        RuleFor(req => req.From)
            .Matches(ValidationConstants.TransferFromAccountNumber)
            .WithMessage("'From' account number is not a valid account number.");
        RuleFor(req => req.To)
            .Matches(ValidationConstants.TransferToAccountNumber)
            .WithMessage("'To' account number is not a valid account number.");
        RuleFor(req => req.AmountCents)
            .NotEmpty()
            .WithMessage("Cannot transfer an amount of 0 cents.");
    }
}
