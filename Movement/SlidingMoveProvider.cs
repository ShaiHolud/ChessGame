using ChessGame.Core.Model;
using ChessGame.Core.Pieces;

namespace ChessGame.Core.Movement
{
    public class SlidingMoveProvider : MoveProviderBase
    {
        private readonly IReadOnlyCollection<Direction> _directions;

        public SlidingMoveProvider(
            IReadOnlyCollection<Direction> directions)
        {
            _directions = directions;
        }

        public override IReadOnlyCollection<Position> GetMoves(
            ChessPiece piece,
            Board board)
        {
            List<Position> moves = new();

            foreach (Direction direction in _directions)
            {
                Position current = piece.Position.Offset(direction);

                while (board.Contains(current))
                {
                    ChessPiece? target = board.GetPiece(current);

                    if (target == null)
                    {
                        moves.Add(current);
                        current = current.Offset(direction);
                        continue;
                    }

                    if (target.Color != piece.Color)
                        moves.Add(current);

                    break;
                }
            }
            return moves;
        }
    }
}