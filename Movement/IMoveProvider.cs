using ChessGame.Core.Model;
using ChessGame.Core.Pieces;

namespace ChessGame.Core.Movement
{
    public interface IMoveProvider
    {
        IReadOnlyCollection<Position> GetMoves(
            ChessPiece piece,
            Board board);
    }
}
