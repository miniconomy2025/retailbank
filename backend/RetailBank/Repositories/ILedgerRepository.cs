using TigerBeetle;

namespace RetailBank.Repositories;

public interface ILedgerRepository
{
    public Task CreateAccount(ulong accountNumber, ulong userData64 = 0, UInt128? userData128 = null, uint userData32 = 0, uint ledger = 1, ushort code = 1, AccountFlags accountFlags = AccountFlags.None);
    public Task Transfer(UInt128 id, ulong debitAccountId, ulong creditAccountId, UInt128 amount, uint ledger = 1, TransferFlags transferFlags = TransferFlags.None, ushort code = 1);
}
