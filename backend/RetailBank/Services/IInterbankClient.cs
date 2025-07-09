using RetailBank.Models.Interbank;
using RetailBank.Models.Ledger;

namespace RetailBank.Services;

public interface IInterbankClient
{
    Task<NotificationResult> TryNotify(BankId bank, UInt128 transactionId, UInt128 from, UInt128 to, UInt128 amount, ulong? reference);
}
