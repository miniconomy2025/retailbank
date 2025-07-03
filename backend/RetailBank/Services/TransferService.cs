using RetailBank.Exceptions;
using RetailBank.Models;
using RetailBank.Models.Dtos;
using RetailBank.Models.Interbank;
using RetailBank.Repositories;

namespace RetailBank.Services;

public class TransferService(ILedgerRepository ledgerRepository, IInterbankClient interbankClient) : ITransferService
{
    public async Task<TransferEvent?> GetTransfer(UInt128 id)
    {
        var transfer = await ledgerRepository.GetTransfer(id);

        if (transfer.HasValue)
            return new TransferEvent(transfer.Value, await TransferEvent.MapEventType(transfer.Value, this));
        else
            return null;
    }

    public async Task<IEnumerable<TransferEvent>> GetTransfers(uint limit, ulong timestampMax)
    {
        var transfers = await ledgerRepository.GetTransfers(limit, timestampMax);

        return await Task.WhenAll(transfers.Select(async transfer => new TransferEvent(transfer, await TransferEvent.MapEventType(transfer, this))));
    }

    public async Task<UInt128> Transfer(ulong payerAccountId, ulong payeeAccountId, UInt128 amount)
    {
        var payerAccount = await ledgerRepository.GetAccount(payerAccountId) ?? throw new AccountNotFoundException(payerAccountId);

        if (payerAccount.Code != (ushort)LedgerAccountCode.Transactional)
            throw new InvalidAccountException((LedgerAccountCode)payerAccount.Code, LedgerAccountCode.Transactional);

        var payeeBankCode = GetBankCode(payeeAccountId);

        switch (payeeBankCode)
        {
            case BankId.Retail:
                var payeeAccount = await ledgerRepository.GetAccount(payeeAccountId) ?? throw new AccountNotFoundException(payeeAccountId);

                if (payeeAccount.Code != (ushort)LedgerAccountCode.Transactional)
                    throw new InvalidAccountException((LedgerAccountCode)payerAccount.Code, LedgerAccountCode.Transactional);

                var idInternal = await ledgerRepository.Transfer(new LedgerTransfer(payerAccountId, payeeAccountId, amount));
                return idInternal;
            case BankId.Commercial:
                var idCommercial = await ExternalCommercialTransfer(payerAccountId, payeeAccountId, amount);
                return idCommercial;
            default:
                throw new InvalidDataException();
        }
    }

    public async Task PaySalary(ulong accountId)
    {
        var account = await ledgerRepository.GetAccount(accountId) ?? throw new AccountNotFoundException(accountId);

        if (account.Code != (ushort)LedgerAccountCode.Transactional)
            throw new InvalidAccountException((LedgerAccountCode)account.Code, LedgerAccountCode.Transactional);

        var salary = account.UserData64;

        var id = await ledgerRepository.Transfer(new LedgerTransfer((ulong)BankId.Retail, account.Id, salary));
    }

    private async Task<UInt128> ExternalCommercialTransfer(ulong payerAccountId, ulong externalAccountId, UInt128 amount)
    {
        var transfer = new LedgerTransfer(payerAccountId, (ulong)BankId.Commercial, amount, externalAccountId);
        var pendingId = await ledgerRepository.StartTransfer(transfer);

        var result = await interbankClient.TryNotify(BankId.Commercial, pendingId.ToString("X"), payerAccountId, externalAccountId, amount);

        switch (result)
        {
            case NotificationResult.Succeeded:
                var id = await ledgerRepository.PostPendingTransfer(pendingId, transfer);
                return id;
            case NotificationResult.Failed:
                await ledgerRepository.VoidPendingTransfer(pendingId, transfer);
                throw new ExternalTransferFailedException();
            default:
                // We have not received a success or failure response
                // from the external service, so we cannot know whether
                // the external service has processed the transaction
                // or not. The transfer must be left as pending and then 
                // must be retried or manually resolved later.
                throw new ExternalTransferFailedException();
        }
    }

    private static BankId? GetBankCode(UInt128 accountNumber)
    {
        var divisor = (UInt128)Math.Pow(10, (int)Math.Log10((double)accountNumber) - 3);
        var prefix = (ushort)(accountNumber / divisor);
        var bankCode = (BankId)prefix;

        if (Enum.IsDefined(bankCode))
            return bankCode;

        return null;
    }
}
