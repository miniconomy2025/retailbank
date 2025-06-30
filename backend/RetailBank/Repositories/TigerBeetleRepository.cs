using RetailBank.Models;
using RetailBank.Models.Dtos;
using RetailBank.Services;
using TigerBeetle;

namespace RetailBank.Repositories;

public class TigerBeetleRepository(ITigerBeetleClientProvider tbClientProvider) : ILedgerRepository
{
    public const uint LedgerId = 1;
    public const ushort TransferCode = 1;

    public async Task CreateAccount(
        UInt128 accountId,
        LedgerAccountCode code,
        AccountFlags flags,
        UInt128 userData128,
        ulong userData64,
        uint userData32
    )
    {
        var result = await tbClientProvider.Client.CreateAccountAsync(new Account
        {
            Id = accountId,
            Code = (ushort)code,
            UserData128 = userData128,
            UserData64 = userData64,
            UserData32 = userData32,
            Ledger = LedgerId,
            Flags = flags,
        });

        if (result != CreateAccountResult.Ok)
            throw new TigerBeetleResultException<CreateAccountResult>(result);
    }

    public async Task<Account?> GetAccount(UInt128 accountId)
    {
        return await tbClientProvider.Client.LookupAccountAsync(accountId);
    }

    public async Task<Transfer[]> GetAccountTransfers(UInt128 id, uint limit, ulong timestampMax, TransferSide side)
    {
        var filter = new AccountFilter();
        filter.AccountId = id;
        filter.Limit = limit;
        filter.TimestampMax = timestampMax;
        filter.Flags = side.ToAccountFilterFlags() | AccountFilterFlags.Reversed;

        return await tbClientProvider.Client.GetAccountTransfersAsync(filter);
    }

    public async Task<IEnumerable<Account>> GetAccounts(LedgerAccountCode code)
    {
        var allAccounts = new List<Account>();
        ulong nextTimestamp = 0;
        const uint batchSize = 1000;

        while (true)
        {
            var filter = new QueryFilter
            {
                Limit = batchSize,
                TimestampMin = nextTimestamp,
                Code = (ushort)code
            };

            var accounts = await tbClientProvider.Client.QueryAccountsAsync(filter);

            if (accounts.Length == 0)
                break;

            allAccounts.AddRange(accounts);
            nextTimestamp = accounts.Max(a => a.Timestamp) + 1;

            if (accounts.Length < batchSize)
                break;
        }

        return allAccounts;
    }

    public async Task<Transfer[]> GetTransfers(uint limit, ulong timestampMax)
    {
        var filter = new QueryFilter();
        filter.Limit = limit;
        filter.TimestampMax = timestampMax;
        filter.Flags = QueryFilterFlags.Reversed;

        return await tbClientProvider.Client.QueryTransfersAsync(filter);
    }

    public async Task<Transfer?> GetTransfer(UInt128 id)
    {
        return await tbClientProvider.Client.LookupTransferAsync(id);
    }

    public async Task<UInt128> Transfer(LedgerTransfer simpleTransfer)
    {
        var transfer = simpleTransfer.ToTransfer();
        var transferResult = await tbClientProvider.Client.CreateTransferAsync(transfer);

        if (transferResult != CreateTransferResult.Ok)
            throw new TigerBeetleResultException<CreateTransferResult>(transferResult);

        return transfer.Id;
    }

    public async Task<IEnumerable<UInt128>> TransferLinked(IEnumerable<LedgerTransfer> simpleTransfers)
    {
        var length = simpleTransfers.Count();
        var transfers = simpleTransfers.Select((transfer, index) => transfer.ToTransfer(
            index < length - 1 ? TransferFlags.Linked : TransferFlags.None
        ));

        await TransferBatch(transfers.ToArray());

        return transfers.Select(transfer => transfer.Id);
    }

    public async Task<UInt128> StartTransfer(LedgerTransfer simpleTransfer)
    {
        var transfer = simpleTransfer.ToTransfer();
        var result = await tbClientProvider.Client.CreateTransferAsync(transfer);

        if (result != CreateTransferResult.Ok)
            throw new TigerBeetleResultException<CreateTransferResult>(result);

        return transfer.Id;
    }

    public async Task<UInt128> PostPendingTransfer(UInt128 pendingId, LedgerTransfer simpleTransfer)
    {
        var transfer = simpleTransfer.ToTransfer(TransferFlags.PostPendingTransfer, pendingId);

        var result = await tbClientProvider.Client.CreateTransferAsync(transfer);

        if (result != CreateTransferResult.Ok)
            throw new TigerBeetleResultException<CreateTransferResult>(result);

        return transfer.Id;
    }

    public async Task<UInt128> VoidPendingTransfer(UInt128 pendingId, LedgerTransfer simpleTransfer)
    {
        var transfer = simpleTransfer.ToTransfer(TransferFlags.VoidPendingTransfer, pendingId);

        var result = await tbClientProvider.Client.CreateTransferAsync(transfer);

        if (result != CreateTransferResult.Ok)
            throw new TigerBeetleResultException<CreateTransferResult>(result);

        return transfer.Id;
    }

    public async Task<(UInt128, UInt128)> BalanceAndCloseCredit(UInt128 debitAccountId, UInt128 creditAccountId)
    {
        var idBalance = ID.Create();

        var transferBalance = new Transfer
        {
            Id = idBalance,
            Code = TransferCode,
            DebitAccountId = debitAccountId,
            CreditAccountId = creditAccountId,
            Amount = UInt128.MaxValue,
            Ledger = LedgerId,
            Flags = TransferFlags.BalancingCredit | TransferFlags.Linked,
        };

        var idClose = ID.Create();

        var transferClose = new Transfer
        {
            Id = idClose,
            Code = TransferCode,
            DebitAccountId = debitAccountId,
            CreditAccountId = creditAccountId,
            Amount = 0,
            Ledger = LedgerId,
            Flags = TransferFlags.Pending | TransferFlags.ClosingCredit,
        };

        var transferResults = await tbClientProvider.Client.CreateTransfersAsync(new[] { transferBalance, transferClose });
        ThrowBatchError(transferResults);

        return (idBalance, idClose);
    }

    private async Task TransferBatch(Transfer[] transfers)
    {
        var results = await tbClientProvider.Client.CreateTransfersAsync(transfers);
        ThrowBatchError(results);
    }

    // Find most useful error in for linked transfers
    private void ThrowBatchError(CreateTransfersResult[] results)
    {
        if (results.Length == 0)
            return;

        var first = results[0].Result;

        if (first != CreateTransferResult.Ok)
        {
            foreach (var result in results)
                if (result.Result != CreateTransferResult.Ok && result.Result != CreateTransferResult.LinkedEventFailed)
                    throw new TigerBeetleResultException<CreateTransferResult>(result.Result);

            throw new TigerBeetleResultException<CreateTransferResult>(first);
        }
    }
}
