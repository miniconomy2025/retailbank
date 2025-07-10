using FluentValidation;
using RetailBank.Models.Dtos;

namespace RetailBank.Validation;

public class CreateTransferRequestValidator : AbstractValidator<CreateTransferRequest>
{
    public CreateTransferRequestValidator()
    {
        RuleFor(req => req.From)
            .Matches(ValidationConstants.TransactionalAccountNumber)
            .WithMessage("'From' account number is not a valid transactional account number.");
        RuleFor(req => req.To)
            .Matches(ValidationConstants.TransactionalAccountNumber)
            .WithMessage("'To' account number is not a valid transactional account number.");
        RuleFor(req => req.AmountCents)
            .NotEmpty()
            .WithMessage("Cannot transfer an amount of 0 cents.");
    }
}
