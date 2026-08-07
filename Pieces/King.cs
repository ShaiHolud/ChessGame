using ChessGame.Core.Model;
using ChessGame.Core.Movement;

namespace ChessGame.Core.Pieces
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