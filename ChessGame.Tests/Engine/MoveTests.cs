using ChessGame.Core;
using ChessGame.Core.Pieces;
using ChessGame.Core.Model;
using ChessGame.Core.Events;
using ChessGame.Core.Results;

namespace ChessGame.Tests.Engine
{
    public class MoveTests
    {
        // ===== Обычное движение =====
        [Fact]
        public void Pawn_Should_Move_Forward_One_Cell()
        {
            // Arrange
            Board board = new();
            Game game = new(board);

            Pawn pawn = new(PieceColor.White, Position.Parse("E2"));

            board.AddPiece(pawn);
            board.AddPiece(new King(PieceColor.White, Position.Parse("E1")));
            board.AddPiece(new King(PieceColor.Black, Position.Parse("E8")));

            // Act
            game.Move("E2", "E3");

            // Assert
            Assert.Null(board.GetPiece(Position.Parse("E2")));

            ChessPiece? piece = board.GetPiece(Position.Parse("E3"));

            Assert.NotNull(piece);
            Assert.IsType<Pawn>(piece);

            Assert.Equal(PieceColor.White, piece.Color);
            Assert.Equal(1, piece.MoveCount);
        }

        [Fact]
        public void Pawn_Should_Move_Two_Cells_From_Start()
        {
            // Arrange
            Board board = new();
            Game game = new(board);

            Pawn pawn = new(PieceColor.White, Position.Parse("E2"));

            board.AddPiece(pawn);
            board.AddPiece(new King(PieceColor.White, Position.Parse("E1")));
            board.AddPiece(new King(PieceColor.Black, Position.Parse("E8")));

            // Act
            game.Move("E2", "E4");

            // Assert
            Assert.Null(board.GetPiece(Position.Parse("E2")));

            ChessPiece? piece = board.GetPiece(Position.Parse("E4"));

            Assert.NotNull(piece);
            Assert.IsType<Pawn>(piece);

            Assert.Equal(PieceColor.White, piece.Color);
            Assert.Equal(1, piece.MoveCount);
        }

        [Fact]
        public void Pawn_Should_Not_Move_Backward()
        {
            // Arrange
            Board board = new();
            Game game = new(board);

            board.AddPiece(new King(PieceColor.White, Position.Parse("E1")));
            board.AddPiece(new King(PieceColor.Black, Position.Parse("E8")));
            board.AddPiece(new Pawn(PieceColor.White, Position.Parse("E2")));

            // Act + Assert
            Assert.Throws<InvalidOperationException>(
                () => game.Move("E2", "E1"));
        }

        [Fact]
        public void Pawn_Should_Not_Move_Sideways()
        {
            // Arrange
            Board board = new();
            Game game = new(board);

            board.AddPiece(new King(PieceColor.White, Position.Parse("E1")));
            board.AddPiece(new King(PieceColor.Black, Position.Parse("E8")));
            board.AddPiece(new Pawn(PieceColor.White, Position.Parse("E2")));

            // Act + Assert
            Assert.Throws<InvalidOperationException>(
                () => game.Move("E2", "D2"));
        }

        // ===== Препятствия =====

        [Fact]
        public void Pawn_Should_Not_Move_Forward_When_Blocked()
        {
            // Arrange
            Board board = new();
            Game game = new(board);

            board.AddPiece(new King(PieceColor.White, Position.Parse("E1")));
            board.AddPiece(new King(PieceColor.Black, Position.Parse("E8")));
            board.AddPiece(new Pawn(PieceColor.White, Position.Parse("E2")));
            board.AddPiece(new Pawn(PieceColor.Black, Position.Parse("E5")));

            // Act
            game.Move("E2", "E3");
            game.Move("E5", "E4");

            // Act + Assert
            Assert.Throws<InvalidOperationException>(
                () => game.Move("E3", "E4"));
        }

        [Fact]
        public void Pawn_Should_Not_Move_Two_Cells_When_First_Cell_Is_Blocked()
        {
            // Arrange
            Board board = new();
            Game game = new(board);

            board.AddPiece(new King(PieceColor.White, Position.Parse("E1")));
            board.AddPiece(new King(PieceColor.Black, Position.Parse("E8")));
            board.AddPiece(new Pawn(PieceColor.White, Position.Parse("E2")));
            board.AddPiece(new Knight(PieceColor.White, Position.Parse("E3")));

            // Act + Assert
            Assert.Throws<InvalidOperationException>(
                () => game.Move("E2", "E4"));
        }

        [Fact]
        public void Pawn_Should_Not_Move_Two_Cells_When_Second_Cell_Is_Blocked()
        {
            // Arrange
            Board board = new();
            Game game = new(board);

            board.AddPiece(new King(PieceColor.White, Position.Parse("E1")));
            board.AddPiece(new King(PieceColor.Black, Position.Parse("E8")));
            board.AddPiece(new Pawn(PieceColor.White, Position.Parse("E2")));
            board.AddPiece(new Rook(PieceColor.White, Position.Parse("E5")));

            // Act + Assert
            Assert.Throws<InvalidOperationException>(
                () => game.Move("E2", "E5"));
        }

        // ===== Взятие =====

        [Fact]
        public void Pawn_Should_Capture_Diagonally()
        {
            // Arrange
            Board board = new();
            Game game = new(board);

            Pawn pawn_1 = new(PieceColor.White, Position.Parse("E2"));
            Pawn pawn_2 = new(PieceColor.Black, Position.Parse("D3"));
            board.AddPiece(pawn_1);
            board.AddPiece(pawn_2);
            board.AddPiece(new King(PieceColor.White, Position.Parse("E1")));
            board.AddPiece(new King(PieceColor.Black, Position.Parse("E8")));

            // Act
            game.Move("E2", "D3");

            // Assert
            Assert.Null(board.GetPiece(Position.Parse("E2")));

            ChessPiece? piece = board.GetPiece(Position.Parse("D3"));

            Assert.NotNull(piece);
            Assert.IsType<Pawn>(piece);

            Assert.Equal(PieceColor.White, piece.Color);
            Assert.Equal(1, piece.MoveCount);
        }

        [Fact]
        public void Pawn_Should_Not_Capture_Forward()
        {
            // Arrange
            Board board = new();
            Game game = new(board);

            board.AddPiece(new King(PieceColor.White, Position.Parse("E1")));
            board.AddPiece(new King(PieceColor.Black, Position.Parse("E8")));
            board.AddPiece(new Pawn(PieceColor.White, Position.Parse("E2")));
            board.AddPiece(new Pawn(PieceColor.Black, Position.Parse("E3")));

            // Act + Assert
            Assert.Throws<InvalidOperationException>(
                () => game.Move("E2", "E3"));
        }

        [Fact]
        public void Pawn_Should_Not_Capture_Own_Piece()
        {
            // Arrange
            Board board = new();
            Game game = new(board);

            board.AddPiece(new King(PieceColor.White, Position.Parse("E1")));
            board.AddPiece(new King(PieceColor.Black, Position.Parse("E8")));
            board.AddPiece(new Pawn(PieceColor.White, Position.Parse("E2")));
            board.AddPiece(new Pawn(PieceColor.White, Position.Parse("D3")));

            // Act + Assert
            Assert.Throws<InvalidOperationException>(
                () => game.Move("E2", "D3"));
        }

        // ===== Первый ход =====

        [Fact]
        public void Pawn_Should_Not_Move_Two_Cells_After_First_Move()
        {
            // Arrange
            Board board = new();
            Game game = new(board);

            board.AddPiece(new King(PieceColor.White, Position.Parse("E1")));
            board.AddPiece(new King(PieceColor.Black, Position.Parse("E8")));
            board.AddPiece(new Pawn(PieceColor.White, Position.Parse("E2")));
            board.AddPiece(new Pawn(PieceColor.Black, Position.Parse("E7")));

            // Act
            game.Move("E2", "E3");
            game.Move("E7", "E6");

            // Act + Assert
            Assert.Throws<InvalidOperationException>(
                () => game.Move("E3", "E5"));
        }

        [Theory]
        [InlineData(PromotionPiece.Queen)]
        [InlineData(PromotionPiece.Rook)]
        [InlineData(PromotionPiece.Bishop)]
        [InlineData(PromotionPiece.Knight)]
        public void Black_Promotion_Should_Create_Selected_Piece(
            PromotionPiece promotionPiece)
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

            board.AddPiece(
                new Pawn(
                    PieceColor.Black,
                    Position.Parse("B2")));

            // Передаём ход белым,
            // чтобы теперь ходил Black.
            MoveResult whiteMove =
                game.Move(
                    new Move(
                        Position.Parse("E1"),
                        Position.Parse("E2")));

            Assert.True(whiteMove.Success);

            Assert.Equal(
                PieceColor.Black,
                game.CurrentTurn);

            // Act
            MoveResult result =
                game.Move(
                    new Move(
                        Position.Parse("B2"),
                        Position.Parse("B1"),
                        promotionPiece));

            // Assert
            Assert.True(result.Success);

            ChessPiece? promotedPiece =
                board.GetPiece(
                    Position.Parse("B1"));

            Assert.NotNull(promotedPiece);

            Assert.Equal(
                PieceColor.Black,
                promotedPiece.Color);

            switch (promotionPiece)
            {
                case PromotionPiece.Queen:
                    Assert.IsType<Queen>(promotedPiece);
                    break;

                case PromotionPiece.Rook:
                    Assert.IsType<Rook>(promotedPiece);
                    break;

                case PromotionPiece.Bishop:
                    Assert.IsType<Bishop>(promotedPiece);
                    break;

                case PromotionPiece.Knight:
                    Assert.IsType<Knight>(promotedPiece);
                    break;
            }

            Assert.Null(
                board.GetPiece(
                    Position.Parse("B2")));

            Assert.Contains(
                result.Events,
                e => e.Type == GameEventType.Promotion);
        }

        [Theory]
        [InlineData(PromotionPiece.Queen)]
        [InlineData(PromotionPiece.Rook)]
        [InlineData(PromotionPiece.Bishop)]
        [InlineData(PromotionPiece.Knight)]
        public void Undo_Should_Restore_Pawn_After_Promotion(
    PromotionPiece promotionPiece)
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
                    Position.Parse("B7"));

            board.AddPiece(whitePawn);

            // Act — promotion
            MoveResult result =
                game.Move(
                    new Move(
                        Position.Parse("B7"),
                        Position.Parse("B8"),
                        promotionPiece));

            // Assert — promotion выполнен
            Assert.True(result.Success);

            ChessPiece? promotedPiece =
                board.GetPiece(
                    Position.Parse("B8"));

            Assert.NotNull(promotedPiece);

            // На B8 должна находиться выбранная фигура
            switch (promotionPiece)
            {
                case PromotionPiece.Queen:
                    Assert.IsType<Queen>(promotedPiece);
                    break;

                case PromotionPiece.Rook:
                    Assert.IsType<Rook>(promotedPiece);
                    break;

                case PromotionPiece.Bishop:
                    Assert.IsType<Bishop>(promotedPiece);
                    break;

                case PromotionPiece.Knight:
                    Assert.IsType<Knight>(promotedPiece);
                    break;
            }

            Assert.Null(
                board.GetPiece(
                    Position.Parse("B7")));

            // Act — Undo
            game.Undo();

            // Assert — пешка восстановлена
            ChessPiece? restoredPiece =
                board.GetPiece(
                    Position.Parse("B7"));

            Assert.NotNull(restoredPiece);

            Assert.IsType<Pawn>(
                restoredPiece);

            Assert.Equal(
                PieceColor.White,
                restoredPiece.Color);

            // Очень важная проверка:
            // восстановлен именно исходный объект Pawn
            Assert.Same(
                whitePawn,
                restoredPiece);

            // B8 снова свободна
            Assert.Null(
                board.GetPiece(
                    Position.Parse("B8")));

            // Пешка вернулась в исходное состояние
            Assert.Equal(
                0,
                restoredPiece.MoveCount);

            Assert.True(
                restoredPiece.IsAlive);

            // После Undo снова ход White
            Assert.Equal(
                PieceColor.White,
                game.CurrentTurn);
        }

        [Theory]
        [InlineData(PromotionPiece.Queen, "B7 → B8=Queen")]
        [InlineData(PromotionPiece.Rook, "B7 → B8=Rook")]
        [InlineData(PromotionPiece.Bishop, "B7 → B8=Bishop")]
        [InlineData(PromotionPiece.Knight, "B7 → B8=Knight")]
        public void Move_ToString_Should_Include_Promotion(
    PromotionPiece promotionPiece,
    string expected)
        {
            // Arrange
            Move move =
                new(
                    Position.Parse("B7"),
                    Position.Parse("B8"),
                    promotionPiece);

            // Act
            string actual = move.ToString();

            // Assert
            Assert.Equal(
                expected,
                actual);
        }

        [Fact]
        public void Move_ToString_Without_Promotion_Should_Return_Regular_Notation()
        {
            // Arrange
            Move move =
                new(
                    Position.Parse("E2"),
                    Position.Parse("E4"));

            // Act
            string actual = move.ToString();

            // Assert
            Assert.Equal(
                "E2 → E4",
                actual);
        }

        [Fact]
        public void Move_Should_Return_Draw_When_Only_Kings_Remain()
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

            Assert.Contains(
                result.Events,
                e => e.Type == GameEventType.Draw);
        }

        [Fact]
        public void HalfMoveClock_Should_Increase_After_Normal_Move()
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

            Assert.Equal(0, game.HalfMoveClock);

            // Act
            MoveResult result =
                game.Move(
                    new Move(
                        Position.Parse("A1"),
                        Position.Parse("A2")));

            // Assert
            Assert.True(result.Success);

            Assert.Equal(
                1,
                game.HalfMoveClock);
        }

        [Fact]
        public void HalfMoveClock_Should_Reset_After_Pawn_Move()
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
                new Pawn(
                    PieceColor.White,
                    Position.Parse("B2")));

            board.AddPiece(
                new King(
                    PieceColor.Black,
                    Position.Parse("E8")));

            // Белые: обычный ход
            game.Move(
                new Move(
                    Position.Parse("A1"),
                    Position.Parse("A2")));

            // Чёрные: ход королём
            game.Move(
                new Move(
                    Position.Parse("E8"),
                    Position.Parse("E7")));

            Assert.Equal(
                2,
                game.HalfMoveClock);

            // Act — ход пешкой
            game.Move(
                new Move(
                    Position.Parse("B2"),
                    Position.Parse("B3")));

            // Assert
            Assert.Equal(
                0,
                game.HalfMoveClock);
        }

        [Fact]
        public void HalfMoveClock_Should_Reset_After_Capture()
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

            board.AddPiece(
                new Rook(
                    PieceColor.Black,
                    Position.Parse("A8")));

            // Белые: обычный ход
            game.Move(
                new Move(
                    Position.Parse("A1"),
                    Position.Parse("A2")));

            // Чёрные: обычный ход
            game.Move(
                new Move(
                    Position.Parse("A8"),
                    Position.Parse("A7")));

            Assert.Equal(
                2,
                game.HalfMoveClock);

            // Белые берут чёрную ладью
            MoveResult result =
                game.Move(
                    new Move(
                        Position.Parse("A2"),
                        Position.Parse("A7")));

            // Assert
            Assert.True(result.Success);

            Assert.Equal(
                0,
                game.HalfMoveClock);

            Assert.Contains(
                result.Events,
                e => e.Type == GameEventType.Capture);
        }

        [Fact]
        public void Undo_Should_Restore_Previous_HalfMoveClock()
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
                new Pawn(
                    PieceColor.White,
                    Position.Parse("B2")));

            board.AddPiece(
                new King(
                    PieceColor.Black,
                    Position.Parse("E8")));

            // Белые: обычный ход
            game.Move(
                new Move(
                    Position.Parse("A1"),
                    Position.Parse("A2")));

            // Чёрные: обычный ход
            game.Move(
                new Move(
                    Position.Parse("E8"),
                    Position.Parse("E7")));

            Assert.Equal(
                2,
                game.HalfMoveClock);

            // Белые: ход пешкой — сбрасывает счётчик
            game.Move(
                new Move(
                    Position.Parse("B2"),
                    Position.Parse("B3")));

            Assert.Equal(
                0,
                game.HalfMoveClock);

            // Act
            game.Undo();

            // Assert
            Assert.Equal(
                2,
                game.HalfMoveClock);

            Assert.NotNull(
                board.GetPiece(
                    Position.Parse("B2")));

            Assert.Null(
                board.GetPiece(
                    Position.Parse("B3")));

            Assert.Equal(
                PieceColor.White,
                game.CurrentTurn);
        }

        [Fact]
        public void Move_When_HalfMoveClock_Reaches_100_Should_End_Game_In_Draw()
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

            typeof(Game)
                .GetProperty(nameof(Game.HalfMoveClock))!
                .SetValue(game, 99);

            // Act
            MoveResult result =
                game.Move(
                    new Move(
                        Position.Parse("A1"),
                        Position.Parse("A2")));

            // Assert
            Assert.True(result.Success);

            Assert.Equal(
                100,
                game.HalfMoveClock);

            Assert.Equal(
                GameState.Draw,
                game.State);

            Assert.True(
                game.IsFinished);

            Assert.Contains(
                result.Events,
                e => e.Type == GameEventType.Draw);
        }

        [Fact]
        public void Undo_After_FiftyMoveRule_Draw_Should_Restore_Previous_State()
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

            typeof(Game)
                .GetProperty(nameof(Game.HalfMoveClock))!
                .SetValue(game, 99);

            game.Move(
                new Move(
                    Position.Parse("A1"),
                    Position.Parse("A2")));

            Assert.Equal(
                100,
                game.HalfMoveClock);

            Assert.Equal(
                GameState.Draw,
                game.State);

            Assert.True(game.IsFinished);

            // Act
            game.Undo();

            // Assert
            Assert.Equal(
                99,
                game.HalfMoveClock);

            Assert.Equal(
                GameState.Normal,
                game.State);

            Assert.False(game.IsFinished);

            Assert.Equal(
                PieceColor.White,
                game.CurrentTurn);

            Assert.NotNull(
                board.GetPiece(
                    Position.Parse("A1")));

            Assert.Null(
                board.GetPiece(
                    Position.Parse("A2")));
        }

        [Fact]
        public void PositionHistory_Should_Count_Repeated_Position()
        {
            // Arrange
            Board board = new();
            

            board.AddPiece(
                new King(
                    PieceColor.White,
                    Position.Parse("E1")));

            board.AddPiece(
                new Knight(
                    PieceColor.White,
                    Position.Parse("G1")));

            board.AddPiece(
                new King(
                    PieceColor.Black,
                    Position.Parse("E8")));

            board.AddPiece(
                new Knight(
                    PieceColor.Black,
                    Position.Parse("G8")));

            Game game = new(board);

            Assert.Equal(1, game.GetCurrentPositionRepetitionCount());

            // Act
            game.Move(
                new Move(
                    Position.Parse("G1"),
                    Position.Parse("F3")));

            game.Move(
                new Move(
                    Position.Parse("G8"),
                    Position.Parse("F6")));

            game.Move(
                new Move(
                    Position.Parse("F3"),
                    Position.Parse("G1")));

            game.Move(
                new Move(
                    Position.Parse("F6"),
                    Position.Parse("G8")));

            Assert.Equal(2, game.GetCurrentPositionRepetitionCount());

            // Assert
            Assert.Equal(
                GameState.Normal,
                game.State);
        }

        [Fact]
        public void Game_Should_End_In_Draw_After_Threefold_Repetition()
        {
            // Arrange
            Board board = new();

            board.AddPiece(
                new King(
                    PieceColor.White,
                    Position.Parse("E1")));

            board.AddPiece(
                new Knight(
                    PieceColor.White,
                    Position.Parse("G1")));

            board.AddPiece(
                new King(
                    PieceColor.Black,
                    Position.Parse("E8")));

            board.AddPiece(
                new Knight(
                    PieceColor.Black,
                    Position.Parse("G8")));

            Game game = new(board);

            // Первый цикл — позиция повторяется второй раз
            game.Move(
                new Move(
                    Position.Parse("G1"),
                    Position.Parse("F3")));

            game.Move(
                new Move(
                    Position.Parse("G8"),
                    Position.Parse("F6")));

            game.Move(
                new Move(
                    Position.Parse("F3"),
                    Position.Parse("G1")));

            game.Move(
                new Move(
                    Position.Parse("F6"),
                    Position.Parse("G8")));

            Assert.Equal(
                2,
                game.GetCurrentPositionRepetitionCount());

            Assert.Equal(
                GameState.Normal,
                game.State);

            // Второй цикл — позиция повторяется третий раз
            game.Move(
                new Move(
                    Position.Parse("G1"),
                    Position.Parse("F3")));

            game.Move(
                new Move(
                    Position.Parse("G8"),
                    Position.Parse("F6")));

            game.Move(
                new Move(
                    Position.Parse("F3"),
                    Position.Parse("G1")));

            MoveResult result =
                game.Move(
                    new Move(
                        Position.Parse("F6"),
                        Position.Parse("G8")));

            // Assert
            Assert.Equal(
                3,
                game.GetCurrentPositionRepetitionCount());

            Assert.Equal(
                GameState.Draw,
                game.State);

            Assert.True(game.IsFinished);

            Assert.Contains(
                result.Events,
                e => e.Type == GameEventType.Draw);
        }

        [Fact]
        public void Undo_After_Threefold_Repetition_Draw_Should_Restore_Game()
        {
            // Arrange
            Board board = new();

            board.AddPiece(
                new King(
                    PieceColor.White,
                    Position.Parse("E1")));

            board.AddPiece(
                new Knight(
                    PieceColor.White,
                    Position.Parse("G1")));

            board.AddPiece(
                new King(
                    PieceColor.Black,
                    Position.Parse("E8")));

            board.AddPiece(
                new Knight(
                    PieceColor.Black,
                    Position.Parse("G8")));

            Game game = new(board);

            // Первый цикл
            game.Move(new Move(
                Position.Parse("G1"),
                Position.Parse("F3")));

            game.Move(new Move(
                Position.Parse("G8"),
                Position.Parse("F6")));

            game.Move(new Move(
                Position.Parse("F3"),
                Position.Parse("G1")));

            game.Move(new Move(
                Position.Parse("F6"),
                Position.Parse("G8")));

            // Второй цикл
            game.Move(new Move(
                Position.Parse("G1"),
                Position.Parse("F3")));

            game.Move(new Move(
                Position.Parse("G8"),
                Position.Parse("F6")));

            game.Move(new Move(
                Position.Parse("F3"),
                Position.Parse("G1")));

            game.Move(new Move(
                Position.Parse("F6"),
                Position.Parse("G8")));

            Assert.Equal(
                GameState.Draw,
                game.State);

            Assert.Equal(
                3,
                game.GetCurrentPositionRepetitionCount());

            // Act
            game.Undo();

            // Assert
            Assert.Equal(
                GameState.Normal,
                game.State);

            Assert.False(game.IsFinished);

            Assert.Equal(
                PieceColor.Black,
                game.CurrentTurn);

            Assert.NotNull(
                board.GetPiece(
                    Position.Parse("F6")));

            Assert.Null(
                board.GetPiece(
                    Position.Parse("G8")));
        }

        [Fact]
        public void Kings_And_Bishops_On_Same_Color_Squares_Should_Be_Draw()
        {
            // Arrange
            Board board = new();

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

            board.AddPiece(
                new Bishop(
                    PieceColor.Black,
                    Position.Parse("F8")));

            Game game = new(board);

            // Act
            bool result = game.IsDrawByInsufficientMaterial();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Kings_And_Bishops_On_Different_Color_Squares_Should_Not_Be_Draw()
        {
            // Arrange
            Board board = new();

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

            board.AddPiece(
                new Bishop(
                    PieceColor.Black,
                    Position.Parse("C8")));

            Game game = new(board);

            // Act
            bool result =
                game.IsDrawByInsufficientMaterial();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void Move_When_Only_King_And_Bishop_Against_King_Should_End_In_Draw()
        {
            // Arrange
            Board board = new();

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

            Game game = new(board);

            // Act
            MoveResult result =
                game.Move(
                    new Move(
                        Position.Parse("C1"),
                        Position.Parse("D2")));

            // Assert
            Assert.True(result.Success);

            Assert.Equal(
                GameState.Draw,
                game.State);

            Assert.True(game.IsFinished);

            Assert.Contains(
                result.Events,
                e => e.Type == GameEventType.Draw);
        }

        [Fact]
        public void Undo_After_InsufficientMaterial_Draw_Should_Restore_Game()
        {
            // Arrange
            Board board = new();

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

            Game game = new(board);

            game.Move(
                new Move(
                    Position.Parse("C1"),
                    Position.Parse("D2")));

            Assert.Equal(
                GameState.Draw,
                game.State);

            Assert.True(
                game.IsFinished);

            // Act
            game.Undo();

            // Assert
            Assert.Equal(
                GameState.Normal,
                game.State);

            Assert.False(
                game.IsFinished);

            Assert.Equal(
                PieceColor.White,
                game.CurrentTurn);

            ChessPiece? bishop =
                board.GetPiece(
                    Position.Parse("C1"));

            Assert.NotNull(bishop);

            Assert.IsType<Bishop>(bishop);

            Assert.Null(
                board.GetPiece(
                    Position.Parse("D2")));
        }

        [Fact]
        public void Move_After_Undo_ThreefoldRepetition_Should_End_In_Draw_Again()
        {
            // Arrange
            Board board = new();

            board.AddPiece(
                new King(
                    PieceColor.White,
                    Position.Parse("E1")));

            board.AddPiece(
                new Knight(
                    PieceColor.White,
                    Position.Parse("G1")));

            board.AddPiece(
                new King(
                    PieceColor.Black,
                    Position.Parse("E8")));

            board.AddPiece(
                new Knight(
                    PieceColor.Black,
                    Position.Parse("G8")));

            Game game = new(board);

            // Первый цикл — позиция повторяется второй раз
            game.Move(new Move(
                Position.Parse("G1"),
                Position.Parse("F3")));

            game.Move(new Move(
                Position.Parse("G8"),
                Position.Parse("F6")));

            game.Move(new Move(
                Position.Parse("F3"),
                Position.Parse("G1")));

            game.Move(new Move(
                Position.Parse("F6"),
                Position.Parse("G8")));

            Assert.Equal(
                2,
                game.GetCurrentPositionRepetitionCount());

            // Второй цикл — третье повторение
            game.Move(new Move(
                Position.Parse("G1"),
                Position.Parse("F3")));

            game.Move(new Move(
                Position.Parse("G8"),
                Position.Parse("F6")));

            game.Move(new Move(
                Position.Parse("F3"),
                Position.Parse("G1")));

            game.Move(new Move(
                Position.Parse("F6"),
                Position.Parse("G8")));

            Assert.Equal(
                GameState.Draw,
                game.State);

            Assert.Equal(
                3,
                game.GetCurrentPositionRepetitionCount());

            // Act — отменяем последний ход
            game.Undo();

            // Проверяем, что игра снова продолжается
            Assert.Equal(
                GameState.Normal,
                game.State);

            Assert.False(
                game.IsFinished);

            Assert.Equal(
                PieceColor.Black,
                game.CurrentTurn);

            Assert.NotNull(
                board.GetPiece(
                    Position.Parse("F6")));

            Assert.Null(
                board.GetPiece(
                    Position.Parse("G8")));

            // Повторяем отменённый ход
            MoveResult result = game.Move(
                new Move(
                    Position.Parse("F6"),
                    Position.Parse("G8")));

            // Assert
            Assert.True(result.Success);

            Assert.Equal(
                GameState.Draw,
                game.State);

            Assert.True(
                game.IsFinished);

            Assert.Equal(
                3,
                game.GetCurrentPositionRepetitionCount());

            Assert.Contains(
                result.Events,
                e => e.Type == GameEventType.Draw);
        }

        [Fact]
        public void Undo_Should_Restore_LastMove_For_EnPassant()
        {
            // Arrange
            Board board = new();

            board.AddPiece(
                new King(
                    PieceColor.White,
                    Position.Parse("E1")));

            board.AddPiece(
                new Pawn(
                    PieceColor.White,
                    Position.Parse("E2")));

            board.AddPiece(
                new King(
                    PieceColor.Black,
                    Position.Parse("G8")));

            board.AddPiece(
                new Pawn(
                    PieceColor.Black,
                    Position.Parse("D4")));

            Game game = new(board);

            // 1. Белая пешка делает двойной ход
            Move firstMove = new(
                Position.Parse("E2"),
                Position.Parse("E4"));

            game.Move(firstMove);

            Assert.Equal(
                firstMove,
                game.LastMove);

            // 2. Чёрные делают обычный ход
            Move blackMove = new(
                Position.Parse("G8"),
                Position.Parse("F8"));

            game.Move(blackMove);

            Assert.Equal(
                blackMove,
                game.LastMove);

            // Act — отменяем последний ход чёрных
            game.Undo();

            // Assert — LastMove должен снова указывать
            // на двойной ход белой пешки
            Assert.Equal(
                firstMove,
                game.LastMove);

            Assert.Equal(
                PieceColor.Black,
                game.CurrentTurn);

            // Проверяем, что En Passant снова возможен
            Move enPassantMove = new(
                Position.Parse("D4"),
                Position.Parse("E3"));

            MoveResult result =
                game.Move(enPassantMove);

            Assert.True(result.Success);

            Assert.Contains(
                result.Events,
                e => e.Type == GameEventType.EnPassant);

            // Белая пешка должна быть взята
            Assert.Null(
                board.GetPiece(
                    Position.Parse("E4")));

            // Чёрная пешка должна оказаться на E3
            ChessPiece? blackPawn =
                board.GetPiece(
                    Position.Parse("E3"));

            Assert.NotNull(blackPawn);

            Assert.IsType<Pawn>(blackPawn);

            Assert.Equal(
                PieceColor.Black,
                blackPawn.Color);
        }

        [Fact]
        public void Undo_EnPassant_Should_Allow_EnPassant_Again()
        {
            // Arrange
            Board board = new();

            board.AddPiece(
                new King(
                    PieceColor.White,
                    Position.Parse("E1")));

            board.AddPiece(
                new Pawn(
                    PieceColor.White,
                    Position.Parse("E2")));

            board.AddPiece(
                new King(
                    PieceColor.Black,
                    Position.Parse("E8")));

            board.AddPiece(
                new Pawn(
                    PieceColor.Black,
                    Position.Parse("D4")));

            Game game = new(board);

            // Белая пешка делает двойной ход
            Move doublePawnMove = new(
                Position.Parse("E2"),
                Position.Parse("E4"));

            game.Move(doublePawnMove);

            Assert.Equal(
                doublePawnMove,
                game.LastMove);

            // Чёрные берут на проходе
            Move enPassantMove = new(
                Position.Parse("D4"),
                Position.Parse("E3"));

            MoveResult firstResult =
                game.Move(enPassantMove);

            Assert.True(firstResult.Success);

            Assert.Contains(
                firstResult.Events,
                e => e.Type == GameEventType.EnPassant);

            Assert.NotNull(
                board.GetPiece(
                    Position.Parse("E3")));

            Assert.Null(
                board.GetPiece(
                    Position.Parse("E4")));

            Assert.Null(
                board.GetPiece(
                    Position.Parse("D4")));

            // Act — отменяем En Passant
            game.Undo();

            // Assert — доска должна полностью восстановиться

            ChessPiece? whitePawn =
                board.GetPiece(
                    Position.Parse("E4"));

            Assert.NotNull(whitePawn);

            Assert.IsType<Pawn>(whitePawn);

            Assert.Equal(
                PieceColor.White,
                whitePawn.Color);

            ChessPiece? blackPawn =
                board.GetPiece(
                    Position.Parse("D4"));

            Assert.NotNull(blackPawn);

            Assert.IsType<Pawn>(blackPawn);

            Assert.Equal(
                PieceColor.Black,
                blackPawn.Color);

            Assert.Null(
                board.GetPiece(
                    Position.Parse("E3")));

            // LastMove снова должен указывать
            // на двойной ход белой пешки
            Assert.Equal(
                doublePawnMove,
                game.LastMove);

            Assert.Equal(
                PieceColor.Black,
                game.CurrentTurn);

            // Повторяем тот же En Passant
            MoveResult secondResult =
                game.Move(enPassantMove);

            // Assert
            Assert.True(secondResult.Success);

            Assert.Contains(
                secondResult.Events,
                e => e.Type == GameEventType.EnPassant);

            Assert.Null(
                board.GetPiece(
                    Position.Parse("E4")));

            Assert.Null(
                board.GetPiece(
                    Position.Parse("D4")));

            ChessPiece? finalBlackPawn =
                board.GetPiece(
                    Position.Parse("E3"));

            Assert.NotNull(finalBlackPawn);

            Assert.IsType<Pawn>(finalBlackPawn);

            Assert.Equal(
                PieceColor.Black,
                finalBlackPawn.Color);
        }

        [Fact]
        public void Undo_Promotion_Should_Allow_Promotion_Again()
        {
            // Arrange
            Board board = new();

            board.AddPiece(
                new King(
                    PieceColor.White,
                    Position.Parse("E1")));

            board.AddPiece(
                new Pawn(
                    PieceColor.White,
                    Position.Parse("A7")));

            board.AddPiece(
                new King(
                    PieceColor.Black,
                    Position.Parse("E8")));

            Game game = new(board);

            Move promotionMove = new(
                Position.Parse("A7"),
                Position.Parse("A8"),
                PromotionPiece.Queen);

            // Act — первое превращение
            MoveResult firstResult =
                game.Move(promotionMove);

            // Assert
            Assert.True(firstResult.Success);

            Assert.Contains(
                firstResult.Events,
                e => e.Type == GameEventType.Promotion);

            ChessPiece? promotedPiece =
                board.GetPiece(
                    Position.Parse("A8"));

            Assert.NotNull(promotedPiece);

            Assert.IsType<Queen>(promotedPiece);

            Assert.Equal(
                PieceColor.White,
                promotedPiece.Color);

            Assert.Null(
                board.GetPiece(
                    Position.Parse("A7")));

            // Act — отменяем превращение
            game.Undo();

            // Assert — должна вернуться исходная пешка
            ChessPiece? restoredPawn =
                board.GetPiece(
                    Position.Parse("A7"));

            Assert.NotNull(restoredPawn);

            Assert.IsType<Pawn>(restoredPawn);

            Assert.Equal(
                PieceColor.White,
                restoredPawn.Color);

            Assert.Null(
                board.GetPiece(
                    Position.Parse("A8")));

            Assert.Equal(
                PieceColor.White,
                game.CurrentTurn);

            // Act — повторяем превращение
            MoveResult secondResult =
                game.Move(promotionMove);

            // Assert
            Assert.True(secondResult.Success);

            Assert.Contains(
                secondResult.Events,
                e => e.Type == GameEventType.Promotion);

            ChessPiece? finalPromotedPiece =
                board.GetPiece(
                    Position.Parse("A8"));

            Assert.NotNull(finalPromotedPiece);

            Assert.IsType<Queen>(finalPromotedPiece);

            Assert.Equal(
                PieceColor.White,
                finalPromotedPiece.Color);

            Assert.Null(
                board.GetPiece(
                    Position.Parse("A7")));
        }

        [Fact]
        public void Undo_Castle_Should_Allow_Castling_Again()
        {
            // Arrange
            Board board = new();

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

            Game game = new(board);

            Move castleMove = new(
                Position.Parse("E1"),
                Position.Parse("G1"));

            // Act — первая рокировка
            MoveResult firstResult =
                game.Move(castleMove);

            // Assert
            Assert.True(firstResult.Success);

            Assert.Contains(
                firstResult.Events,
                e => e.Type == GameEventType.Castle);

            ChessPiece? kingAfterCastle =
                board.GetPiece(
                    Position.Parse("G1"));

            Assert.NotNull(kingAfterCastle);

            Assert.IsType<King>(kingAfterCastle);

            ChessPiece? rookAfterCastle =
                board.GetPiece(
                    Position.Parse("F1"));

            Assert.NotNull(rookAfterCastle);

            Assert.IsType<Rook>(rookAfterCastle);

            Assert.Null(
                board.GetPiece(
                    Position.Parse("E1")));

            Assert.Null(
                board.GetPiece(
                    Position.Parse("H1")));

            // Act — отменяем рокировку
            game.Undo();

            // Assert — король должен вернуться
            ChessPiece? restoredKing =
                board.GetPiece(
                    Position.Parse("E1"));

            Assert.NotNull(restoredKing);

            Assert.IsType<King>(restoredKing);

            Assert.Equal(
                PieceColor.White,
                restoredKing.Color);

            // Ладья должна вернуться
            ChessPiece? restoredRook =
                board.GetPiece(
                    Position.Parse("H1"));

            Assert.NotNull(restoredRook);

            Assert.IsType<Rook>(restoredRook);

            Assert.Equal(
                PieceColor.White,
                restoredRook.Color);

            Assert.Null(
                board.GetPiece(
                    Position.Parse("G1")));

            Assert.Null(
                board.GetPiece(
                    Position.Parse("F1")));

            // Важно: после Undo снова ход белых
            Assert.Equal(
                PieceColor.White,
                game.CurrentTurn);

            // Дополнительная проверка:
            // MoveCount должен быть восстановлен,
            // иначе повторная рокировка не сработает
            Assert.Equal(
                0,
                restoredKing.MoveCount);

            Assert.Equal(
                0,
                restoredRook.MoveCount);

            // Act — рокируемся повторно
            MoveResult secondResult =
                game.Move(castleMove);

            // Assert
            Assert.True(secondResult.Success);

            Assert.Contains(
                secondResult.Events,
                e => e.Type == GameEventType.Castle);

            ChessPiece? finalKing =
                board.GetPiece(
                    Position.Parse("G1"));

            Assert.NotNull(finalKing);

            Assert.IsType<King>(finalKing);

            ChessPiece? finalRook =
                board.GetPiece(
                    Position.Parse("F1"));

            Assert.NotNull(finalRook);

            Assert.IsType<Rook>(finalRook);

            Assert.Null(
                board.GetPiece(
                    Position.Parse("E1")));

            Assert.Null(
                board.GetPiece(
                    Position.Parse("H1")));
        }

        [Fact]
        public void Undo_Capture_Should_Allow_Capture_Again()
        {
            // Arrange
            Board board = new();

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

            board.AddPiece(
                new Knight(
                    PieceColor.Black,
                    Position.Parse("A8")));

            Game game = new(board);

            Move captureMove = new(
                Position.Parse("A1"),
                Position.Parse("A8"));

            // Первый захват
            MoveResult firstResult =
                game.Move(captureMove);

            Assert.True(firstResult.Success);

            Assert.Contains(
                firstResult.Events,
                e => e.Type == GameEventType.Capture);

            ChessPiece? rookAfterCapture =
                board.GetPiece(
                    Position.Parse("A8"));

            Assert.NotNull(rookAfterCapture);

            Assert.IsType<Rook>(rookAfterCapture);

            Assert.Equal(
                PieceColor.White,
                rookAfterCapture.Color);

            Assert.Null(
                board.GetPiece(
                    Position.Parse("A1")));

            // Act — отменяем взятие
            game.Undo();

            // Ладья должна вернуться
            ChessPiece? restoredRook =
                board.GetPiece(
                    Position.Parse("A1"));

            Assert.NotNull(restoredRook);

            Assert.IsType<Rook>(restoredRook);

            Assert.Equal(
                PieceColor.White,
                restoredRook.Color);

            // Конь должен восстановиться
            ChessPiece? restoredKnight =
                board.GetPiece(
                    Position.Parse("A8"));

            Assert.NotNull(restoredKnight);

            Assert.IsType<Knight>(restoredKnight);

            Assert.Equal(
                PieceColor.Black,
                restoredKnight.Color);

            Assert.Equal(
                PieceColor.White,
                game.CurrentTurn);

            // Повторяем взятие
            MoveResult secondResult =
                game.Move(captureMove);

            // Assert
            Assert.True(secondResult.Success);

            Assert.Contains(
                secondResult.Events,
                e => e.Type == GameEventType.Capture);

            ChessPiece? finalRook =
                board.GetPiece(
                    Position.Parse("A8"));

            Assert.NotNull(finalRook);

            Assert.IsType<Rook>(finalRook);

            Assert.Equal(
                PieceColor.White,
                finalRook.Color);

            Assert.Null(
                board.GetPiece(
                    Position.Parse("A1")));
        }

        [Fact]
        public void Undo_Capture_Should_Restore_MoveCount()
        {
            // Arrange
            Board board = new();

            Rook rook = new(
                PieceColor.White,
                Position.Parse("A1"));

            Knight knight = new(
                PieceColor.Black,
                Position.Parse("A8"));

            board.AddPiece(
                new King(
                    PieceColor.White,
                    Position.Parse("E1")));

            board.AddPiece(rook);

            board.AddPiece(
                new King(
                    PieceColor.Black,
                    Position.Parse("E8")));

            board.AddPiece(knight);

            Game game = new(board);

            Move captureMove = new(
                Position.Parse("A1"),
                Position.Parse("A8"));

            // Начальное состояние
            Assert.Equal(
                0,
                rook.MoveCount);

            // Первый ход
            game.Move(captureMove);

            Assert.Equal(
                1,
                rook.MoveCount);

            // Undo
            game.Undo();

            Assert.Equal(
                0,
                rook.MoveCount);

            Assert.Same(
                rook,
                board.GetPiece(
                    Position.Parse("A1")));

            Assert.Same(
                knight,
                board.GetPiece(
                    Position.Parse("A8")));

            // Повторный ход
            game.Move(captureMove);

            Assert.Equal(
                1,
                rook.MoveCount);
        }

        [Fact]
        public void Undo_NormalMove_Should_Restore_HalfMoveClock()
        {
            // Arrange
            Board board = new();

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

            Game game = new(board);

            int initialHalfMoveClock =
                game.HalfMoveClock;

            Move move = new(
                Position.Parse("A1"),
                Position.Parse("A2"));

            // Act — обычный ход ладьёй
            game.Move(move);

            // Assert — счётчик должен увеличиться
            Assert.Equal(
                initialHalfMoveClock + 1,
                game.HalfMoveClock);

            // Act — отменяем ход
            game.Undo();

            // Assert — счётчик должен восстановиться
            Assert.Equal(
                initialHalfMoveClock,
                game.HalfMoveClock);

            // Дополнительно проверяем позицию
            ChessPiece? rook =
                board.GetPiece(
                    Position.Parse("A1"));

            Assert.NotNull(rook);

            Assert.IsType<Rook>(rook);

            Assert.Null(
                board.GetPiece(
                    Position.Parse("A2")));

            Assert.Equal(
                PieceColor.White,
                game.CurrentTurn);
        }

        [Fact]
        public void QueenSide_Castling_Should_Move_King_And_Rook()
        {
            // Arrange
            Board board = new();

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

            Game game = new(board);

            Move castleMove = new(
                Position.Parse("E1"),
                Position.Parse("C1"));

            // Act
            MoveResult result =
                game.Move(castleMove);

            // Assert
            Assert.True(result.Success);

            Assert.Contains(
                result.Events,
                e => e.Type == GameEventType.Castle);

            // Король должен оказаться на C1
            ChessPiece? king =
                board.GetPiece(
                    Position.Parse("C1"));

            Assert.NotNull(king);

            Assert.IsType<King>(king);

            Assert.Equal(
                PieceColor.White,
                king.Color);

            // Ладья должна оказаться на D1
            ChessPiece? rook =
                board.GetPiece(
                    Position.Parse("D1"));

            Assert.NotNull(rook);

            Assert.IsType<Rook>(rook);

            Assert.Equal(
                PieceColor.White,
                rook.Color);

            // Исходные клетки должны быть пустыми
            Assert.Null(
                board.GetPiece(
                    Position.Parse("E1")));

            Assert.Null(
                board.GetPiece(
                    Position.Parse("A1")));

            // Между исходной позицией и конечной
            // не должно появиться лишних фигур
            Assert.Null(
                board.GetPiece(
                    Position.Parse("B1")));

            // Ход должен перейти чёрным
            Assert.Equal(
                PieceColor.Black,
                game.CurrentTurn);
        }

        [Fact]
        public void QueenSide_Castling_Should_Not_Be_Allowed_Through_Attacked_Square()
        {
            // Arrange
            Board board = new();

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

            // Чёрная ладья атакует D1
            board.AddPiece(
                new Rook(
                    PieceColor.Black,
                    Position.Parse("D8")));

            Game game = new(board);

            Move castleMove = new(
                Position.Parse("E1"),
                Position.Parse("C1"));

            // Act + Assert
            InvalidOperationException exception =
                Assert.Throws<InvalidOperationException>(
                    () => game.Move(castleMove));

            Assert.Contains(
                "Недопустимый ход",
                exception.Message);

            // Король остаётся на месте
            ChessPiece? king =
                board.GetPiece(
                    Position.Parse("E1"));

            Assert.NotNull(king);
            Assert.IsType<King>(king);
            Assert.Equal(
                PieceColor.White,
                king.Color);

            // Ладья тоже остаётся на месте
            ChessPiece? rook =
                board.GetPiece(
                    Position.Parse("A1"));

            Assert.NotNull(rook);
            Assert.IsType<Rook>(rook);
            Assert.Equal(
                PieceColor.White,
                rook.Color);

            // Целевые клетки пустые
            Assert.Null(
                board.GetPiece(
                    Position.Parse("C1")));

            Assert.Null(
                board.GetPiece(
                    Position.Parse("D1")));

            // Ход остаётся за белыми
            Assert.Equal(
                PieceColor.White,
                game.CurrentTurn);
        }

        [Fact]
        public void Undo_QueenSide_Castling_Should_Allow_Castling_Again()
        {
            // Arrange
            Board board = new();

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

            Game game = new(board);

            Move castleMove = new(
                Position.Parse("E1"),
                Position.Parse("C1"));

            // Act — первая длинная рокировка
            MoveResult firstResult =
                game.Move(castleMove);

            // Assert
            Assert.True(firstResult.Success);

            Assert.Contains(
                firstResult.Events,
                e => e.Type == GameEventType.Castle);

            Assert.IsType<King>(
                board.GetPiece(
                    Position.Parse("C1")));

            Assert.IsType<Rook>(
                board.GetPiece(
                    Position.Parse("D1")));

            // Act — Undo
            game.Undo();

            // Assert — исходная позиция восстановлена
            ChessPiece? restoredKing =
                board.GetPiece(
                    Position.Parse("E1"));

            Assert.NotNull(restoredKing);

            Assert.IsType<King>(restoredKing);

            ChessPiece? restoredRook =
                board.GetPiece(
                    Position.Parse("A1"));

            Assert.NotNull(restoredRook);

            Assert.IsType<Rook>(restoredRook);

            Assert.Null(
                board.GetPiece(
                    Position.Parse("C1")));

            Assert.Null(
                board.GetPiece(
                    Position.Parse("D1")));

            Assert.Equal(
                PieceColor.White,
                game.CurrentTurn);

            // Главное: MoveCount должен восстановиться
            Assert.Equal(
                0,
                restoredKing.MoveCount);

            Assert.Equal(
                0,
                restoredRook.MoveCount);

            // Act — повторяем длинную рокировку
            MoveResult secondResult =
                game.Move(castleMove);

            // Assert
            Assert.True(secondResult.Success);

            Assert.Contains(
                secondResult.Events,
                e => e.Type == GameEventType.Castle);

            ChessPiece? finalKing =
                board.GetPiece(
                    Position.Parse("C1"));

            Assert.NotNull(finalKing);

            Assert.IsType<King>(finalKing);

            ChessPiece? finalRook =
                board.GetPiece(
                    Position.Parse("D1"));

            Assert.NotNull(finalRook);

            Assert.IsType<Rook>(finalRook);

            Assert.Equal(
                PieceColor.White,
                finalKing.Color);

            Assert.Equal(
                PieceColor.White,
                finalRook.Color);

            Assert.Null(
                board.GetPiece(
                    Position.Parse("E1")));

            Assert.Null(
                board.GetPiece(
                    Position.Parse("A1")));
        }

        [Fact]
        public void Pawn_Should_Promote_To_Rook()
        {
            // Arrange
            Board board = new();

            board.AddPiece(
                new King(
                    PieceColor.White,
                    Position.Parse("E1")));

            board.AddPiece(
                new Pawn(
                    PieceColor.White,
                    Position.Parse("A7")));

            board.AddPiece(
                new King(
                    PieceColor.Black,
                    Position.Parse("E8")));

            Game game = new(board);

            Move promotionMove = new(
                Position.Parse("A7"),
                Position.Parse("A8"),
                PromotionPiece.Rook);

            // Act
            MoveResult result =
                game.Move(promotionMove);

            // Assert
            Assert.True(result.Success);

            Assert.Contains(
                result.Events,
                e => e.Type == GameEventType.Promotion);

            // Пешка должна исчезнуть
            Assert.Null(
                board.GetPiece(
                    Position.Parse("A7")));

            // На A8 должна появиться ладья
            ChessPiece? promotedPiece =
                board.GetPiece(
                    Position.Parse("A8"));

            Assert.NotNull(promotedPiece);

            Assert.IsType<Rook>(promotedPiece);

            Assert.Equal(
                PieceColor.White,
                promotedPiece.Color);

            // Проверяем, что это действительно
            // новая фигура с начальным MoveCount
            Assert.Equal(
                0,
                promotedPiece.MoveCount);

            // После хода очередь чёрных
            Assert.Equal(
                PieceColor.Black,
                game.CurrentTurn);
        }

        [Fact]
        public void Pawn_Should_Promote_To_Bishop()
        {
            // Arrange
            Board board = new();

            board.AddPiece(
                new King(
                    PieceColor.White,
                    Position.Parse("E1")));

            board.AddPiece(
                new Pawn(
                    PieceColor.White,
                    Position.Parse("A7")));

            board.AddPiece(
                new King(
                    PieceColor.Black,
                    Position.Parse("E8")));

            Game game = new(board);

            Move promotionMove = new(
                Position.Parse("A7"),
                Position.Parse("A8"),
                PromotionPiece.Bishop);

            // Act
            MoveResult result =
                game.Move(promotionMove);

            // Assert
            Assert.True(result.Success);

            Assert.Contains(
                result.Events,
                e => e.Type == GameEventType.Promotion);

            // Исходная пешка должна исчезнуть
            Assert.Null(
                board.GetPiece(
                    Position.Parse("A7")));

            // На A8 должен появиться слон
            ChessPiece? promotedPiece =
                board.GetPiece(
                    Position.Parse("A8"));

            Assert.NotNull(promotedPiece);

            Assert.IsType<Bishop>(promotedPiece);

            Assert.Equal(
                PieceColor.White,
                promotedPiece.Color);

            // Новая фигура ещё не делала ходов
            Assert.Equal(
                0,
                promotedPiece.MoveCount);

            // После хода очередь чёрных
            Assert.Equal(
                PieceColor.Black,
                game.CurrentTurn);
        }

        [Fact]
        public void Pawn_Should_Promote_To_Knight()
        {
            // Arrange
            Board board = new();

            board.AddPiece(
                new King(
                    PieceColor.White,
                    Position.Parse("E1")));

            board.AddPiece(
                new Pawn(
                    PieceColor.White,
                    Position.Parse("A7")));

            board.AddPiece(
                new King(
                    PieceColor.Black,
                    Position.Parse("E8")));

            Game game = new(board);

            Move promotionMove = new(
                Position.Parse("A7"),
                Position.Parse("A8"),
                PromotionPiece.Knight);

            // Act
            MoveResult result =
                game.Move(promotionMove);

            // Assert
            Assert.True(result.Success);

            Assert.Contains(
                result.Events,
                e => e.Type == GameEventType.Promotion);

            Assert.Null(
                board.GetPiece(
                    Position.Parse("A7")));

            ChessPiece? promotedPiece =
                board.GetPiece(
                    Position.Parse("A8"));

            Assert.NotNull(promotedPiece);

            Assert.IsType<Knight>(promotedPiece);

            Assert.Equal(
                PieceColor.White,
                promotedPiece.Color);

            Assert.Equal(
                0,
                promotedPiece.MoveCount);

            Assert.Equal(
                PieceColor.Black,
                game.CurrentTurn);
        }

        [Fact]
        public void Undo_Knight_Promotion_Should_Restore_Pawn()
        {
            // Arrange
            Board board = new();

            board.AddPiece(
                new King(
                    PieceColor.White,
                    Position.Parse("E1")));

            Pawn pawn = new(
                PieceColor.White,
                Position.Parse("A7"));

            board.AddPiece(pawn);

            board.AddPiece(
                new King(
                    PieceColor.Black,
                    Position.Parse("E8")));

            Game game = new(board);

            Move promotionMove = new(
                Position.Parse("A7"),
                Position.Parse("A8"),
                PromotionPiece.Knight);

            // Act
            game.Move(promotionMove);

            // Проверяем превращение
            Assert.IsType<Knight>(
                board.GetPiece(
                    Position.Parse("A8")));

            // Undo
            game.Undo();

            // Assert
            ChessPiece? restoredPiece =
                board.GetPiece(
                    Position.Parse("A7"));

            Assert.NotNull(restoredPiece);

            Assert.IsType<Pawn>(restoredPiece);

            Assert.Same(
                pawn,
                restoredPiece);

            Assert.Null(
                board.GetPiece(
                    Position.Parse("A8")));

            Assert.Equal(
                PieceColor.White,
                game.CurrentTurn);
        }

        [Fact]
        public void King_And_Two_Bishops_Against_King_Should_Not_Be_Draw()
        {
            // Arrange
            Board board = new();

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

            Game game = new(board);

            // Act
            bool isDraw =
                game.IsDrawByInsufficientMaterial();

            // Assert
            Assert.False(isDraw);
        }

        [Fact]
        public void King_And_Rook_Against_King_Should_Not_Be_Draw()
        {
            // Arrange
            Board board = new();

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

            Game game = new(board);

            // Act
            bool isDraw =
                game.IsDrawByInsufficientMaterial();

            // Assert
            Assert.False(isDraw);
        }

        [Fact]
        public void King_And_Queen_Against_King_Should_Not_Be_Draw()
        {
            // Arrange
            Board board = new();

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

            Game game = new(board);

            // Act
            bool isDraw =
                game.IsDrawByInsufficientMaterial();

            // Assert
            Assert.False(isDraw);
        }

        [Fact]
        public void Multiple_Undo_Should_Restore_Initial_Position()
        {
            // Arrange
            Board board = new();

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

            board.AddPiece(
                new Rook(
                    PieceColor.Black,
                    Position.Parse("A8")));

            Game game = new(board);

            Move whiteMove1 = new(
                Position.Parse("A1"),
                Position.Parse("A2"));

            Move blackMove = new(
                Position.Parse("A8"),
                Position.Parse("A7"));

            Move whiteMove2 = new(
                Position.Parse("A2"),
                Position.Parse("A3"));

            // Act
            game.Move(whiteMove1);
            game.Move(blackMove);
            game.Move(whiteMove2);

            game.Undo();
            game.Undo();
            game.Undo();

            // Assert
            Assert.IsType<Rook>(
                board.GetPiece(
                    Position.Parse("A1")));

            Assert.IsType<Rook>(
                board.GetPiece(
                    Position.Parse("A8")));

            Assert.Null(
                board.GetPiece(
                    Position.Parse("A2")));

            Assert.Null(
                board.GetPiece(
                    Position.Parse("A3")));

            Assert.Null(
                board.GetPiece(
                    Position.Parse("A7")));

            Assert.Equal(
                PieceColor.White,
                game.CurrentTurn);

            Assert.Null(game.LastMove);
        }

        [Fact]
        public void Multiple_Undo_Should_Restore_LastMove()
        {
            // Arrange
            Board board = new();

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

            board.AddPiece(
                new Rook(
                    PieceColor.Black,
                    Position.Parse("A8")));

            Game game = new(board);

            Move whiteMove = new(
                Position.Parse("A1"),
                Position.Parse("A2"));

            Move blackMove = new(
                Position.Parse("A8"),
                Position.Parse("A7"));

            // Act
            game.Move(whiteMove);
            game.Move(blackMove);

            game.Undo();

            // Assert
            Assert.Equal(
                whiteMove,
                game.LastMove);

            Assert.Equal(
                PieceColor.Black,
                game.CurrentTurn);
        }
    }
}