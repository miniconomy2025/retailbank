using RetailBank.Services;
using TigerBeetle;

namespace RetailBank.Models.Dtos;

public enum TransferEventType
{
    Transfer,
    StartTransfer,
    CompleteTransfer,
    CancelTransfer,
    ClosingCredit,
    ClosingDebit,
    ReopenCredit,
    ReopenDebit,
}
