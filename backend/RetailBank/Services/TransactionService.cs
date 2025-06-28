using System.Threading.Tasks;
using TigerBeetle;
namespace RetailBank.Services;

public class TransactionService(Client tbClient) : ITransactionService
{

    public async Task<UInt128> CreateAccount(UInt64 SalaryCents)
    {
        var account = new Account
        {
            Id = ID.Create(),
            UserData128 = 0,
            UserData64 = SalaryCents,
            UserData32 = 0,
            Ledger = 1,
            Code = 2000,
            Flags = AccountFlags.None,
        };

        var accountResult = await tbClient.CreateAccountAsync(account);
        if (accountResult != CreateAccountResult.Ok)
        {
            throw new TigerBeetleResultException<CreateAccountResult>(accountResult);
        }

        return account.Id;
    }

    public async Task ExternalTransfer(UInt128 fromAccountId, UInt128 ExternalAccountId, UInt128 amount)
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
                DebitAccountId = Constants.COMMERCIAL_BANK_ID,
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

        if (!await TryExternalTransfer(ExternalAccountId, amount))
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

    public async Task InternalTransfer(UInt128 fromAccountId, UInt128 toAccountId, UInt128 amount)
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

    private static async Task<bool> TryExternalTransfer(UInt128 ExternalAccountId, UInt128 amount)
    {
        // Todo: This function should attempt to call the endpoint provided by the commercial bank to initiate the transfer of 
        // funds on their end. This should try multiple times and the endpoint should be idempotent
        await Task.Delay(1000);
        return true;
    }
}