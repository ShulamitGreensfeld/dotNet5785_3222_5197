
namespace BO;

/// <summary>
/// An exception for when trying to access a not-existing item.
/// </summary>
[Serializable]
public class BlDoesNotExistException : Exception
{
    public BlDoesNotExistException(string? message) : base(message) { }
    public BlDoesNotExistException(string message, Exception innerException)
                : base(message, innerException) { }
}

/// <summary>
/// An exception for when trying to add an existing item.
/// </summary>
[Serializable]
public class BlAlreadyExistException : Exception
{
    public BlAlreadyExistException(string? message) : base(message) { }
    public BlAlreadyExistException(string message, Exception innerException)
                : base(message, innerException) { }
}

/// <summary>
/// An exception fot when trying to delete an item that is not allowed to be deleted.
/// /// </summary>
[Serializable]
public class BlDeletionException : Exception
{
    public BlDeletionException(string? message) : base(message) { }
    public BlDeletionException(string message, Exception innerException) : base(message, innerException) { }

}

/// <summary>
/// An exception fot when entering invalid input for an instance of one of the BO entities.
/// /// </summary>
[Serializable]
public class BlInvalidFormatException : Exception
{
    public BlInvalidFormatException(string? message) : base(message) { }
    public BlInvalidFormatException(string message, Exception innerException) : base(message, innerException) { }

}

/// <summary>
/// An exception for when trying to do an action that is forbidden for the one who requested to.
/// </summary>
[Serializable]
public class BlUnauthorizedException : Exception
{
    public BlUnauthorizedException(string? message) : base(message) { }
    public BlUnauthorizedException(string message, Exception innerException) : base(message, innerException) { }

}

/// <summary>
/// General exception in the Bl.
/// </summary>
[Serializable]
public class BlGeneralException : Exception
{
    public BlGeneralException(string message) : base(message) { }
    public BlGeneralException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>
/// Null exception in the Bl.
/// </summary>
[Serializable]
public class BlNullPropertyException : Exception
{
    public BlNullPropertyException(string? message) : base(message) { }
}

/// <summary>
/// logging exception in the Bl.
/// </summary>
[Serializable]
public class BlInvalidInputException : Exception
{
    public BlInvalidInputException(string? message) : base(message) { }
}

/// <summary>
/// logging exception in the Bl.
/// </summary>
[Serializable]
    public class BlNotFoundException : Exception
    {
        public BlNotFoundException() { }
        public BlNotFoundException(string message) : base(message) { }
        public BlNotFoundException(string message, Exception inner) : base(message, inner) { }
    }