using System.Security.Cryptography;
using RetailBank.Models;
using RetailBank.Repositories;
using TigerBeetle;
namespace RetailBank.Services;

public class TransactionService(Client tbClient, ILedgerRepository ledgerRepository) : ITransactionService
{

    public async Task Transfer(ulong payerAccountId, ulong payeeAccountId, UInt128 amount)
    {
        var payerBankAccount = await tbClient.LookupAccountAsync(payerAccountId) ?? throw new AccountNotFoundException(payerAccountId);
        if (payerBankAccount.Code != (int)AccountCode.Savings)
        {
            throw new InvalidAccountException();
        }
        var payeeBankCode = GetBankCode(payeeAccountId);
        if (payeeBankCode == (int)BankCode.Retail)
        {
            var payeeBankAccount = await tbClient.LookupAccountAsync(payeeAccountId) ?? throw new AccountNotFoundException(payeeAccountId);
            if (payeeBankAccount.Code != (int)AccountCode.Savings) throw new InvalidAccountException();
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
        var pendingTransfer =
            new Transfer
            {
                Id = ID.Create(),
                DebitAccountId = payerAccount.Id,
                CreditAccountId = (ushort)BankCode.Commercial,
                UserData64 = externalAccountId,
                Amount = amount,
                Ledger = 1,
                Flags = TransferFlags.Pending,
                Code = 1,
            };

        var pendingTransferResult = await tbClient.CreateTransferAsync(pendingTransfer);

        if (pendingTransferResult != CreateTransferResult.Ok)
        {
            throw new TigerBeetleResultException<CreateTransferResult>(pendingTransferResult);
        }

        if (!await TryExternalTransfer(externalAccountId, amount))
        {
            // external transfer has failed, void the pending tranfer
            var cancelTransfer = new Transfer
            {
                Id = ID.Create(),
                PendingId = pendingTransfer.Id,
                Flags = TransferFlags.VoidPendingTransfer,
                UserData64 = externalAccountId,
                Code = 1,
            };

            var cancelTransferResult = await tbClient.CreateTransferAsync(cancelTransfer);
            if (cancelTransferResult != CreateTransferResult.Ok)
            {
                throw new TigerBeetleResultException<CreateTransferResult>(cancelTransferResult);
            }

            throw new ExternalTransferFailedException();
        }
        else
        {
            var postTransfer = new Transfer
            {
                Id = ID.Create(),
                Amount = TigerBeetle.Transfer.AmountMax,
                PendingId = pendingTransfer.Id,
                UserData64 = externalAccountId,
                Flags = TransferFlags.PostPendingTransfer,
                Code = 1,
            };
            var postTransferResult = tbClient.CreateTransfer(postTransfer);
            if (postTransferResult != CreateTransferResult.Ok)
            {
                throw new TigerBeetleResultException<CreateTransferResult>(postTransferResult);
            }
        }
    }

    private async Task InternalTransfer(Account payerAccount, Account payeeAccount, UInt128 amount)
    {
        var transfer =
            new Transfer
            {
                Id = ID.Create(),
                DebitAccountId = payerAccount.Id,
                CreditAccountId = payeeAccount.Id,
                Amount = amount,
                Code = 1,
                Ledger = 1,
            };

        var transferResult = await tbClient.CreateTransferAsync(transfer);
        if (transferResult != CreateTransferResult.Ok)
        {
            throw new TigerBeetleResultException<CreateTransferResult>(transferResult);
        }
    }

    private static async Task<bool> TryExternalTransfer(ulong externalAccountId, UInt128 amount)
    {
        // Todo: This function should attempt to call the endpoint provided by the commercial bank to initiate the transfer of 
        // funds on their end. This should try multiple times and the endpoint should be idempotent
        await Task.Delay(1000);
        return true;
    }

    private static ushort GetBankCode(ulong accountNo) => (UInt16)(accountNo / 100000000);
}