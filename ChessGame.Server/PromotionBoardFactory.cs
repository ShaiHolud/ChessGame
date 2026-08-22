using ChessGame.Core.Model;
using ChessGame.Core.Pieces;
using ChessGame.Core;

namespace ChessGame.Server
{
    public static class PromotionBoardFactory
    {
        public static Board Create()
        {
            Board board = new();

            board.AddPiece(
                new King(
                    PieceColor.White,
                    Position.Parse("E1")));

            board.AddPiece(
                new King(
                    PieceColor.Black,
                    Position.Parse("E8")));

            board.AddPiece(
                new Pawn(
                    PieceColor.White,
                    Position.Parse("A7")));

            return board;
        }
    }
}