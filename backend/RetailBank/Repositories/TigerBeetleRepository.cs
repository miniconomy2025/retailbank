using RetailBank.Exceptions;
using RetailBank.Models.Ledger;
using RetailBank.Services;
using TigerBeetle;

namespace RetailBank.Repositories;

public class TigerBeetleRepository(TigerBeetleClientProvider tbClientProvider, SimulationControllerService simulationControllerService) : ILedgerRepository
{
    public const uint LedgerId = 1;
    public const ushort TransferCode = 1;

    public async Task CreateAccount(LedgerAccount account)
    {
        var result = await tbClientProvider.Client.CreateAccountAsync(account.ToAccount());

        if (result != CreateAccountResult.Ok)
            throw new TigerBeetleResultException<CreateAccountResult>(result);
    }

    public async Task<IEnumerable<LedgerAccount>> GetAccounts(LedgerAccountType? code, UInt128? debitAccountId, uint limit, ulong timestampMax)
    {
        var filter = new QueryFilter();
        filter.Limit = limit;
        filter.TimestampMax = timestampMax;
        filter.Flags = QueryFilterFlags.Reversed;

        if (code.HasValue)
            filter.Code = (ushort)code.Value;

        if (debitAccountId.HasValue)
            filter.UserData128 = debitAccountId.Value;

        var accounts = (await tbClientProvider.Client.QueryAccountsAsync(filter))
            .Select(account => new LedgerAccount(account, simulationControllerService.StartTime, simulationControllerService.TimeScale));
        return accounts;
    }

    public async Task<LedgerAccount?> GetAccount(UInt128 accountId)
    {
        var account = await tbClientProvider.Client.LookupAccountAsync(accountId);
        
        if (!account.HasValue)
            return null;
        
        return new LedgerAccount(account.Value, simulationControllerService.StartTime, simulationControllerService.TimeScale);
    }

    public async Task<IEnumerable<LedgerTransfer>> GetAccountTransfers(UInt128 id, uint limit, ulong timestampMax, TransferSide? side)
    {
        var filter = new AccountFilter();
        filter.AccountId = id;
        filter.Limit = limit;
        filter.TimestampMax = timestampMax;
        filter.Flags = (side?.ToAccountFilterFlags() ?? (AccountFilterFlags.Debits | AccountFilterFlags.Credits)) | AccountFilterFlags.Reversed;

        var transfers = (await tbClientProvider.Client.GetAccountTransfersAsync(filter))
            .Select(transfer => new LedgerTransfer(transfer, simulationControllerService.StartTime, simulationControllerService.TimeScale));
        
        return transfers;
    }

    public async Task<IEnumerable<LedgerTransfer>> GetTransfers(uint limit, ulong timestampMax)
    {
        var filter = new QueryFilter();
        filter.Limit = limit;
        filter.TimestampMax = timestampMax;
        filter.Flags = QueryFilterFlags.Reversed;

        var transfers = (await tbClientProvider.Client.QueryTransfersAsync(filter))
            .Select(transfer => new LedgerTransfer(transfer, simulationControllerService.StartTime, simulationControllerService.TimeScale));
        return transfers;
    }

    public async Task<LedgerTransfer?> GetTransfer(UInt128 id)
    {
        var transfer = await tbClientProvider.Client.LookupTransferAsync(id);

        if (!transfer.HasValue)
            return null;

        return new LedgerTransfer(transfer.Value, simulationControllerService.StartTime, simulationControllerService.TimeScale);
    }

    public async Task<UInt128> Transfer(LedgerTransfer ledgerTransfer)
    {
        var transfer = ledgerTransfer.ToTransfer(false);
        var transferResult = await tbClientProvider.Client.CreateTransferAsync(transfer);

        if (transferResult != CreateTransferResult.Ok)
            throw new TigerBeetleResultException<CreateTransferResult>(transferResult);

        return transfer.Id;
    }

    public async Task<IEnumerable<UInt128>> TransferLinked(IEnumerable<LedgerTransfer> ledgerTransfers)
    {
        var length = ledgerTransfers.Count();
        var transfers = ledgerTransfers.Select((transfer, index) => transfer.ToTransfer(index < length - 1));

        await TransferBatch(transfers.ToArray());

        return transfers.Select(transfer => transfer.Id);
    }

    public async Task<(UInt128, UInt128)> BalanceAndCloseCredit(UInt128 debitAccountId, UInt128 creditAccountId)
    {
        var idBalance = ID.Create();

        var transferBalance = new Transfer
        {
            Id = idBalance,
            Code = (ushort)TransferType.BalanceCredit,
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
            Code = (ushort)TransferType.CloseCredit,
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

    public async Task InitialiseInternalAccounts()
    {
        foreach (var variant in Enum.GetValues<Bank>())
        {
            try
            {
                await CreateAccount(new LedgerAccount((ulong)variant, LedgerAccountType.Internal));
            }
            catch (TigerBeetleResultException<CreateAccountResult> ex) when (ex.ErrorCode == CreateAccountResult.Exists) { }
        }

        foreach (var variant in Enum.GetValues<LedgerAccountId>())
        {
            try
            {
                await CreateAccount(new LedgerAccount((ulong)variant, LedgerAccountType.Internal));
            }
            catch (TigerBeetleResultException<CreateAccountResult> ex) when (ex.ErrorCode == CreateAccountResult.Exists) { }
        }

        var mainAccount = await GetAccount((ulong)Bank.Retail).ConfigureAwait(false);
    }

    private async Task TransferBatch(Transfer[] transfers)
    {
        var results = await tbClientProvider.Client.CreateTransfersAsync(transfers);
        ThrowBatchError(results);
    }

    // Find most useful error in for batch of transfers
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
