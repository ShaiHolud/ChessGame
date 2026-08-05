using ChessGame.Model;
using ChessGame.Pieces;

namespace ChessGame.Movement
{
    public interface IMoveProvider
    {
        IReadOnlyCollection<Position> GetMoves(
            ChessPiece piece,
            Board board);
    }
}
