using RetailBank.Repositories;
using RetailBank.Services;
using TigerBeetle;

namespace RetailBank.Models.Ledger;

public record LedgerTransfer(
    UInt128 Id,
    UInt128 DebitAccountId,
    UInt128 CreditAccountId,
    UInt128 Amount,
    ulong Reference,
    TransferType TransferType,
    UInt128? ParentId = null,
    ulong Timestamp = 0
)
{
    public LedgerTransfer(Transfer transfer, ulong startTime, uint timeScale) : this(
        transfer.Id,
        transfer.DebitAccountId,
        transfer.CreditAccountId,
        transfer.Amount,
        transfer.UserData64,
        transfer.Flags.ToTransferType(),
        transfer.PendingId > 0 ? transfer.PendingId : null,
        SimulationControllerService.MapToSimTimestamp(transfer.Timestamp, startTime, timeScale)
    )
    {
        // external transfers have bank ID as credit account ID, and external bank account number as UserData128
        var bank = TransferService.GetBankCode(transfer.CreditAccountId);
        if (bank.HasValue && bank.Value != BankId.Retail)
        {
            CreditAccountId = transfer.UserData128;
        }
    }

    public Transfer ToTransfer(bool linked)
    {
        var bank = TransferService.GetBankCode(CreditAccountId);

        UInt128 creditAccount = CreditAccountId;
        UInt128 supplementaryAccountId = 0;
        
        // Transfers to external banks must go to the bank's internal account, since we do not know about their account types.
        if (bank.HasValue && bank.Value != BankId.Retail)
        {
            creditAccount = (ulong)bank.Value;
            supplementaryAccountId = CreditAccountId;
        }

        var linkedFlag = linked ? TransferFlags.Linked : TransferFlags.None;

        return new Transfer
        {
            Id = Id,
            DebitAccountId = DebitAccountId,
            CreditAccountId = creditAccount,
            Amount = Amount,
            UserData128 = supplementaryAccountId,
            UserData64 = Reference,
            UserData32 = 0,
            Ledger = TigerBeetleRepository.LedgerId,
            Code = TigerBeetleRepository.TransferCode,
            PendingId = ParentId ?? 0,
            Flags = TransferType.ToTransferFlags() | linkedFlag,
        };
    }
}
