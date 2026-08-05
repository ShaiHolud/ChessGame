using ChessGame.Model;
using ChessGame.Pieces;

namespace ChessGame.Movement
{
    public class PawnMoveProvider : MoveProviderBase
    {
        public override IReadOnlyCollection<Position> GetMoves(ChessPiece piece, Board board)
        {
            Pawn pawn = (Pawn)piece;
            List<Position> moves = new();

            int direction = pawn.Color == PieceColor.White ? 1 : -1;

            Direction forward = new(direction, 0);
            Direction doubleForward = new(direction * 2, 0);
            Direction leftAttack = new(direction, -1);
            Direction rightAttack = new(direction, 1);

            Position first = pawn.Position.Offset(forward);

            if (board.Contains(first) && board.IsCellFree(first))
                moves.Add(first);

            Position second = pawn.Position.Offset(doubleForward);
            if (pawn.MoveCount == 0 && board.Contains(second) && board.IsCellFree(second))
            {
                moves.Add(second);
            }

            Position left = pawn.Position.Offset(leftAttack);
            if (board.Contains(left))
            {
                ChessPiece? target = board.GetPiece(left);

                if (target != null &&
                    target.Color != piece.Color)
                {
                    moves.Add(left);
                }
            }

            Position right = pawn.Position.Offset(rightAttack);

            if (board.Contains(right))
            {
                ChessPiece? target = board.GetPiece(right);

                if (target != null &&
                    target.Color != piece.Color)
                {
                    moves.Add(right);
                }
            }

            return moves;
        }
    }
}
