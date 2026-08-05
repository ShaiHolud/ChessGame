using ChessGame.Model;
using ChessGame.Movement;
namespace ChessGame.Pieces
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
