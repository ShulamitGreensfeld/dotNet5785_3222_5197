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
