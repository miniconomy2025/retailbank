using RetailBank.Models;
using RetailBank.Models.Interbank;

namespace RetailBank.Services;

public interface IInterbankClient
{
    public Task<NotificationResult> TryNotify(BankId bank, string transactionId, ulong from, ulong to, UInt128 amount);
}
