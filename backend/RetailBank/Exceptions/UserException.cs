namespace RetailBank.Exceptions;

/// <summary>
/// Marks that an exception can be safely displayed to the user.
/// </summary>
public abstract class UserException : Exception {
    public ushort Status { get; private init; }
    public string Title { get; private init; }

    public UserException(ushort status, string title, string message) : base(message) {
        Status = status;
        Title = title;
    }
}
