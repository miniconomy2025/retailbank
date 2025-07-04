namespace RetailBank.Models.Options;

public class InterbankNotificationOptions
{
    public const string Section = "InterbankNotification";

    public required uint RetryCount { get; init; } = 3;
    public required uint DelaySeconds { get; init; } = 15;
    public required string CommercialBank { get; init; }
}
