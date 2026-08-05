using ChessGame.Model;
using ChessGame.Pieces;

namespace ChessGame.Movement
{
    public abstract class MoveProviderBase : IMoveProvider
    {
        public abstract IReadOnlyCollection<Position> GetMoves(
            ChessPiece piece,
            Board board);
    }
}
