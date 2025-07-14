using Microsoft.Extensions.Options;
using RetailBank.Exceptions;
using RetailBank.Models.Interbank;
using RetailBank.Models.Ledger;
using RetailBank.Models.Options;
using RetailBank.Repositories;
using TigerBeetle;

namespace RetailBank.Services;

public class TransferService(LedgerRepository ledgerRepository, InterbankClient interbankClient, IOptions<TransferOptions> options, IOptions<SimulationOptions> simOptions)
{
    public async Task<LedgerTransfer?> GetTransfer(UInt128 id)
    {
        return await ledgerRepository.GetTransfer(id);
    }

    public async Task<IEnumerable<LedgerTransfer>> GetTransfers(uint limit, ulong cursorMax, ulong? reference)
    {
        return await ledgerRepository.GetTransfers(limit, cursorMax, reference);
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

                var feeAmount = (UInt128)((decimal)amount * options.Value.TransferFeePercent / 100.0m);

                var idInternal = await ledgerRepository.TransferLinked([
                    new LedgerTransfer(ID.Create(), payerAccountId, payeeAccountId, amount, reference, TransferType.Transfer),
                    new LedgerTransfer(ID.Create(), payerAccountId, (ulong)LedgerAccountId.FeeIncome, feeAmount, 0, TransferType.Transfer)
                ]);
                return idInternal.First();
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

        var feeAmount = (ulong)(account.DebitOrder.Amount * options.Value.DepositFeePercent / 100.0m);

        await ledgerRepository.TransferLinked([
            new LedgerTransfer(
                ID.Create(), account.DebitOrder.DebitAccountId,
                account.Id, account.DebitOrder.Amount,
                0, TransferType.Transfer
            ),
            new LedgerTransfer(
                ID.Create(), account.Id,
                (ulong)LedgerAccountId.FeeIncome, feeAmount,
                0, TransferType.Transfer
            )
        ]);
    }

    public async Task<UInt128> GetRecentVolume()
    {
        const uint BatchSize = 4096;
        // 1 day in-sim
        ulong RecentTimePeriod = 24ul * 3600ul * 1000000000ul / simOptions.Value.TimeScale;

        UInt128 volume = 0;

        var transfers = await GetTransfers(BatchSize, 0, null);

        var minCursor = 0ul;

        if (transfers.Count() > 0)
            minCursor = transfers.First().Cursor - RecentTimePeriod;

        var finished = false;

        while (transfers.Count() > 0)
        {
            foreach (var transfer in transfers)
            {
                if (transfer.Cursor < minCursor)
                {
                    finished = true;
                    break;
                }
                if (transfer.TransferType == TransferType.Transfer || transfer.TransferType == TransferType.CompleteTransfer)
                    volume += transfer.Amount;
            }

            if (finished)
                break;

            transfers = await GetTransfers(BatchSize, transfers.Last().Cursor - 1, null);
        }

        return volume;
    }

    private async Task<UInt128> ExternalCommercialTransfer(UInt128 payerAccountId, UInt128 externalAccountId, UInt128 amount, ulong reference)
    {
        var feeAmount = (UInt128)((decimal)amount * options.Value.TransferFeePercent / 100.0m);

        var pendingTransfers = new[] {
            new LedgerTransfer(ID.Create(), payerAccountId, externalAccountId, amount, reference, TransferType.StartTransfer),
            new LedgerTransfer(ID.Create(), payerAccountId, (ulong)LedgerAccountId.FeeIncome, feeAmount, 0, TransferType.StartTransfer)
        };

        await ledgerRepository.TransferLinked(pendingTransfers);

        var result = await interbankClient.TryExternalTransfer(Bank.Commercial, payerAccountId, externalAccountId, amount, reference);

        switch (result)
        {
            case NotificationResult.Succeeded:
                var completionTransfers = pendingTransfers.Select(transfer => transfer with
                {
                    Id = ID.Create(),
                    ParentId = transfer.Id,
                    TransferType = TransferType.CompleteTransfer,
                });

                var completedIds = await ledgerRepository.TransferLinked(completionTransfers);
                return completedIds.First();
            
            case NotificationResult.Rejected:
            case NotificationResult.AccountNotFound:
                var cancellationTransfers = pendingTransfers.Select(transfer => transfer with
                {
                    Id = ID.Create(),
                    ParentId = transfer.Id,
                    TransferType = TransferType.CancelTransfer,
                });

                await ledgerRepository.TransferLinked(cancellationTransfers);

                if (result == NotificationResult.AccountNotFound)
                    throw new AccountNotFoundException(externalAccountId);

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
