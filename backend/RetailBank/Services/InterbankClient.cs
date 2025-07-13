using System.Net;
using Microsoft.Extensions.Options;
using RetailBank.Models.Interbank;
using RetailBank.Models.Ledger;
using RetailBank.Models.Options;

namespace RetailBank.Services;

public class InterbankClient(HttpClient httpClient, IOptions<InterbankTransferOptions> options, ILogger<InterbankClient> logger)
{
    private async Task<decimal?> TryGetExternalAccountBalance(string getAccountUrl, string createAccountUrl, string notifyUrl)
    {
        try
        {
            var createAccountResponse = await httpClient.PostAsJsonAsync(createAccountUrl, new CreateCommercialAccountRequest(notifyUrl));
         
            if (createAccountResponse.StatusCode != HttpStatusCode.Conflict)
            {
                if (!createAccountResponse.IsSuccessStatusCode)
                {
                    var response = await createAccountResponse.Content.ReadAsStringAsync();
                    logger.LogError($"Invalid response from commercial bank while creating account: {response}");
                }
                
                createAccountResponse.EnsureSuccessStatusCode();
            }
        }
        catch (Exception e)
        {
            logger.LogError($"Failed to create commercial bank account: {e}");
            return null;
        }

        try
        {
            var getAccountResponse = await httpClient.GetAsync(getAccountUrl);

            if (getAccountResponse.StatusCode != HttpStatusCode.NotFound)
            {
                if (!getAccountResponse.IsSuccessStatusCode)
                {
                    var response = await getAccountResponse.Content.ReadAsStringAsync();
                    logger.LogError($"Invalid response from commercial bank while getting account balance: {response}");
                }
                getAccountResponse.EnsureSuccessStatusCode();
            }

            var getAccountBody = await getAccountResponse.Content.ReadFromJsonAsync<GetCommercialAccountResponse>();
            ArgumentNullException.ThrowIfNull(getAccountBody);

            return getAccountBody.NetBalance;
        }
        catch (Exception e)
        {
            logger.LogError($"Failed to check commercial bank account balance: {e}");
            return null;
        }
    }
    
    private async Task<bool> TryCreateExternalLoan(string issueLoanUrl, UInt128 loanAmountCents)
    {
        try
        {
            var loanResponse = await httpClient.PostAsJsonAsync(
                issueLoanUrl,
                new CreateCommercialLoanRequest((decimal)loanAmountCents / 100.0m)
            );

            if (!loanResponse.IsSuccessStatusCode)
            {
                var response = await loanResponse.Content.ReadAsStringAsync();
                logger.LogError($"Invalid response from commercial bank while issuing loan: {response}");
            }

            loanResponse.EnsureSuccessStatusCode();

            return true;
        }
        catch (Exception e)
        {
            logger.LogError($"Failed to create commercial bank loan: {e}");
            return false;
        }
    }

    private async Task<NotificationResult> TryExternalTransferInternal(InterbankTransferBankDetails details, UInt128 from, UInt128 to, UInt128 amount, ulong reference)
    {
        var externalBalanceDecimal = await TryGetExternalAccountBalance(details.GetAccountUrl, details.CreateAccountUrl, details.NotifyUrl);
        if (externalBalanceDecimal == null)
            return NotificationResult.Rejected;

        var externalBalanceCents = (UInt128)(100 * externalBalanceDecimal);

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
        catch (Exception e)
        {
            logger.LogError($"Failed to transfer to commercial bank: {e}");
            return NotificationResult.Failed;
        }

        if ((int)response.StatusCode / 100 == 2)
            return NotificationResult.Succeeded;

        if ((int)response.StatusCode / 100 == 4)
            return NotificationResult.Rejected;

        return NotificationResult.Failed;
    }

    public async Task<NotificationResult> TryExternalTransfer(Bank bank, UInt128 from, UInt128 to, UInt128 amount, ulong reference)
    {
        if (!options.Value.Banks.TryGetValue(bank, out var bankDetails))
            return NotificationResult.Rejected;

        NotificationResult result = NotificationResult.Rejected;

        for (int i = 0; i < options.Value.RetryCount; i++)
        {
            result = await TryExternalTransferInternal(bankDetails, from, to, amount, reference).ConfigureAwait(false);

            if (result == NotificationResult.Succeeded)
                return result;

            await Task.Delay((int)options.Value.DelaySeconds * 1000);
        }

        return result;
    }
}
