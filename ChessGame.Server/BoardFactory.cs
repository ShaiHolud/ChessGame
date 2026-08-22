using ChessGame.Core;
using ChessGame.Core.Model;
using ChessGame.Core.Pieces;

namespace ChessGame.Server
{
    public static class InitialBoardFactory
    {
        public static Board Create()
        {
            Board board = new();

            // Белые
            board.AddPiece(new King(PieceColor.White, Position.Parse("E1")));
            board.AddPiece(new Queen(PieceColor.White, Position.Parse("D1")));
            board.AddPiece(new Rook(PieceColor.White, Position.Parse("A1")));
            board.AddPiece(new Rook(PieceColor.White, Position.Parse("H1")));
            board.AddPiece(new Bishop(PieceColor.White, Position.Parse("C1")));
            board.AddPiece(new Bishop(PieceColor.White, Position.Parse("F1")));
            board.AddPiece(new Knight(PieceColor.White, Position.Parse("B1")));
            board.AddPiece(new Knight(PieceColor.White, Position.Parse("G1")));


            // Чёрные
            board.AddPiece(new King(PieceColor.Black, Position.Parse("E8")));
            board.AddPiece(new Queen(PieceColor.Black, Position.Parse("D8")));
            board.AddPiece(new Rook(PieceColor.Black, Position.Parse("A8")));
            board.AddPiece(new Rook(PieceColor.Black, Position.Parse("H8")));
            board.AddPiece(new Bishop(PieceColor.Black, Position.Parse("C8")));
            board.AddPiece(new Bishop(PieceColor.Black, Position.Parse("F8")));
            board.AddPiece(new Knight(PieceColor.Black, Position.Parse("B8")));
            board.AddPiece(new Knight(PieceColor.Black, Position.Parse("G8")));

            //Пешки
            foreach (var file in "ABCDEFGH")
            {
                board.AddPiece(new Pawn(PieceColor.White, Position.Parse($"{file}2")));
                board.AddPiece(new Pawn(PieceColor.Black, Position.Parse($"{file}7")));
            }

            return board;
        }
    }
}
