using RetailBank.Repositories;
using TigerBeetle;

namespace RetailBank.Models;

public record LedgerTransfer(
    UInt128 DebitAccountId,
    UInt128 CreditAccountId,
    UInt128 Amount,
    UInt128 UserData128 = default,
    ulong UserData64 = default,
    uint UserData32 = default
)
{
    public Transfer ToTransfer(TransferFlags flags = TransferFlags.None, UInt128 pendingId = default)
    {
        return new Transfer
        {
            Id = ID.Create(),
            DebitAccountId = DebitAccountId,
            CreditAccountId = CreditAccountId,
            Amount = Amount,
            UserData128 = UserData128,
            UserData64 = UserData64,
            UserData32 = UserData32,
            Ledger = TigerBeetleRepository.LedgerId,
            Code = TigerBeetleRepository.TransferCode,
            PendingId = pendingId,
            Flags = flags,
        };
    }
}
