using ChessGame.Core.Events;

namespace ChessGame.Core.Results
{
    public sealed class MoveResult
    {
        public bool Success { get; init; }

        public string? Error { get; init; }

        public List<GameEvent> Events { get; } = [];
    }
}
