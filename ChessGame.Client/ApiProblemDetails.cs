namespace ChessGame.Client
{
    internal sealed class ApiProblemDetails
    {
        public string? Title { get; init; }

        public string? Detail { get; init; }

        public int? Status { get; init; }
    }
}
