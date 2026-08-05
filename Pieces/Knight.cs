using ChessGame.Model;
using ChessGame.Movement;
namespace ChessGame.Pieces
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
