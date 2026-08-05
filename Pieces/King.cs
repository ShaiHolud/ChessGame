using ChessGame.Model;
using ChessGame.Movement;
namespace ChessGame.Pieces
{
    public class King : ChessPiece
    {
        public King(
            PieceColor color,
            Position position)
            : base(
                color,
                position,
                new KingMoveProvider())
        {
        }
        public override char ShortName => 'K';
    }
}