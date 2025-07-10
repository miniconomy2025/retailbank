namespace RetailBank.Exceptions;

public class IdempotencyException : UserException
{
    public IdempotencyException() : base(
        StatusCodes.Status422UnprocessableEntity,
        "Idempotency Error",
        "An identical request has been made recently."
    )
    { }
}
