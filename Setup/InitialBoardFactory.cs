using ChessGame.Core.Model;
using ChessGame.Core.Pieces;

namespace ChessGame.Core.Setup
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
            board.AddPiece(new Pawn(PieceColor.White, Position.Parse("A2")));
            board.AddPiece(new Pawn(PieceColor.White, Position.Parse("B2")));
            board.AddPiece(new Pawn(PieceColor.White, Position.Parse("C2")));
            board.AddPiece(new Pawn(PieceColor.White, Position.Parse("D2")));
            board.AddPiece(new Pawn(PieceColor.White, Position.Parse("E2")));
            board.AddPiece(new Pawn(PieceColor.White, Position.Parse("F2")));
            board.AddPiece(new Pawn(PieceColor.White, Position.Parse("G2")));
            board.AddPiece(new Pawn(PieceColor.White, Position.Parse("H2")));

            // Чёрные
            board.AddPiece(new King(PieceColor.Black, Position.Parse("E8")));
            board.AddPiece(new Queen(PieceColor.Black, Position.Parse("D8")));
            board.AddPiece(new Rook(PieceColor.Black, Position.Parse("A8")));
            board.AddPiece(new Rook(PieceColor.Black, Position.Parse("H8")));
            board.AddPiece(new Bishop(PieceColor.Black, Position.Parse("C8")));
            board.AddPiece(new Bishop(PieceColor.Black, Position.Parse("F8")));
            board.AddPiece(new Knight(PieceColor.Black, Position.Parse("B8")));
            board.AddPiece(new Knight(PieceColor.Black, Position.Parse("G8")));
            board.AddPiece(new Pawn(PieceColor.Black, Position.Parse("A7")));
            board.AddPiece(new Pawn(PieceColor.Black, Position.Parse("B7")));
            board.AddPiece(new Pawn(PieceColor.Black, Position.Parse("C7")));
            board.AddPiece(new Pawn(PieceColor.Black, Position.Parse("D7")));
            board.AddPiece(new Pawn(PieceColor.Black, Position.Parse("E7")));
            board.AddPiece(new Pawn(PieceColor.Black, Position.Parse("F7")));
            board.AddPiece(new Pawn(PieceColor.Black, Position.Parse("G7")));
            board.AddPiece(new Pawn(PieceColor.Black, Position.Parse("H7")));

            return board;
        }
    }
}
