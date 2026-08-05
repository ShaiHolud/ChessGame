using ChessGame.Model;
using ChessGame.Movement;
namespace ChessGame.Pieces
{
    public class Queen : ChessPiece
    {
        private static readonly Direction[] QueenDirections =
        {
            // ладья
            new(1, 0),
            new(-1, 0),
            new(0, 1),
            new(0, -1),

        // слон
            new(1, 1),
            new(1, -1),
            new(-1, 1),
            new(-1, -1)
        };

        public Queen(
            PieceColor color,
            Position position)
            : base(
                color,
                position,
                new SlidingMoveProvider(QueenDirections))
        {
        }

        public override char ShortName => 'Q';
    }
}