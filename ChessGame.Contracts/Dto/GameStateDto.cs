using ChessGame.Core.Model;
using ChessGame.Core.Pieces;

namespace ChessGame.Contracts.Dto
{
    public sealed class GameStateDto
    {
        public Guid Id { get; init; }

        public PieceColor CurrentTurn { get; init; }

        public bool Check { get; init; }

        public bool Checkmate { get; init; }

        public bool Stalemate { get; init; }

        public Move? LastMove { get; init; }

        public List<PieceDto> Pieces { get; init; } = [];
    }
}
