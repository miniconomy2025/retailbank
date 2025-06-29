using RetailBank.Models;
using RetailBank.Repositories;
using TigerBeetle;

namespace RetailBank.Services;

public class TransactionService(Client tbClient, ILedgerRepository ledgerRepository) : ITransactionService
{
    public async Task Transfer(ulong payerAccountId, ulong payeeAccountId, UInt128 amount)
    {
        var payerBankAccount = await tbClient.LookupAccountAsync(payerAccountId) ?? throw new AccountNotFoundException(payerAccountId);
        
        if (payerBankAccount.Code != (int)LedgerAccountCode.Transactional)
            throw new InvalidAccountException();
        
        var payeeBankCode = GetBankCode(payeeAccountId);
        
        if (payeeBankCode == (int)BankCode.Retail)
        {
            var payeeBankAccount = await tbClient.LookupAccountAsync(payeeAccountId) ?? throw new AccountNotFoundException(payeeAccountId);
            
            if (payeeBankAccount.Code != (int)LedgerAccountCode.Transactional) throw new InvalidAccountException();
            
            await InternalTransfer(payerBankAccount, payeeBankAccount, amount);
            
            return;
        }
        
        await ExternalTransfer(payerBankAccount, payeeAccountId, amount);
    }

    public async Task PaySalary(Account account)
    {
        var salary = account.UserData64;
        await ledgerRepository.Transfer(ID.Create(), (ushort)BankCode.Retail, (ulong)account.Id, salary);
    }

    private async Task ExternalTransfer(Account payerAccount, ulong externalAccountId, UInt128 amount)
    {
        var pendingTransferID = ID.Create();
        await ledgerRepository.Transfer(pendingTransferID, (ulong)payerAccount.Id, (ushort)BankCode.Commercial, amount, transferFlags: TransferFlags.Pending, userData64: externalAccountId);
      
        if (!await TryExternalTransfer(externalAccountId, amount))
        {
            // external transfer has failed, void the pending tranfer
            await ledgerRepository.Transfer(ID.Create(), 0, 0, 0, pendingId:pendingTransferID, transferFlags: TransferFlags.VoidPendingTransfer, userData64: externalAccountId);
            throw new ExternalTransferFailedException();
        }
        else
        {
            await ledgerRepository.Transfer(ID.Create(), 0, 0, TigerBeetle.Transfer.AmountMax, pendingId: pendingTransferID, userData64: externalAccountId, transferFlags: TransferFlags.PostPendingTransfer);
        }
    }

    private async Task InternalTransfer(Account payerAccount, Account payeeAccount, UInt128 amount)
    {
        await ledgerRepository.Transfer(ID.Create(), (ulong)payerAccount.Id, (ulong)payeeAccount.Id, amount);
    }

    private static async Task<bool> TryExternalTransfer(ulong externalAccountId, UInt128 amount)
    {
        // Todo: This function should attempt to call the endpoint provided by the commercial bank to initiate the transfer of 
        // funds on their end. This should try multiple times and the endpoint should be idempotent
        await Task.Delay(1000);
        return true;
    }

    private static ushort GetBankCode(ulong accountNumber) => (ushort)(accountNumber / 1_0000_0000);
}