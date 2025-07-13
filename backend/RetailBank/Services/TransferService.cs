using RetailBank.Exceptions;
using RetailBank.Models.Interbank;
using RetailBank.Models.Ledger;
using RetailBank.Repositories;
using TigerBeetle;

namespace RetailBank.Services;

public class TransferService(ILedgerRepository ledgerRepository, InterbankClient interbankClient)
{
    public async Task<LedgerTransfer?> GetTransfer(UInt128 id)
    {
        return await ledgerRepository.GetTransfer(id);
    }

    public async Task<IEnumerable<LedgerTransfer>> GetTransfers(uint limit, ulong timestampMax)
    {
        return await ledgerRepository.GetTransfers(limit, timestampMax);
    }

    public async Task<UInt128> Transfer(UInt128 payerAccountId, UInt128 payeeAccountId, UInt128 amount, ulong reference)
    {
        var payerAccount = await ledgerRepository.GetAccount(payerAccountId) ?? throw new AccountNotFoundException(payerAccountId);

        if (payerAccount.AccountType != LedgerAccountType.Transactional)
            throw new InvalidAccountException(payerAccount.AccountType, LedgerAccountType.Transactional);

        var payeeBankCode = GetBankCode(payeeAccountId);

        switch (payeeBankCode)
        {
            case Bank.Retail:
                var payeeAccount = await ledgerRepository.GetAccount(payeeAccountId) ?? throw new AccountNotFoundException(payeeAccountId);

                if (payeeAccount.AccountType != LedgerAccountType.Transactional)
                    throw new InvalidAccountException(payerAccount.AccountType, LedgerAccountType.Transactional);

                var idInternal = await ledgerRepository.Transfer(
                    new LedgerTransfer(ID.Create(), payerAccountId, payeeAccountId, amount, reference, TransferType.Transfer)
                );
                return idInternal;
            case Bank.Commercial:
                var idCommercial = await ExternalCommercialTransfer(payerAccountId, payeeAccountId, amount, reference);
                return idCommercial;
            default:
                throw new InvalidDataException();
        }
    }

    public async Task PaySalary(UInt128 accountId)
    {
        var account = await ledgerRepository.GetAccount(accountId) ?? throw new AccountNotFoundException(accountId);

        if (account.AccountType != LedgerAccountType.Transactional)
            throw new InvalidAccountException(account.AccountType, LedgerAccountType.Transactional);

        if (account.DebitOrder == null || account.DebitOrder.Amount == 0)
            return;

        await ledgerRepository.Transfer(
            new LedgerTransfer(
                ID.Create(), account.DebitOrder.DebitAccountId,
                account.Id, account.DebitOrder.Amount,
                0, TransferType.Transfer
            )
        );
    }

    private async Task<UInt128> ExternalCommercialTransfer(UInt128 payerAccountId, UInt128 externalAccountId, UInt128 amount, ulong reference)
    {
        var pendingTransfer = new LedgerTransfer(ID.Create(), payerAccountId, externalAccountId, amount, reference, TransferType.StartTransfer);
        var pendingId = await ledgerRepository.Transfer(pendingTransfer);

        var result = await interbankClient.TryExternalTransfer(Bank.Commercial, pendingId, payerAccountId, externalAccountId, amount, reference);

        switch (result)
        {
            case NotificationResult.Succeeded:
                var completionTransfer = pendingTransfer with {
                    Id = ID.Create(),
                    ParentId = pendingId,
                    TransferType = TransferType.CompleteTransfer,
                };
                
                var completedId = await ledgerRepository.Transfer(completionTransfer);
                return completedId;
            case NotificationResult.Rejected:
                var cancellationTransfer = pendingTransfer with
                {
                    Id = ID.Create(),
                    ParentId = pendingId,
                    TransferType = TransferType.CancelTransfer,
                };
                
                await ledgerRepository.Transfer(cancellationTransfer);
                
                throw new ExternalTransferFailedException();
            default:
                // We have not received a success or rejection response
                // from the external service, so we cannot know whether
                // the external service has processed the transaction
                // or not. The transfer must be left as pending and then 
                // must be retried or manually resolved later.
                // This transfer will remain pending, and will have to 
                // be resolved by a business process.
                throw new ExternalTransferFailedException();
        }
    }

    public static Bank? GetBankCode(UInt128 accountNumber)
    {
        var divisor = (UInt128)Math.Pow(10, (int)Math.Log10((double)accountNumber) - 3);
        var prefix = (ushort)(accountNumber / divisor);
        var bankCode = (Bank)prefix;

        if (Enum.IsDefined(bankCode))
            return bankCode;

        return null;
    }
}
