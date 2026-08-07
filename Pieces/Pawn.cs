using ChessGame.Core.Model;
using ChessGame.Core.Movement;

namespace ChessGame.Core.Pieces
{
    public class Pawn : ChessPiece
    {
        public Pawn(
            PieceColor color,
            Position position)
            : base(
                color,
                position,
                new PawnMoveProvider())
        {
        }

        public override char ShortName => 'P';
    }
}
