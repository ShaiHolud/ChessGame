using ChessGame.Model;
using ChessGame.Movement;
namespace ChessGame.Pieces
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
