using ChessGame.Core.Model;
using ChessGame.Core.Pieces;
using global::ChessGame.Core;

namespace ChessGame.Web.Services
{
    public sealed class ChessGameService
    {
        public Game Game { get; }

        public Board Board { get; }

        public ChessGameService()
        {
            Board = CreateBoard();
            Game = new Game(Board);
        }

        public ChessPiece? GetPiece(Position position)
        {
            return Board.GetPiece(position);
        }

        public void Undo()
        {
            Game.Undo();
        }

        public PieceColor CurrentTurn => Game.CurrentTurn;

        public GameState State => Game.State;

        private static Board CreateBoard()
        {
            Board board = new();

            // =========================
            // White pieces
            // =========================

            board.AddPiece(
                new Rook(
                    PieceColor.White,
                    new Position(0, 0)));

            board.AddPiece(
                new Knight(
                    PieceColor.White,
                    new Position(0, 1)));

            board.AddPiece(
                new Bishop(
                    PieceColor.White,
                    new Position(0, 2)));

            board.AddPiece(
                new Queen(
                    PieceColor.White,
                    new Position(0, 3)));

            board.AddPiece(
                new King(
                    PieceColor.White,
                    new Position(0, 4)));

            board.AddPiece(
                new Bishop(
                    PieceColor.White,
                    new Position(0, 5)));

            board.AddPiece(
                new Knight(
                    PieceColor.White,
                    new Position(0, 6)));

            board.AddPiece(
                new Rook(
                    PieceColor.White,
                    new Position(0, 7)));

            for (int column = 0; column < 8; column++)
            {
                board.AddPiece(
                    new Pawn(
                        PieceColor.White,
                        new Position(1, column)));
            }

            // =========================
            // Black pieces
            // =========================

            board.AddPiece(
                new Rook(
                    PieceColor.Black,
                    new Position(7, 0)));

            board.AddPiece(
                new Knight(
                    PieceColor.Black,
                    new Position(7, 1)));

            board.AddPiece(
                new Bishop(
                    PieceColor.Black,
                    new Position(7, 2)));

            board.AddPiece(
                new Queen(
                    PieceColor.Black,
                    new Position(7, 3)));

            board.AddPiece(
                new King(
                    PieceColor.Black,
                    new Position(7, 4)));

            board.AddPiece(
                new Bishop(
                    PieceColor.Black,
                    new Position(7, 5)));

            board.AddPiece(
                new Knight(
                    PieceColor.Black,
                    new Position(7, 6)));

            board.AddPiece(
                new Rook(
                    PieceColor.Black,
                    new Position(7, 7)));

            for (int column = 0; column < 8; column++)
            {
                board.AddPiece(
                    new Pawn(
                        PieceColor.Black,
                        new Position(6, column)));
            }

            return board;
        }
    }
}