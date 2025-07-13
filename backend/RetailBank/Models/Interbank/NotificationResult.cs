namespace RetailBank.Models.Interbank;

public enum NotificationResult
{
    UnknownFailure = 0,
    Succeeded = 1,
    Rejected = 2,
    AccountNotFound = 4 | Rejected,
}
