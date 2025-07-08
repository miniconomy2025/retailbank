using RetailBank.Models.Ledger;
namespace RetailBank.Models.Dtos;

public record Report(uint TransactionalAccounts, uint LoanAccounts, Int128 BankBalance, UInt128 TotalMoney);    