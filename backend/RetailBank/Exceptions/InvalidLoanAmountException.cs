namespace RetailBank.Exceptions;

public class InvalidLoanAmountException : UserException
{
    public InvalidLoanAmountException(ulong loanAmount) : base(
        StatusCodes.Status400BadRequest,
        "Invalid Loan Amount",
        $"Loan amount {loanAmount} is invalid. Loan amount must be greater than 0."
    ) { }
}
