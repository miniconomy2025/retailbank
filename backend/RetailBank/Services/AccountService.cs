using System.Security.Cryptography;
using RetailBank.Models;
using RetailBank.Models.Dtos;
using RetailBank.Repositories;

namespace RetailBank.Services;

public class AccountService(ILedgerRepository ledgerRepository, ITransferService transferService) : IAccountService
{
    public async Task<ulong> CreateTransactionalAccount(ulong salary)
    {
        var id = GenerateTransactionalAccountNumber();

        await ledgerRepository.CreateAccount(
            id,
            LedgerAccountCode.Transactional,
            TigerBeetle.AccountFlags.DebitsMustNotExceedCredits,
            0,
            salary
        );

        return id;
    }

    public async Task<IEnumerable<LedgerAccount>> GetAccounts(LedgerAccountCode? code, uint limit, ulong timestampMax)
    {
        return (await ledgerRepository.GetAccounts(code, limit, timestampMax)).Select(account => new LedgerAccount(account));
    }

    public async Task<LedgerAccount?> GetAccount(ulong accountId)
    {
        var account = await ledgerRepository.GetAccount(accountId);

        if (account.HasValue)
            return new LedgerAccount(account.Value);
        else
            return null;
    }

    public async Task<IEnumerable<TransferEvent>> GetAccountTransfers(ulong accountId, uint limit, ulong timestampMax, TransferSide side)
    {
        var transfers = await ledgerRepository.GetAccountTransfers(accountId, limit, timestampMax, side);

        return await Task.WhenAll(transfers.Select(async transfer => new TransferEvent(transfer, await TransferEvent.MapEventType(transfer, transferService))));
    }

    // 12 digits starting with "1000"
    private static ulong GenerateTransactionalAccountNumber()
    {
        var number = 1000_0000_0000ul + (ulong)RandomNumberGenerator.GetInt32(1_0000_0000);
        return number;
    }
}
