using Microsoft.Extensions.Options;
using RetailBank.Extensions;
using RetailBank.Models.Interbank;
using RetailBank.Models.Ledger;
using RetailBank.Models.Options;

namespace RetailBank.Services;

public class InterbankClient(HttpClient httpClient, IOptions<InterbankNotificationOptions> options) : IInterbankClient
{
    private async Task<NotificationResult> TryNotifyInternal(BankId bank, UInt128 transactionId, UInt128 from, UInt128 to, UInt128 amount, ulong? reference)
    {
        switch (bank)
        {
            case BankId.Commercial:
                var commercialNotification = new CommercialBankNotification(
                    transactionId.ToHex(),
                    from.ToString(),
                    to.ToString(),
                    amount,
                    reference?.ToString() ?? "Retail Bank Transfer"
                );

                HttpResponseMessage response;
                try
                {
                    response = await httpClient.PostAsJsonAsync(options.Value.CommercialBank, commercialNotification);
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
            default:
                return NotificationResult.Rejected;
        }
    }

    public async Task<NotificationResult> TryNotify(BankId bank, UInt128 transactionId, UInt128 from, UInt128 to, UInt128 amount, ulong? reference)
    {
        NotificationResult result = NotificationResult.Rejected;
        
        for (int i = 0; i < options.Value.RetryCount; i++)
        {
            result = await TryNotifyInternal(bank, transactionId, from, to, amount, reference).ConfigureAwait(false);

            if (result == NotificationResult.Succeeded)
                return result;

            await Task.Delay((int)options.Value.DelaySeconds * 1000);
        }

        return result;
    }
}
