namespace RmtoSync.Services;

using RmtoSync.Its;

public sealed class RahdariSendException : Exception
{
    public long PhotoId { get; }
    public long? ItsErrorCode { get; }

    /// <summary>
    /// A deterministic business-validation rejection (ITS 102–129, 191, 1011).
    /// Repeating the identical payload cannot succeed; the record must be corrected
    /// and explicitly re-queued instead of consuming the retry budget.
    /// </summary>
    public bool IsPermanent => ItsErrorCode is { } code && ItsErrorCodes.IsPermanentTtoValidation(code);

    public RahdariSendException(long photoId, string message, long? itsErrorCode = null)
        : base(message)
    {
        PhotoId = photoId;
        ItsErrorCode = itsErrorCode;
    }

    public RahdariSendException(long photoId, string message, Exception innerException, long? itsErrorCode = null)
        : base(message, innerException)
    {
        PhotoId = photoId;
        ItsErrorCode = itsErrorCode;
    }
}
