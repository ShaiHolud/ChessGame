using ChessGame.Model;
using ChessGame.Movement;
namespace ChessGame.Pieces
{
    public class Rook : ChessPiece
    {
        private static readonly Direction[] RookDirections =
        {
            new(1, 0),
            new(-1, 0),
            new(0, 1),
            new(0, -1)
        };

        public Rook(
            PieceColor color,
            Position position)
            : base(
                color,
                position,
                new SlidingMoveProvider(RookDirections))
        {
        }

        public override char ShortName => 'R';
    }
}