using System.Text.Json.Serialization;

namespace RetailBank.Models.Interbank;

public record CreateCommercialAccountRequest(
    [property: JsonPropertyName("notification_url")]
    string NotificationUrl
);
