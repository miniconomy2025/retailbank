using RetailBank.Models;
using TigerBeetle;

namespace RetailBank.Repositories;

public interface ILedgerRepository
{
    public Task CreateAccount
    (
        ulong accountNumber,
        LedgerAccountCode code,
        ulong userData64 = 0,
        UInt128? userData128 = null,
        uint userData32 = 0,
        AccountFlags accountFlags = AccountFlags.None
    );
    public Task Transfer
    (
        UInt128 transferId,
        ulong debitAccountId,
        ulong creditAccountId,
        UInt128 amount,
        TransferFlags transferFlags = TransferFlags.None,
        ushort code = 1,
        UInt64 userData64 = 0,
        UInt128? pendingId = null
    );
}
