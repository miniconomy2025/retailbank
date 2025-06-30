using RetailBank.Models;
using RetailBank.Models.Dtos;
using RetailBank.Repositories;
using TigerBeetle;

namespace RetailBank.Services;

public class TransferService(ILedgerRepository ledgerRepository) : ITransferService
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

    public async Task Transfer(ulong payerAccountId, ulong payeeAccountId, UInt128 amount)
    {
        var payerAccount = await ledgerRepository.GetAccount(payerAccountId) ?? throw new AccountNotFoundException(payerAccountId);

        if (payerAccount.Code != (ushort)LedgerAccountCode.Transactional)
            throw new InvalidAccountException();

        var payeeBankCode = GetBankCode(payeeAccountId);

        switch (payeeBankCode)
        {
            case BankCode.Retail:
                var payeeAccount = await ledgerRepository.GetAccount(payeeAccountId) ?? throw new AccountNotFoundException(payeeAccountId);

                if (payeeAccount.Code != (ushort)LedgerAccountCode.Transactional)
                    throw new InvalidAccountException();

                await InternalTransfer(payerAccountId, payeeAccountId, amount);
                break;
            case BankCode.Commercial:
                await ExternalCommercialTransfer(payerAccountId, payeeAccountId, amount);
                break;
            default:
                throw new InvalidDataException();
        }
    }

    public async Task PaySalary(ulong accountId)
    {
        var account = await ledgerRepository.GetAccount(accountId) ?? throw new AccountNotFoundException(accountId);

        if (account.Code != (ushort)LedgerAccountCode.Transactional)
            throw new InvalidAccountException();

        var salary = account.UserData64;

        await ledgerRepository.Transfer(new LedgerTransfer((ulong)LedgerAccountId.Bank, account.Id, salary));
    }

    private async Task InternalTransfer(ulong payerAccountId, ulong payeeAccountId, UInt128 amount)
    {
        await ledgerRepository.Transfer(new LedgerTransfer(payerAccountId, payeeAccountId, amount));
    }

    private async Task ExternalCommercialTransfer(ulong payerAccountId, ulong externalAccountId, UInt128 amount)
    {
        var transfer = new LedgerTransfer(payerAccountId, (ulong)BankCode.Commercial, amount, externalAccountId);
        var pendingId = await ledgerRepository.StartTransfer(transfer);

        if (await TryExternalCommercialTransfer(externalAccountId, amount))
            await ledgerRepository.PostPendingTransfer(pendingId, transfer);
        else
        {
            await ledgerRepository.VoidPendingTransfer(pendingId, transfer);
            throw new ExternalTransferFailedException();
        }
    }

    private static async Task<bool> TryExternalCommercialTransfer(ulong externalAccountId, UInt128 amount)
    {
        // Todo: This function should attempt to call the endpoint provided by the commercial bank to initiate the transfer of 
        // funds on their end. This should try multiple times and the endpoint should be idempotent
        await Task.Delay(1000);
        return true;
    }

    private static BankCode? GetBankCode(UInt128 accountNumber)
    {
        var prefix = (ushort)(accountNumber / (UInt128)Math.Pow(10, Math.Log10((double)accountNumber) - 3));
        var bankCode = (BankCode)prefix;

        if (Enum.IsDefined(bankCode))
            return bankCode;

        return null;
    }
}
