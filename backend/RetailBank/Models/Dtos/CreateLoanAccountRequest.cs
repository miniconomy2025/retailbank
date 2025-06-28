namespace RetailBank.Models.Dtos;

public record CreateLoanAccountRequest(
    ulong LoanAmount,
    ulong UserAccountNumber
);
