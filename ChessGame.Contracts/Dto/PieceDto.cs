using ChessGame.Core.Pieces;

namespace ChessGame.Contracts.Dto
{
    public sealed class PieceDto
    {
        public string Type { get; init; } = "";

        public string Color { get; init; } = "";

        public string Position { get; init; } = "";

        public int MoveCount { get; init; }
    }
}
