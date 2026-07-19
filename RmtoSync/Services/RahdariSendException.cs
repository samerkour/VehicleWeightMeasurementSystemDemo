namespace RmtoSync.Services;

public sealed class RahdariSendException : Exception
{
    public long PhotoId { get; }
    public long? ItsErrorCode { get; }

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
