using ChessGame.Core.Events;

namespace ChessGame.Core.Results
{
    public sealed class MoveResult
    {
        public bool Success { get; init; } = true;

        public List<GameEvent> Events { get; } = [];

        public bool Check { get; set; }

        public bool Checkmate { get; set; }

        public bool Stalemate { get; set; }

        public bool Draw { get; set; }
    }
}
