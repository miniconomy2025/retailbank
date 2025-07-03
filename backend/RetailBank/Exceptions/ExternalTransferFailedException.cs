namespace RetailBank.Exceptions;

public class ExternalTransferFailedException : UserException
{
    public ExternalTransferFailedException() : base(StatusCodes.Status503ServiceUnavailable, "External Transfer Failed", "Failed to transfer to commercial bank, downstream service is unavailable.") { }
}
