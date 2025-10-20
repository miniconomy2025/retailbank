namespace RetailBank.Tests.Mocks;

using RetailBank.Models.Ledger;
using RetailBank.Repositories;
using System.Collections.Concurrent;
using TigerBeetle;

public class LedgerRepositoryMock : ILedgerRepository
{
    private readonly ConcurrentDictionary<UInt128, Account> _accounts = new();
    private readonly ConcurrentDictionary<UInt128, Transfer> _transfers = new();
    public const uint LedgerId = 1;
    public const ushort TransferCode = 1;
    public Task CreateAccount(LedgerAccount account)
    {
        if (_accounts.ContainsKey(account.Id))
            throw new InvalidOperationException($"Account with ID {account.Id} already exists.");

        _accounts[account.Id] = account.ToAccount();
        return Task.CompletedTask;
    }

    public Task<IEnumerable<LedgerAccount>> GetAccounts(
        LedgerAccountType? code,
        UInt128? debitAccountId,
        uint limit,
        ulong cursorMax)
    {
        var accounts = _accounts.Values.AsEnumerable();

        if (code.HasValue)
            accounts = accounts.Where(a => a.Code == (ushort)code.Value);

        if (debitAccountId.HasValue)
            accounts = accounts.Where(a => a.UserData128 == debitAccountId.Value);

        accounts = accounts.Where(a => a.Timestamp < cursorMax);
        accounts = accounts.Take((int)limit);
        return Task.FromResult(accounts.Select(a=> new LedgerAccount(a)));
    }

    public Task<LedgerAccount?> GetAccount(UInt128 accountId)
    {
        _accounts.TryGetValue(accountId, out var account);
        return Task.FromResult<LedgerAccount?>(new LedgerAccount(account));
    }

    public Task<IEnumerable<LedgerTransfer>> GetAccountTransfers(
        UInt128 id,
        uint limit,
        ulong cursorMax,
        ulong? reference,
        TransferSide? side)
    {

        var transfers = _transfers.Values.AsEnumerable();
        if (side != null)
            transfers = side == TransferSide.Debit ? transfers.Where((t) => t.DebitAccountId == id) : transfers.Where((t) => t.CreditAccountId == id);
        else
        {
            transfers = transfers.Where((t) => t.DebitAccountId == id || t.CreditAccountId == id);
        }

        if (reference != null)
        {
            transfers = transfers.Where((t) => t.UserData64 == reference);
        }

        return Task.FromResult(transfers.Where((t) => t.Timestamp < cursorMax).Take((int)limit).Select((t) => new LedgerTransfer(t)));
    }

    public Task<IEnumerable<LedgerTransfer>> GetTransfers(uint limit, ulong cursorMax, ulong? reference)
    {
        var transfers = _transfers.Values.AsEnumerable();

        if (reference.HasValue)
            transfers = transfers.Where(t => t.UserData64 == reference.Value);

        return Task.FromResult(transfers.Take((int)limit).Select(t => new LedgerTransfer(t)));
    }

    public Task<LedgerTransfer?> GetTransfer(UInt128 id)
    {
        _transfers.TryGetValue(id, out var transfer);
        return Task.FromResult<LedgerTransfer?>(new LedgerTransfer(transfer));
    }

    public Task<UInt128> Transfer(LedgerTransfer ledgerTransfer)
    {
        if (!_accounts.ContainsKey(ledgerTransfer.DebitAccountId) || !_accounts.ContainsKey(ledgerTransfer.CreditAccountId))
            throw new InvalidOperationException("Both accounts must exist before making a transfer.");
        var id = ID.Create();
        _transfers[id] = ledgerTransfer.ToTransfer(false); 
        return Task.FromResult(id);
    }

    public Task<IEnumerable<UInt128>> TransferLinked(IEnumerable<LedgerTransfer> ledgerTransfers)
    {
        var ids = new List<UInt128>();

        foreach (var lt in ledgerTransfers)
        {
            var id = ID.Create();
            _transfers[id] = lt.ToTransfer(true);
            ids.Add(id);
        }

        return Task.FromResult(ids.AsEnumerable());
    }

    public Task<(UInt128, UInt128)> BalanceAndCloseCredit(UInt128 debitAccountId, UInt128 creditAccountId)
    {
        var idBalance = ID.Create();
        var idClose = ID.Create();

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

        _transfers[idBalance] = transferBalance;
        _transfers[idClose] = transferClose;

        return Task.FromResult((idBalance, idClose));
    }

    public Task InitialiseInternalAccounts()
    {
        foreach (var variant in Enum.GetValues<Bank>())
        {
            var account = new LedgerAccount((ulong)variant, LedgerAccountType.Internal);
            _accounts[account.Id] = account.ToAccount();
        }

        foreach (var variant in Enum.GetValues<LedgerAccountId>())
        {
            var account = new LedgerAccount((ulong)variant, LedgerAccountType.Internal);
            _accounts[account.Id] = account.ToAccount();
        }
        return Task.CompletedTask;
    }
}
