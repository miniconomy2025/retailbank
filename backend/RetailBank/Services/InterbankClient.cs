using Microsoft.Extensions.Options;
using RetailBank.Models;
using RetailBank.Models.Interbank;
using RetailBank.Models.Options;

namespace RetailBank.Services;

public class InterbankClient(HttpClient httpClient, IOptions<InterbankNotificationOptions> options) : IInterbankClient
{
    private async Task<NotificationResult> TryNotifyInternal(BankId bank, string transactionId, ulong from, ulong to, UInt128 amount)
    {
        switch (bank)
        {
            case BankId.Commercial:
                var commercialNotification = new CommercialBankNotification(
                    transactionId,
                    from.ToString(),
                    to.ToString(),
                    amount,
                    "Retail Bank Transfer"
                );

                HttpResponseMessage response;
                try
                {
                    response = await httpClient.PostAsJsonAsync(options.Value.CommercialBank, commercialNotification);
                }
                catch
                {
                    return NotificationResult.NetworkFailure;
                }

                try
                {
                    response.EnsureSuccessStatusCode();
                }
                catch
                {
                    return NotificationResult.Failed;
                }

                return NotificationResult.Succeeded;
            default:
                return NotificationResult.Failed;
        }
    }

    public async Task<NotificationResult> TryNotify(BankId bank, string transactionId, ulong from, ulong to, UInt128 amount)
    {
        NotificationResult result = NotificationResult.Failed;
        
        for (int i = 0; i < options.Value.RetryCount; i++)
        {
            result = await TryNotifyInternal(bank, transactionId, from, to, amount).ConfigureAwait(false);

            if (result == NotificationResult.Succeeded)
                return result;

            await Task.Delay((int)options.Value.DelaySeconds * 1000);
        }

        return result;
    }
}
