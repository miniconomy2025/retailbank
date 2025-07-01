namespace RetailBank.Models.Dtos;

public record CreateLoanAccountRequest(
    ulong LoanAmountCents,
    ulong DebtorAccountNumber
);
