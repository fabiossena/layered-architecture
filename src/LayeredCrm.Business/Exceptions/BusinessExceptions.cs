namespace LayeredCrm.Business.Exceptions;

/// <summary>
/// Business rule violation (mapped to HTTP 400/409 by the presentation layer).
/// </summary>
public class BusinessException : Exception
{
    public BusinessException(string message)
        : base(message)
    {
    }

    public BusinessException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}

/// <summary>
/// Requested resource does not exist (mapped to HTTP 404).
/// </summary>
public class NotFoundException : BusinessException
{
    public NotFoundException(string resource, object key)
        : base($"{resource} '{key}' was not found.")
    {
    }
}

/// <summary>
/// Validation failure carrying the list of errors per field.
/// </summary>
public sealed class ValidationException : BusinessException
{
    public IReadOnlyList<string> Errors { get; }

    public ValidationException(IReadOnlyList<string> errors)
        : base("Validation failed. See the detailed errors.")
    {
        Errors = errors;
    }
}
