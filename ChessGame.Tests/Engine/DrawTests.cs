using ChessGame.Core.Model;
using ChessGame.Core.Pieces;
using ChessGame.Core;
using ChessGame.Core.Results;

namespace ChessGame.Tests.Engine
{
    public class DrawTests
    {
        [Fact]
        public void Draw_By_Insufficient_Material_King_Vs_King()
        {
            // Arrange
            Board board = new();
            Game game = new(board);

            board.AddPiece(
                new King(
                    PieceColor.White,
                    Position.Parse("E1")));

            board.AddPiece(
                new King(
                    PieceColor.Black,
                    Position.Parse("E8")));

            // Act
            bool isDraw =
                game.IsDrawByInsufficientMaterial();

            // Assert
            Assert.True(isDraw);
        }

        [Fact]
        public void Draw_By_Insufficient_Material_King_And_Bishop_Vs_King()
        {
            // Arrange
            Board board = new();
            Game game = new(board);

            board.AddPiece(
                new King(
                    PieceColor.White,
                    Position.Parse("E1")));

            board.AddPiece(
                new Bishop(
                    PieceColor.White,
                    Position.Parse("C1")));

            board.AddPiece(
                new King(
                    PieceColor.Black,
                    Position.Parse("E8")));

            // Act
            bool isDraw =
                game.IsDrawByInsufficientMaterial();

            // Assert
            Assert.True(isDraw);
        }

        [Fact]
        public void Draw_By_Insufficient_Material_King_And_Knight_Vs_King()
        {
            // Arrange
            Board board = new();
            Game game = new(board);

            board.AddPiece(
                new King(
                    PieceColor.White,
                    Position.Parse("E1")));

            board.AddPiece(
                new Knight(
                    PieceColor.White,
                    Position.Parse("B1")));

            board.AddPiece(
                new King(
                    PieceColor.Black,
                    Position.Parse("E8")));

            // Act
            bool isDraw =
                game.IsDrawByInsufficientMaterial();

            // Assert
            Assert.True(isDraw);
        }

        [Fact]
        public void Draw_By_Insufficient_Material_King_And_Rook_Vs_King_Should_Be_False()
        {
            // Arrange
            Board board = new();
            Game game = new(board);

            board.AddPiece(
                new King(
                    PieceColor.White,
                    Position.Parse("E1")));

            board.AddPiece(
                new Rook(
                    PieceColor.White,
                    Position.Parse("A1")));

            board.AddPiece(
                new King(
                    PieceColor.Black,
                    Position.Parse("E8")));

            // Act
            bool isDraw =
                game.IsDrawByInsufficientMaterial();

            // Assert
            Assert.False(isDraw);
        }

        [Fact]
        public void Draw_By_Insufficient_Material_King_And_Queen_Vs_King_Should_Be_False()
        {
            // Arrange
            Board board = new();
            Game game = new(board);

            board.AddPiece(
                new King(
                    PieceColor.White,
                    Position.Parse("E1")));

            board.AddPiece(
                new Queen(
                    PieceColor.White,
                    Position.Parse("D1")));

            board.AddPiece(
                new King(
                    PieceColor.Black,
                    Position.Parse("E8")));

            // Act
            bool isDraw =
                game.IsDrawByInsufficientMaterial();

            // Assert
            Assert.False(isDraw);
        }

        [Fact]
        public void Draw_By_Insufficient_Material_Two_Bishops_Vs_King_Should_Be_False()
        {
            // Arrange
            Board board = new();
            Game game = new(board);

            board.AddPiece(
                new King(
                    PieceColor.White,
                    Position.Parse("E1")));

            board.AddPiece(
                new Bishop(
                    PieceColor.White,
                    Position.Parse("C1")));

            board.AddPiece(
                new Bishop(
                    PieceColor.White,
                    Position.Parse("F1")));

            board.AddPiece(
                new King(
                    PieceColor.Black,
                    Position.Parse("E8")));

            // Act
            bool isDraw =
                game.IsDrawByInsufficientMaterial();

            // Assert
            Assert.False(isDraw);
        }

        [Fact]
        public void InsufficientMaterial_Should_Not_Detect_Rook_As_Draw()
        {
            // Arrange
            Board board = new();
            Game game = new(board);

            board.AddPiece(
                new King(
                    PieceColor.White,
                    Position.Parse("E1")));

            board.AddPiece(
                new Rook(
                    PieceColor.White,
                    Position.Parse("A1")));

            board.AddPiece(
                new King(
                    PieceColor.Black,
                    Position.Parse("E8")));

            // Act
            bool isDraw =
                game.IsDrawByInsufficientMaterial();

            // Assert
            Assert.False(isDraw);
        }

        [Fact]
        public void Move_Should_Set_GameState_To_Draw_By_Insufficient_Material()
        {
            // Arrange
            Board board = new();
            Game game = new(board);

            board.AddPiece(
                new King(
                    PieceColor.White,
                    Position.Parse("E1")));

            board.AddPiece(
                new King(
                    PieceColor.Black,
                    Position.Parse("E8")));

            // Act
            MoveResult result =
                game.Move(
                    new Move(
                        Position.Parse("E1"),
                        Position.Parse("D1")));

            // Assert
            Assert.True(result.Success);
            Assert.True(result.Draw);

            Assert.Equal(
                GameState.Draw,
                game.State);
        }

        [Fact]
        public void Move_After_Draw_Should_Be_Rejected()
        {
            // Arrange
            Board board = new();
            Game game = new(board);

            board.AddPiece(
                new King(
                    PieceColor.White,
                    Position.Parse("E1")));

            board.AddPiece(
                new King(
                    PieceColor.Black,
                    Position.Parse("E8")));

            // Первый ход приводит к ничьей
            game.Move(
                new Move(
                    Position.Parse("E1"),
                    Position.Parse("D1")));

            Assert.Equal(
                GameState.Draw,
                game.State);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(
                () =>
                    game.Move(
                        new Move(
                            Position.Parse("E8"),
                            Position.Parse("D8"))));
        }

        [Fact]
        public void Undo_After_Draw_Should_Restore_Normal_Game_State()
        {
            // Arrange
            Board board = new();
            Game game = new(board);

            board.AddPiece(
                new King(
                    PieceColor.White,
                    Position.Parse("E1")));

            board.AddPiece(
                new King(
                    PieceColor.Black,
                    Position.Parse("E8")));

            Move move = new(
                Position.Parse("E1"),
                Position.Parse("D1"));

            // Act
            game.Move(move);

            Assert.Equal(
                GameState.Draw,
                game.State);

            game.Undo();

            // Assert
            Assert.Equal(
                GameState.Normal,
                game.State);

            Assert.Equal(
                PieceColor.White,
                game.CurrentTurn);

            ChessPiece? king =
                board.GetPiece(
                    Position.Parse("E1"));

            Assert.NotNull(king);
            Assert.IsType<King>(king);

            Assert.Null(
                board.GetPiece(
                    Position.Parse("D1")));

            Assert.Empty(game.MoveHistory);
        }
    }
}
