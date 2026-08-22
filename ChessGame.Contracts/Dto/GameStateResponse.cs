namespace ChessGame.Contracts.Dto
{
    public sealed class GameStateResponse
    {
        public required Guid Id { get; init; }

        public required string CurrentTurn { get; init; }

        public bool Finished { get; init; }

        public string State { get; init; } = string.Empty;

        public required List<PieceDto> Pieces { get; init; }
    }
}
