using System.Net;
using Microsoft.Extensions.Options;
using RetailBank.Extensions;
using RetailBank.Models.Interbank;
using RetailBank.Models.Ledger;
using RetailBank.Models.Options;

namespace RetailBank.Services;

public class InterbankClient(HttpClient httpClient, IOptions<InterbankTransferOptions> options)
{
    private async Task<string?> TryGetExternalAccount(string getAccountUrl, string createAccountUrl)
    {

        string externalAccountId;

        try
        {
            var getAccountResponse = await httpClient.GetAsync(getAccountUrl);

            if (getAccountResponse.StatusCode != HttpStatusCode.NotFound)
                getAccountResponse.EnsureSuccessStatusCode();

            var getAccountBody = await getAccountResponse.Content.ReadFromJsonAsync<CommercialAccountNumberResponse>();
            ArgumentNullException.ThrowIfNull(getAccountBody);

            externalAccountId = getAccountBody.AccountNumber;
        }
        catch
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(externalAccountId))
        {
            try
            {
                var createAccountResponse = await httpClient.PostAsync(createAccountUrl, null);
                createAccountResponse.EnsureSuccessStatusCode();

                var createAccountBody = await createAccountResponse.Content.ReadFromJsonAsync<CommercialAccountNumberResponse>();
                ArgumentNullException.ThrowIfNull(createAccountBody);

                externalAccountId = createAccountBody.AccountNumber;
            }
            catch
            {
                return null;
            }
        }

        if (string.IsNullOrWhiteSpace(externalAccountId))
            return null;
        
        return externalAccountId;
    }

    public async Task<bool> TryCreateExternalLoan(Bank bank)
    {
        if (!options.Value.Banks.TryGetValue(bank, out var bankDetails))
            return false;
        
        try
        {
            var balanceResponse = await httpClient.PostAsJsonAsync(
                bankDetails.IssueLoanUrl,
                new CreateCommercialLoanRequest(options.Value.LoanAmountCents / 100.0m)
            );

            balanceResponse.EnsureSuccessStatusCode();

            return true;
        }
        catch
        {
            return false;
        }
    }

    private async Task<NotificationResult> TryExternalTransferInternal(InterbankTransferBankDetails details, UInt128 transactionId, UInt128 from, UInt128 to, UInt128 amount, ulong reference)
    {
        var externalAccount = await TryGetExternalAccount(details.GetAccountUrl, details.CreateAccountUrl);

        if (string.IsNullOrWhiteSpace(externalAccount))
            return NotificationResult.Rejected;

        var transfer = new CreateCommercialTransferRequest(
            transactionId.ToHex(),
            externalAccount,
            to.ToString(),
            amount,
            reference.ToString()
        );

        HttpResponseMessage response;
        try
        {
            response = await httpClient.PostAsJsonAsync(details.TransferUrl, transfer);
        }
        catch
        {
            return NotificationResult.Failed;
        }

        if ((int)response.StatusCode / 100 == 2)
            return NotificationResult.Succeeded;

        if ((int)response.StatusCode / 100 == 4)
            return NotificationResult.Rejected;

        return NotificationResult.Failed;
    }

    public async Task<NotificationResult> TryExternalTransfer(Bank bank, UInt128 transactionId, UInt128 from, UInt128 to, UInt128 amount, ulong reference)
    {
        if (!options.Value.Banks.TryGetValue(bank, out var bankDetails))
            return NotificationResult.Rejected;

        NotificationResult result = NotificationResult.Rejected;

        for (int i = 0; i < options.Value.RetryCount; i++)
        {
            result = await TryExternalTransferInternal(bankDetails, transactionId, from, to, amount, reference).ConfigureAwait(false);

            if (result == NotificationResult.Succeeded)
                return result;

            await Task.Delay((int)options.Value.DelaySeconds * 1000);
        }

        return result;
    }

    public async Task GetExternalAccount(Bank bank)
    {
        if (!options.Value.Banks.TryGetValue(bank, out var bankDetails))
            return;

        var account = await httpClient.GetFromJsonAsync<CommercialAccountNumberResponse>(bankDetails.GetAccountUrl);
    }
}
