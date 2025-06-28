using System.Security.Cryptography;
using RetailBank.Models;
using TigerBeetle;
namespace RetailBank.Services;

public class TransactionService(Client tbClient) : ITransactionService
{
    public async Task<ulong> CreateSavingsAccount(ulong salaryCents)
    {
        var accountNumber = GenerateSavingsAccountNumber();
        var account = new Account
        {
            Id = accountNumber,
            UserData128 = 0,
            UserData64 = salaryCents,
            UserData32 = 0,
            Ledger = 1,
            Code = (ushort)AccountCode.Savings,
            Flags = AccountFlags.DebitsMustNotExceedCredits,
        };

        var accountResult = await tbClient.CreateAccountAsync(account);
        if (accountResult != CreateAccountResult.Ok)
        {
            throw new TigerBeetleResultException<CreateAccountResult>(accountResult);
        }

        return accountNumber;
    }

    public async Task<ulong> CreateLoanAccount()
    {
        var accountNumber = GenerateLoanAccountNumber();
        var account = new Account
        {
            Id = accountNumber,
            UserData128 = 0,
            UserData64 = 0,
            UserData32 = 0,
            Ledger = 1,
            Code = (ushort)AccountCode.Loan,
            Flags = AccountFlags.CreditsMustNotExceedDebits,
        };

        var accountResult = await tbClient.CreateAccountAsync(account);
        if (accountResult != CreateAccountResult.Ok)
        {
            throw new TigerBeetleResultException<CreateAccountResult>(accountResult);
        }

        return accountNumber;
    }

    public async Task<Account?> GetAccount(ulong accountId)
    {
        return await tbClient.LookupAccountAsync(accountId);
    }

    public async Task<Transfer[]> GetAccountTransfers(ulong accountId)
    {
        var filter = new AccountFilter();
        filter.AccountId = accountId;
        return await tbClient.GetAccountTransfersAsync(filter);
    }

    public async Task ExternalTransfer(ulong fromAccountId, string externalAccountId, UInt128 amount)
    {
        var fromAccount = await tbClient.LookupAccountAsync(fromAccountId) ?? throw new AccountNotFoundException(fromAccountId);

        if (fromAccount.Code != 2000)
        {
            throw new InvalidAccountException();
        }

        var pendingTransfer =
            new Transfer
            {
                Id = ID.Create(),
                DebitAccountId = (ushort)BankCode.Commercial,
                CreditAccountId = fromAccountId,
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
                Amount = Transfer.AmountMax,
                PendingId = pendingTransfer.Id,
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

    public async Task InternalTransfer(ulong fromAccountId, ulong toAccountId, UInt128 amount)
    {
        var fromAccount = await tbClient.LookupAccountAsync(fromAccountId) ?? throw new AccountNotFoundException(fromAccountId);
        var toAccount = await tbClient.LookupAccountAsync(toAccountId) ?? throw new AccountNotFoundException(toAccountId);
        if (fromAccount.Code != 2000 || toAccount.Code != 2000)
        {
            throw new InvalidAccountException();
        }
        var transfer =
            new Transfer
            {
                Id = ID.Create(),
                DebitAccountId = toAccountId,
                CreditAccountId = fromAccountId,
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

    private static async Task<bool> TryExternalTransfer(string externalAccountId, UInt128 amount)
    {
        // Todo: This function should attempt to call the endpoint provided by the commercial bank to initiate the transfer of 
        // funds on their end. This should try multiple times and the endpoint should be idempotent
        await Task.Delay(1000);
        return true;
    }

    // 12 digits starting with "1000"
    private static ulong GenerateSavingsAccountNumber()
    {
        var number = 1000_0000_0000ul + (ulong)RandomNumberGenerator.GetInt32(1_0000_0000);
        return number;
    }

    // 13 digits starting with "1000"
    private static ulong GenerateLoanAccountNumber()
    {
        var number = 1_0000_0000_0000ul + (ulong)RandomNumberGenerator.GetInt32(10_0000_0000);
        return number;
    }
}