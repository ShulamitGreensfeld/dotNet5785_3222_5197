namespace DO;

[Serializable]
public class DalDoesNotExistException : Exception
{
    public DalDoesNotExistException(string? message) : base(message) { }
}

[Serializable]
public class DalAlreadyExistsException : Exception
{
    public DalAlreadyExistsException(string? message) : base(message) { }
}

[Serializable]
public class DalDeletionImpossibleException : Exception
{
    public DalDeletionImpossibleException(string? message) : base(message) { }
}
[Serializable]
public class DalXMLFileLoadCreateException : Exception
{
    public DalXMLFileLoadCreateException(string? message) : base(message) { }
}

[Serializable]

public class NullException : Exception
{
    public NullException(string? message) : base(message) { }
}

[Serializable]

public class DataAccessException : Exception
{
    public DataAccessException(string? message) : base(message) { }
}

public class DalEntityNotFoundException : Exception
{
    public DalEntityNotFoundException(string? message) : base(message) { }
}

public class DalEntityAlreadyExistsException : Exception
{
    public DalEntityAlreadyExistsException(string? message) : base(message) { }
}