﻿namespace RetailBank.Models.Dtos;

public record CreateLoanAccountRequest(
    ulong LoanAmountCents,
    string DebtorAccountId
);
