using ChessGame.Model;
using ChessGame.Movement;
namespace ChessGame.Pieces
{
    public abstract class ChessPiece
    {
        public PieceColor Color { get; }

        public Position Position { get; private set; }

        public abstract char ShortName { get; }

        public int MoveCount { get; private set; }

        public IMoveProvider MoveProvider { get; }

        public bool IsAlive { get; private set; } = true;

        internal void Capture()
        {
            IsAlive = false;
        }

        public char ColorCode => Color == PieceColor.White ? 'W' : 'B';

        protected ChessPiece(PieceColor color, Position position, IMoveProvider moveProvider)
        {
            Color = color;
            Position = position;
            MoveProvider = moveProvider;
        }

        internal void MoveTo(Position position)
        {
            Position = position;
            MoveCount++;
        }

        public IReadOnlyCollection<Position> GetPossibleMoves(Board board)
        {
            return MoveProvider.GetMoves(this, board);
        }

        internal void Restore(Position position, int moveCount, bool isAlive)
        {
            Position = position;
            MoveCount = moveCount;
            IsAlive = isAlive;
        }
    }

    public enum PieceColor
    {
        White,
        Black
    }
}
