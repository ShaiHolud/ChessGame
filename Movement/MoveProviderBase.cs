using ChessGame.Core.Model;
using ChessGame.Core.Pieces;

namespace ChessGame.Core.Movement
{
    public abstract class MoveProviderBase : IMoveProvider
    {
        public abstract IReadOnlyCollection<Position> GetMoves(
            ChessPiece piece,
            Board board);
    }
}
