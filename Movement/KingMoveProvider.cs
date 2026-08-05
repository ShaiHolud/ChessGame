using ChessGame.Model;
using ChessGame.Pieces;

namespace ChessGame.Movement
{
    internal class KingMoveProvider : MoveProviderBase
    {
        public override IReadOnlyCollection<Position> GetMoves(ChessPiece piece, Board board)
        {
            List<Position> moves = new();
            foreach (Direction direction in Directions)
            {
                Position position = piece.Position.Offset(direction);
                if (board.CanMoveTo(piece, position))
                {
                    ChessPiece? target = board.GetPiece(position);

                    if (target != null && target.Color != piece.Color || target == null)
                    {
                        moves.Add(position);
                    }
                }
            }
            return moves;
        }

        private static readonly Direction[] Directions =
        {
            new(1,-1),
            new(1,0),
            new(1,1),
            new(0,-1),
            new(0,1),
            new(-1,-1),
            new(-1,0),
            new(-1,1)
        };
    }
}
