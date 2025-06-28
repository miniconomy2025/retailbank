namespace RetailBank.Models.Dtos;

public record CreateAccountRequest(ulong SalaryCents, CreateAccountType AccountType);
