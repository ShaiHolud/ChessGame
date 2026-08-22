namespace ChessGame.Contracts.Dto
{
    public sealed class GameInfoDto
    {
        public Guid Id { get; init; }

        public string CurrentTurn { get; init; } = "";

        public DateTime LastActivity { get; init; }

        public int WhitePieces { get; init; }

        public int BlackPieces { get; init; }

        public bool Finished { get; init; }

        public string State { get; init; } = string.Empty;
    }
}
