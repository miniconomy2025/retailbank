using System.Net;
using Microsoft.Extensions.Options;
using RetailBank.Extensions;
using RetailBank.Models.Interbank;
using RetailBank.Models.Ledger;
using RetailBank.Models.Options;

namespace RetailBank.Services;

public class InterbankClient(HttpClient httpClient, IOptions<InterbankTransferOptions> options)
{
    private async Task<string?> TryGetOrCreateExternalAccount(string getAccountUrl, string createAccountUrl)
    {

        string? externalAccountId;

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
            externalAccountId = null;
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
                externalAccountId = null;
            }
        }

        return externalAccountId;
    }
    
    private async Task<decimal?> TryGetExternalAccountBalance(string getAccountBalanceUrl)
    {
        try
        {
            var getAccountResponse = await httpClient.GetFromJsonAsync<GetCommercialAccountBalanceResponse>(getAccountBalanceUrl);
            return getAccountResponse?.Balance;
        }
        catch
        {
            return null;
        }
    }

    private async Task<bool> TryCreateExternalLoan(string issueLoanUrl, UInt128 loanAmountCents)
    {
        try
        {
            var balanceResponse = await httpClient.PostAsJsonAsync(
                issueLoanUrl,
                new CreateCommercialLoanRequest((decimal)loanAmountCents / 100.0m)
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
        var externalAccount = await TryGetOrCreateExternalAccount(details.GetAccountUrl, details.CreateAccountUrl);
        if (string.IsNullOrWhiteSpace(externalAccount))
            return NotificationResult.Rejected;

        var externalBalance = await TryGetExternalAccountBalance(details.GetAccountBalanceUrl);
        if (externalBalance == null)
            return NotificationResult.Rejected;

        var externalBalanceCents = (UInt128)(100 * externalBalance);

        // if external balance less than loan threshold then we issue a new loan
        if (externalBalanceCents < options.Value.LoanAmountCents)
        {
            var loanSuccess = await TryCreateExternalLoan(details.IssueLoanUrl, options.Value.LoanAmountCents);
            if (!loanSuccess)
                return NotificationResult.Rejected;
        }

        var transfer = new CreateCommercialTransferRequest(
            to.ToString(),
            "commercial-bank",
            (decimal)amount / 100.0m,
            $"Retail Transfer {from}, Reference: {reference}"
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
}
