using ChessGame.Core.Model;
using ChessGame.Core.Movement;

namespace ChessGame.Core.Pieces
{
    public class Knight : ChessPiece
    {
        public Knight(
            PieceColor color,
            Position position)
            : base(
                color,
                position,
                new KnightMoveProvider())
        {
        }
        public override char ShortName => 'N';
    }
}
