namespace PostsAPI.Models;
public enum ErrorType { NotFound, Forbidden}
public record Error(ErrorType Type, string Message);

public record Result(Error? Error = null)
{
    public bool IsSuccess => Error is null;
}

public record Result<T>(T? Value, Error? Error = null)
{
    public bool IsSuccess => Error is null;
}