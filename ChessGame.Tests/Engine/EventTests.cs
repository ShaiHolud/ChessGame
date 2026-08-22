using ChessGame.Core.Events;
using ChessGame.Core.Model;
using ChessGame.Core.Pieces;
using ChessGame.Core.Results;
using ChessGame.Core;

namespace ChessGame.Tests.Engine
{
    public class EventTests
    {
        [Fact]
        public void Move_Should_Return_Capture_Event()
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

            Pawn whitePawn =
                new(
                    PieceColor.White,
                    Position.Parse("E2"));

            Pawn blackPawn =
                new(
                    PieceColor.Black,
                    Position.Parse("D3"));

            board.AddPiece(whitePawn);
            board.AddPiece(blackPawn);

            // Act
            MoveResult result =
                game.Move("E2", "D3");

            // Assert

            // Ход выполнен успешно
            Assert.True(result.Success);

            // Должно быть событие Capture
            GameEvent? captureEvent =
                result.Events.FirstOrDefault(
                    e => e.Type == GameEventType.Capture);

            Assert.NotNull(captureEvent);

            // Проверяем состояние доски
            Assert.Null(
                board.GetPiece(
                    Position.Parse("E2")));

            ChessPiece? piece =
                board.GetPiece(
                    Position.Parse("D3"));

            Assert.NotNull(piece);
            Assert.Same(whitePawn, piece);

            // Взятая фигура должна быть мертва
            Assert.False(blackPawn.IsAlive);

            // Ходившая пешка должна быть жива
            Assert.True(whitePawn.IsAlive);

            // И у нее должен увеличиться MoveCount
            Assert.Equal(1, whitePawn.MoveCount);
        }

        [Fact]
        public void Move_Should_Return_EnPassant_Event()
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

            Pawn whitePawn =
                new(
                    PieceColor.White,
                    Position.Parse("E2"));

            Pawn blackPawn =
                new(
                    PieceColor.Black,
                    Position.Parse("D7"));

            Pawn blackHelperPawn =
                new(
                    PieceColor.Black,
                    Position.Parse("A7"));

            board.AddPiece(whitePawn);
            board.AddPiece(blackPawn);
            board.AddPiece(blackHelperPawn);

            // Act

            // 1. White: E2 -> E4
            game.Move("E2", "E4");

            // 2. Black: A7 -> A6
            game.Move("A7", "A6");

            // 3. White: E4 -> E5
            game.Move("E4", "E5");

            // 4. Black: D7 -> D5
            game.Move("D7", "D5");

            // 5. White: E5 -> D6 en passant
            MoveResult result =
                game.Move("E5", "D6");

            // Assert

            Assert.True(result.Success);

            // Должно быть событие EnPassant
            GameEvent? enPassantEvent =
                result.Events.FirstOrDefault(
                    e => e.Type == GameEventType.EnPassant);

            Assert.NotNull(enPassantEvent);

            // Белая пешка должна оказаться на D6
            ChessPiece? whitePiece =
                board.GetPiece(
                    Position.Parse("D6"));

            Assert.NotNull(whitePiece);
            Assert.Same(whitePawn, whitePiece);
            Assert.Equal(PieceColor.White, whitePiece.Color);

            // Белая пешка должна сделать 3 хода:
            // E2 -> E4
            // E4 -> E5
            // E5 -> D6
            Assert.Equal(3, whitePawn.MoveCount);

            Assert.True(whitePawn.IsAlive);

            // Черная пешка D5 должна быть взята
            Assert.False(blackPawn.IsAlive);

            Assert.Null(
                board.GetPiece(
                    Position.Parse("D5")));

            // Исходная клетка белой пешки должна быть пустой
            Assert.Null(
                board.GetPiece(
                    Position.Parse("E5")));

            // Вспомогательная черная пешка должна остаться
            ChessPiece? helperPawn =
                board.GetPiece(
                    Position.Parse("A6"));

            Assert.NotNull(helperPawn);
            Assert.Same(blackHelperPawn, helperPawn);
        }

        [Fact]
        public void Move_Should_Return_Castle_Event()
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
                    Position.Parse("H1")));

            board.AddPiece(
                new King(
                    PieceColor.Black,
                    Position.Parse("E8")));

            // Act
            MoveResult result =
                game.Move("E1", "G1");

            // Assert
            Assert.True(result.Success);

            GameEvent? castleEvent =
                result.Events.FirstOrDefault(
                    e => e.Type == GameEventType.Castle);

            Assert.NotNull(castleEvent);

            // Король должен оказаться на G1
            ChessPiece? king =
                board.GetPiece(
                    Position.Parse("G1"));

            Assert.NotNull(king);
            Assert.IsType<King>(king);
            Assert.Equal(PieceColor.White, king.Color);

            // Ладья должна оказаться на F1
            ChessPiece? rook =
                board.GetPiece(
                    Position.Parse("F1"));

            Assert.NotNull(rook);
            Assert.IsType<Rook>(rook);
            Assert.Equal(PieceColor.White, rook.Color);

            // Старые клетки должны быть пустыми
            Assert.Null(
                board.GetPiece(
                    Position.Parse("E1")));

            Assert.Null(
                board.GetPiece(
                    Position.Parse("H1")));
        }

        [Fact]
        public void Move_Should_Return_Promotion_Event()
        {
            // Arrange
            Board board = new();
            Game game = new(board);

            board.AddPiece(new King(
                PieceColor.White,
                Position.Parse("E1")));

            board.AddPiece(new King(
                PieceColor.Black,
                Position.Parse("E8")));

            board.AddPiece(new Pawn(
                PieceColor.White,
                Position.Parse("B7")));

            // Act
            MoveResult result = game.Move("B7", "B8");

            // Assert
            Assert.True(result.Success);

            Assert.Contains(
                result.Events,
                e => e.Type == GameEventType.Promotion);

            ChessPiece? piece =
                board.GetPiece(Position.Parse("B8"));

            Assert.NotNull(piece);
            Assert.IsType<Queen>(piece);
        }

        [Fact]
        public void Move_Should_Return_Check_Event()
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
                    Position.Parse("E2")));

            board.AddPiece(
                new King(
                    PieceColor.Black,
                    Position.Parse("E8")));

            // Act
            MoveResult result =
                game.Move("E2", "E7");

            // Assert

            // Ход выполнен успешно
            Assert.True(result.Success);

            // Состояние игры должно быть Check
            Assert.Equal(GameState.Check, game.State);

            // Должно быть событие Check
            GameEvent? checkEvent =
                result.Events.FirstOrDefault(
                    e => e.Type == GameEventType.Check);

            Assert.NotNull(checkEvent);

            // Проверяем позицию ладьи
            ChessPiece? rook =
                board.GetPiece(
                    Position.Parse("E7"));

            Assert.NotNull(rook);
            Assert.IsType<Rook>(rook);
            Assert.Equal(PieceColor.White, rook.Color);

            // Исходная клетка должна быть пустой
            Assert.Null(
                board.GetPiece(
                    Position.Parse("E2")));
        }

        [Fact]
        public void Move_Should_Return_Checkmate_Event()
        {
            // Arrange
            Board board = new();
            Game game = new(board);

            board.AddPiece(
                new King(
                    PieceColor.White,
                    Position.Parse("F6")));

            board.AddPiece(
                new Queen(
                    PieceColor.White,
                    Position.Parse("G6")));

            board.AddPiece(
                new King(
                    PieceColor.Black,
                    Position.Parse("H8")));

            // Act
            MoveResult result =
                game.Move("G6", "G7");

            // Assert
            Assert.True(result.Success);

            Assert.True(
                game.IsCheck(PieceColor.Black));

            Assert.Equal(
                GameState.Checkmate,
                game.State);

            GameEvent? checkmateEvent =
                result.Events.FirstOrDefault(
                    e => e.Type == GameEventType.Checkmate);

            Assert.NotNull(checkmateEvent);

            ChessPiece? queen =
                board.GetPiece(
                    Position.Parse("G7"));

            Assert.NotNull(queen);
            Assert.IsType<Queen>(queen);
            Assert.Equal(
                PieceColor.White,
                queen.Color);

            Assert.Null(
                board.GetPiece(
                    Position.Parse("G6")));

            Assert.Equal(
                PieceColor.Black,
                game.CurrentTurn);
        }

        [Fact]
        public void Move_Should_Return_Stalemate_Event()
        {
            // Arrange
            Board board = new();
            Game game = new(board);

            board.AddPiece(
                new King(
                    PieceColor.White,
                    Position.Parse("F6")));

            board.AddPiece(
                new Queen(
                    PieceColor.White,
                    Position.Parse("G5")));

            board.AddPiece(
                new King(
                    PieceColor.Black,
                    Position.Parse("H8")));

            // Act
            MoveResult result =
                game.Move("G5", "G6");

            // Assert
            Assert.True(result.Success);

            // Черный король не должен находиться под шахом
            Assert.False(
                game.IsCheck(PieceColor.Black));

            // Но легальных ходов быть не должно
            Assert.False(
                game.HasLegalMoves(PieceColor.Black));

            // Состояние игры должно быть Stalemate
            Assert.Equal(
                GameState.Stalemate,
                game.State);

            // Должно присутствовать событие Stalemate
            GameEvent? stalemateEvent =
                result.Events.FirstOrDefault(
                    e => e.Type == GameEventType.Stalemate);

            Assert.NotNull(stalemateEvent);

            // Ферзь должен оказаться на G6
            ChessPiece? queen =
                board.GetPiece(
                    Position.Parse("G6"));

            Assert.NotNull(queen);
            Assert.IsType<Queen>(queen);
            Assert.Equal(
                PieceColor.White,
                queen.Color);

            // G5 должна стать пустой
            Assert.Null(
                board.GetPiece(
                    Position.Parse("G5")));

            // Ход должен перейти к черным
            Assert.Equal(
                PieceColor.Black,
                game.CurrentTurn);
        }
    }
}