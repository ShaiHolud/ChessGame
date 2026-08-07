using ChessGame.Core.Model;
using ChessGame.Core.Movement;

namespace ChessGame.Core.Pieces
{
    public class Bishop : ChessPiece
    {
        private static readonly Direction[] BishopDirections =
        {
            new( 1, 1),
            new( 1,-1),
            new(-1, 1),
            new(-1,-1)
        };

        public Bishop(
            PieceColor color,
            Position position)
            : base(
                color,
                position,
                new SlidingMoveProvider(BishopDirections))
        {
        }

        public override char ShortName => 'B';
    }
}
