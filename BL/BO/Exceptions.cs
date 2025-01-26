namespace BO;

/// <summary>
/// Exception for when a requested entity is not found
/// </summary>
[Serializable]
public class BlDoesNotExistException : Exception
{
    public BlDoesNotExistException(string? message) : base(message) { }
    public BlDoesNotExistException(string message, Exception innerException)
        : base(message, innerException) { }
}

/// <summary>
/// Exception for when attempting to create an entity that already exists
/// </summary>
[Serializable]
public class BlAlreadyExistsException : Exception
{
    public BlAlreadyExistsException(string? message) : base(message) { }
    public BlAlreadyExistsException(string message, Exception innerException)
        : base(message, innerException) { }
}

/// <summary>
/// Exception for invalid input validation
/// </summary>
[Serializable]
public class BlInvalidInputException : Exception
{
    public BlInvalidInputException(string? message) : base(message) { }
    public BlInvalidInputException(string message, Exception innerException)
        : base(message, innerException) { }
}

/// <summary>
/// Exception for system configuration errors
/// </summary>
[Serializable]
public class BlConfigurationException : Exception
{
    public BlConfigurationException(string? message) : base(message) { }
    public BlConfigurationException(string message, Exception innerException)
        : base(message, innerException) { }
}

/// <summary>
/// Exception for database-related errors
/// </summary>
[Serializable]
public class BlDatabaseException : Exception
{
    public BlDatabaseException(string? message) : base(message) { }
    public BlDatabaseException(string message, Exception innerException)
        : base(message, innerException) { }
}

/// <summary>
/// Exception for general system errors
/// </summary>
[Serializable]
public class BlSystemException : Exception
{
    public BlSystemException(string? message) : base(message) { }
    public BlSystemException(string message, Exception innerException)
        : base(message, innerException) { }
}

/// <summary>
/// Exception for null property validation
/// </summary>
[Serializable]
public class BlNullPropertyException : Exception
{
    public BlNullPropertyException(string? message) : base(message) { }
    public BlNullPropertyException(string message, Exception innerException)
        : base(message, innerException) { }
}

/// <summary>
/// Exception for ArgumentExceptions
/// </summary>
[Serializable]
public class BlArgumentException : Exception
{
    public BlArgumentException(string? message) : base(message) { }
    public BlArgumentException(string message, Exception innerException)
        : base(message, innerException) { }
}

/// <summary>
/// Exception for NullReferenceExceptions
/// </summary>
[Serializable]
public class BlNullReferenceException : Exception
{
    public BlNullReferenceException(string? message) : base(message) { }
    public BlNullReferenceException(string message, Exception innerException)
        : base(message, innerException) { }
}

/// <summary>
/// Exception for general exceptions
/// </summary>
[Serializable]
public class BlGeneralException : Exception
{
    public BlGeneralException(string? message) : base(message) { }
    public BlGeneralException(string message, Exception innerException)
        : base(message, innerException) { }
}

[Serializable]
public class InvalidOperationException : Exception
{
    public InvalidOperationException(string? message) : base(message) { }
    public InvalidOperationException(string message, Exception innerException)
        : base(message, innerException) { }
}

[Serializable]
public class DataAccessException : Exception
{
    public DataAccessException(string? message) : base(message) { }
    public DataAccessException(string message, Exception innerException)
        : base(message, innerException) { }
}

[Serializable]
public class NotFoundException : Exception
{
    public NotFoundException(string? message) : base(message) { }
    public NotFoundException(string message, Exception innerException)
        : base(message, innerException) { }
}

[Serializable]
public class ValidationException : Exception
{
    public ValidationException(string? message) : base(message) { }
    public ValidationException(string message, Exception innerException)
        : base(message, innerException) { }
}

[Serializable]
public class InvalidCredentialsException : Exception
{
    public InvalidCredentialsException(string? message) : base(message) { }
    public InvalidCredentialsException(string message, Exception innerException)
        : base(message, innerException) { }
}

[Serializable]
public class BlInvalidArgumentException : Exception
{
    public BlInvalidArgumentException(string? message) : base(message) { }
    public BlInvalidArgumentException(string message, Exception innerException)
        : base(message, innerException) { }
}

[Serializable]
public class BlInvalidOperationException : Exception
{
    public BlInvalidOperationException(string? message) : base(message) { }
    public BlInvalidOperationException(string message, Exception innerException)
        : base(message, innerException) { }
}

[Serializable]
public class BlInternalErrorException : Exception
{
    public BlInternalErrorException(string? message) : base(message) { }
    public BlInternalErrorException(string message, Exception innerException)
        : base(message, innerException) { }
}

public class BlInvalidLoginException : Exception
{
    public BlInvalidLoginException(string message) : base(message) { }
}

public class BlEntityNotFoundException : Exception
{
    public BlEntityNotFoundException(string message, Exception innerException) : base(message, innerException) { }
}


public class BlUnauthorizedAccessException : Exception
{
    public BlUnauthorizedAccessException(string message) : base(message) { }
}
public class BlDuplicateEntityException : Exception
{
    public BlDuplicateEntityException(string message, Exception? innerException = null) : base(message, innerException) { }
}

public class BlDataAccessException : Exception
{
    public BlDataAccessException(string message, Exception innerException) : base(message, innerException) { }
}