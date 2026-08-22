namespace ChessGame.Client;

public sealed class ApiException : Exception
{
    public int StatusCode { get; }

    public ApiException(int statusCode, string message)
        : base(message)
    {
        StatusCode = statusCode;
    }
}