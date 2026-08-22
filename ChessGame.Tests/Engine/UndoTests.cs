using ChessGame.Core;
using ChessGame.Core.Events;
using ChessGame.Core.Model;
using ChessGame.Core.Pieces;
using ChessGame.Core.Results;

namespace ChessGame.Tests.Engine
{
    public class UndoTests
    {
        [Fact]
        public void Undo_Should_Restore_Normal_Move()
        {
            // Arrange
            Board board = new();
            Game game = new(board);

            board.AddPiece(new King(PieceColor.White, Position.Parse("E1")));
            board.AddPiece(new King(PieceColor.Black, Position.Parse("E8")));

            Pawn whitePawn = new(PieceColor.White, Position.Parse("H2"));
            Rook whiteRook = new(PieceColor.White, Position.Parse("A1"));
            Knight whiteKnight = new(PieceColor.White, Position.Parse("A2"));
            Bishop whiteBishop = new(PieceColor.White, Position.Parse("A3"));
            Queen whiteQueen = new(PieceColor.White, Position.Parse("A4"));
            King whiteKing = new(PieceColor.White, Position.Parse("A5"));

            board.AddPiece(whitePawn);
            board.AddPiece(whiteRook);
            board.AddPiece(whiteKnight);
            board.AddPiece(whiteBishop);
            board.AddPiece(whiteQueen);
            board.AddPiece(whiteKing);

            // Act
            game.Move("H2", "H3");

            // Assert
            Assert.Null(board.GetPiece(Position.Parse("H2")));
            ChessPiece? piece = board.GetPiece(Position.Parse("H3"));
            Assert.NotNull(piece);
            Assert.Same(whitePawn, piece);
            Assert.Equal(1, whitePawn.MoveCount);
            Assert.True(whitePawn.IsAlive);

            // Act
            game.Undo();

            // Assert
            Assert.Null(board.GetPiece(Position.Parse("H3")));
            piece = board.GetPiece(Position.Parse("H2"));
            Assert.NotNull(piece);
            Assert.Same(whitePawn, piece);
            Assert.Equal(0, whitePawn.MoveCount);
            Assert.True(whitePawn.IsAlive);

        }

        [Fact]
        public void Undo_Should_Restore_Capture()
        {
            // Arrange
            Board board = new();
            Game game = new(board);

            board.AddPiece(new King(PieceColor.White, Position.Parse("E1")));
            board.AddPiece(new King(PieceColor.Black, Position.Parse("E8")));

            Pawn whitePawn = new(PieceColor.White, Position.Parse("E2"));
            Pawn blackPawn = new(PieceColor.Black, Position.Parse("D3"));

            board.AddPiece(whitePawn);
            board.AddPiece(blackPawn);

            // Act
            game.Move("E2", "D3");

            // Assert
            Assert.Null(board.GetPiece(Position.Parse("E2")));

            ChessPiece? piece = board.GetPiece(Position.Parse("D3"));

            Assert.NotNull(piece);
            Assert.Same(whitePawn, piece);

            Assert.Equal(1, whitePawn.MoveCount);
            Assert.True(whitePawn.IsAlive);

            Assert.False(blackPawn.IsAlive);

            // Act
            game.Undo();

            // Assert
            Assert.Same(whitePawn, board.GetPiece(Position.Parse("E2")));
            Assert.Same(blackPawn, board.GetPiece(Position.Parse("D3")));

            Assert.Equal(0, whitePawn.MoveCount);

            Assert.True(whitePawn.IsAlive);
            Assert.True(blackPawn.IsAlive);
        }

        [Fact]
        public void Undo_Should_Restore_EnPassant()
        {
            // Arrange
            Board board = new();
            Game game = new(board);

            King whiteKing =
                new(
                    PieceColor.White,
                    Position.Parse("E1"));

            King blackKing =
                new(
                    PieceColor.Black,
                    Position.Parse("E8"));

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

            board.AddPiece(whiteKing);
            board.AddPiece(blackKing);
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

            // Assert — состояние после En Passant
            Assert.True(result.Success);

            Assert.Same(
                whitePawn,
                board.GetPiece(Position.Parse("D6")));

            Assert.Null(
                board.GetPiece(Position.Parse("E5")));

            Assert.Null(
                board.GetPiece(Position.Parse("D5")));

            Assert.Equal(
                3,
                whitePawn.MoveCount);

            Assert.True(
                whitePawn.IsAlive);

            Assert.False(
                blackPawn.IsAlive);

            Assert.Equal(
                1,
                blackPawn.MoveCount);

            Assert.Same(
                blackHelperPawn,
                board.GetPiece(Position.Parse("A6")));

            // Act — Undo
            game.Undo();

            // Assert — полностью восстанавливаем позицию
            Assert.Same(
                whitePawn,
                board.GetPiece(Position.Parse("E5")));

            Assert.Same(
                blackPawn,
                board.GetPiece(Position.Parse("D5")));

            Assert.Same(
                blackHelperPawn,
                board.GetPiece(Position.Parse("A6")));

            Assert.Null(
                board.GetPiece(Position.Parse("D6")));

            Assert.Equal(
                2,
                whitePawn.MoveCount);

            Assert.Equal(
                1,
                blackPawn.MoveCount);

            Assert.True(
                whitePawn.IsAlive);

            Assert.True(
                blackPawn.IsAlive);

            Assert.True(
                blackHelperPawn.IsAlive);

            Assert.Equal(
                PieceColor.White,
                game.CurrentTurn);
        }

        [Fact]
        public void Undo_Should_Restore_Castle()
        {
            // Arrange
            Board board = new();
            Game game = new(board);

            board.AddPiece(new King(PieceColor.Black, Position.Parse("E8")));

            King whiteKing = new King(PieceColor.White, Position.Parse("E1"));
            Rook whiteRook_1 = new(PieceColor.White, Position.Parse("A1"));
            Rook whiteRook_2 = new(PieceColor.White, Position.Parse("A8"));

            board.AddPiece(whiteKing);
            board.AddPiece(whiteRook_1);
            board.AddPiece(whiteRook_2);

            // Act
            game.Move("E1", "C1");

            // Assert
            Assert.Null(board.GetPiece(Position.Parse("E1")));
            Assert.Null(board.GetPiece(Position.Parse("A1")));
            ChessPiece? piece_1 = board.GetPiece(Position.Parse("C1"));
            Assert.NotNull(piece_1);
            Assert.Same(whiteKing, piece_1);
            ChessPiece? piece_2 = board.GetPiece(Position.Parse("D1"));
            Assert.NotNull(piece_2);
            Assert.Same(whiteRook_1, piece_2);


            Assert.Null(board.GetPiece(Position.Parse("A1")));
            Assert.Null(board.GetPiece(Position.Parse("E1")));
            Assert.Equal(1, whiteKing.MoveCount);
            Assert.Equal(1, piece_1.MoveCount);
            Assert.True(whiteKing.IsAlive);
            Assert.True(piece_1.IsAlive);

            // Act
            game.Undo();

            // Assert
            Assert.NotNull(board.GetPiece(Position.Parse("E1")));
            Assert.Null(board.GetPiece(Position.Parse("C1")));
            piece_1 = board.GetPiece(Position.Parse("E1"));
            Assert.NotNull(piece_1);
            Assert.Same(whiteKing, piece_1);
            Assert.Equal(0, whiteKing.MoveCount);
            Assert.True(whiteKing.IsAlive);

            Assert.NotNull(board.GetPiece(Position.Parse("A1")));
            Assert.Null(board.GetPiece(Position.Parse("D1")));
            piece_2 = board.GetPiece(Position.Parse("A1"));
            Assert.NotNull(piece_2);
            Assert.Same(whiteRook_1, piece_2);
            Assert.Equal(0, whiteRook_1.MoveCount);
            Assert.True(whiteRook_1.IsAlive);
        }

        [Fact]
        public void Undo_Should_Restore_Promotion()
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

            // Act — B7 -> B8, promotion
            MoveResult result =
                game.Move("B7", "B8");

            // Assert — promotion выполнен
            Assert.True(result.Success);

            ChessPiece? promotedPiece =
                board.GetPiece(
                    Position.Parse("B8"));

            Assert.NotNull(promotedPiece);

            Assert.IsType<Queen>(
                promotedPiece);

            Assert.Equal(
                PieceColor.White,
                promotedPiece.Color);

            // Исходная клетка пустая
            Assert.Null(
                board.GetPiece(
                    Position.Parse("B7")));

            // Act — Undo
            game.Undo();

            // Assert — исходная пешка восстановлена
            ChessPiece? restoredPiece =
                board.GetPiece(
                    Position.Parse("B7"));

            Assert.NotNull(restoredPiece);

            // Восстановлен именно исходный объект Pawn
            Assert.Same(
                whitePawn,
                restoredPiece);

            Assert.IsType<Pawn>(
                restoredPiece);

            Assert.Equal(
                PieceColor.White,
                restoredPiece.Color);

            // Пешка снова жива
            Assert.True(
                whitePawn.IsAlive);

            // MoveCount вернулся в состояние до promotion
            Assert.Equal(
                0,
                whitePawn.MoveCount);

            // B8 снова свободна
            Assert.Null(
                board.GetPiece(
                    Position.Parse("B8")));

            // Ход снова White
            Assert.Equal(
                PieceColor.White,
                game.CurrentTurn);
        }

        [Fact]
        public void Undo_Should_Restore_Pawn_After_Promotion()
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
                    PieceColor.White,
                    Position.Parse("B7")));

            // Act
            MoveResult result =
                game.Move(
                    new Move(
                        Position.Parse("B7"),
                        Position.Parse("B8"),
                        PromotionPiece.Rook));

            // Assert — promotion выполнен
            Assert.True(result.Success);

            ChessPiece? promotedPiece =
                board.GetPiece(
                    Position.Parse("B8"));

            Assert.NotNull(promotedPiece);
            Assert.IsType<Rook>(promotedPiece);

            Assert.Null(
                board.GetPiece(
                    Position.Parse("B7")));

            // Act — Undo
            game.Undo();

            // Assert — исходное состояние восстановлено
            ChessPiece? restoredPiece =
                board.GetPiece(
                    Position.Parse("B7"));

            Assert.NotNull(restoredPiece);
            Assert.IsType<Pawn>(restoredPiece);
            Assert.Equal(
                PieceColor.White,
                restoredPiece.Color);

            Assert.Null(
                board.GetPiece(
                    Position.Parse("B8")));

            Assert.Equal(
                PieceColor.White,
                game.CurrentTurn);
        }

        [Fact]
        public void MoveHistory_Should_Contain_Made_Move()
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
                    PieceColor.White,
                    Position.Parse("E2")));

            Move move = new(
                Position.Parse("E2"),
                Position.Parse("E4"));

            // Act
            game.Move(move);

            // Assert
            Assert.Single(game.MoveHistory);

            Assert.Equal(
                move,
                game.MoveHistory[0]);
        }

        [Fact]
        public void Undo_Should_Remove_Last_Move_From_History()
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
                    PieceColor.White,
                    Position.Parse("E2")));

            Move move = new(
                Position.Parse("E2"),
                Position.Parse("E4"));

            game.Move(move);

            Assert.Single(game.MoveHistory);

            // Act
            game.Undo();

            // Assert
            Assert.Empty(game.MoveHistory);
        }

        [Fact]
        public void MoveHistory_Should_Preserve_Order_Of_Moves()
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
                    PieceColor.White,
                    Position.Parse("E2")));

            board.AddPiece(
                new Pawn(
                    PieceColor.Black,
                    Position.Parse("E7")));

            Move firstMove = new(
                Position.Parse("E2"),
                Position.Parse("E4"));

            Move secondMove = new(
                Position.Parse("E7"),
                Position.Parse("E5"));

            // Act
            game.Move(firstMove);
            game.Move(secondMove);

            // Assert
            Assert.Equal(
                2,
                game.MoveHistory.Count);

            Assert.Equal(
                firstMove,
                game.MoveHistory[0]);

            Assert.Equal(
                secondMove,
                game.MoveHistory[1]);
        }

        [Fact]
        public void Undo_Should_Remove_Moves_From_History_In_Reverse_Order()
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
                    PieceColor.White,
                    Position.Parse("E2")));

            board.AddPiece(
                new Pawn(
                    PieceColor.Black,
                    Position.Parse("E7")));

            Move firstMove = new(
                Position.Parse("E2"),
                Position.Parse("E4"));

            Move secondMove = new(
                Position.Parse("E7"),
                Position.Parse("E5"));

            // Act
            game.Move(firstMove);
            game.Move(secondMove);

            Assert.Equal(2, game.MoveHistory.Count);

            game.Undo();

            // Assert
            Assert.Single(game.MoveHistory);

            Assert.Equal(
                firstMove,
                game.MoveHistory[0]);

            game.Undo();

            Assert.Empty(game.MoveHistory);
        }

        [Fact]
        public void MoveHistory_Should_Contain_Castle_Move()
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

            Move castleMove = new(
                Position.Parse("E1"),
                Position.Parse("G1"));

            // Act
            game.Move(castleMove);

            // Assert
            Assert.Single(game.MoveHistory);

            Assert.Equal(
                castleMove,
                game.MoveHistory[0]);
        }

        [Fact]
        public void MoveHistory_Should_Contain_EnPassant_Move()
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
                    PieceColor.White,
                    Position.Parse("E2")));

            board.AddPiece(
                new Pawn(
                    PieceColor.Black,
                    Position.Parse("D4")));

            Move whiteMove = new(
                Position.Parse("E2"),
                Position.Parse("E4"));

            Move enPassantMove = new(
                Position.Parse("D4"),
                Position.Parse("E3"));

            // Act
            game.Move(whiteMove);
            game.Move(enPassantMove);

            // Assert
            Assert.Equal(
                2,
                game.MoveHistory.Count);

            Assert.Equal(
                whiteMove,
                game.MoveHistory[0]);

            Assert.Equal(
                enPassantMove,
                game.MoveHistory[1]);
        }

        [Fact]
        public void MoveHistory_Should_Contain_Promotion_Move()
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
                    PieceColor.White,
                    Position.Parse("B7")));

            Move promotionMove = new(
                Position.Parse("B7"),
                Position.Parse("B8"),
                PromotionPiece.Rook);

            // Act
            game.Move(promotionMove);

            // Assert
            Assert.Single(game.MoveHistory);

            Assert.Equal(
                promotionMove,
                game.MoveHistory[0]);

            Assert.Equal(
                PromotionPiece.Rook,
                game.MoveHistory[0].Promotion);
        }

        [Fact]
        public void Undo_Promotion_Should_Remove_Move_From_History()
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
                    PieceColor.White,
                    Position.Parse("B7")));

            Move promotionMove = new(
                Position.Parse("B7"),
                Position.Parse("B8"),
                PromotionPiece.Rook);

            game.Move(promotionMove);

            Assert.Single(game.MoveHistory);

            // Act
            game.Undo();

            // Assert
            Assert.Empty(game.MoveHistory);

            ChessPiece? restoredPiece =
                board.GetPiece(
                    Position.Parse("B7"));

            Assert.NotNull(restoredPiece);
            Assert.IsType<Pawn>(restoredPiece);

            Assert.Null(
                board.GetPiece(
                    Position.Parse("B8")));
        }

        [Fact]
        public void Undo_Castle_Should_Remove_Move_From_History()
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

            Move castleMove = new(
                Position.Parse("E1"),
                Position.Parse("G1"));

            game.Move(castleMove);

            Assert.Single(game.MoveHistory);

            // Act
            game.Undo();

            // Assert
            Assert.Empty(game.MoveHistory);

            ChessPiece? king =
                board.GetPiece(
                    Position.Parse("E1"));

            ChessPiece? rook =
                board.GetPiece(
                    Position.Parse("H1"));

            Assert.NotNull(king);
            Assert.IsType<King>(king);

            Assert.NotNull(rook);
            Assert.IsType<Rook>(rook);

            Assert.Null(
                board.GetPiece(
                    Position.Parse("G1")));

            Assert.Null(
                board.GetPiece(
                    Position.Parse("F1")));
        }

        [Fact]
        public void Undo_EnPassant_Should_Remove_Move_From_History()
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
                    PieceColor.White,
                    Position.Parse("E2")));

            board.AddPiece(
                new Pawn(
                    PieceColor.Black,
                    Position.Parse("D4")));

            Move whiteMove = new(
                Position.Parse("E2"),
                Position.Parse("E4"));

            Move enPassantMove = new(
                Position.Parse("D4"),
                Position.Parse("E3"));

            // Act
            game.Move(whiteMove);
            game.Move(enPassantMove);

            Assert.Equal(2, game.MoveHistory.Count);

            game.Undo();

            // Assert
            Assert.Single(game.MoveHistory);

            Assert.Equal(
                whiteMove,
                game.MoveHistory[0]);

            // Black pawn must return to D4
            ChessPiece? blackPawn =
                board.GetPiece(
                    Position.Parse("D4"));

            Assert.NotNull(blackPawn);
            Assert.IsType<Pawn>(blackPawn);
            Assert.Equal(
                PieceColor.Black,
                blackPawn.Color);

            // White pawn must return to E4
            ChessPiece? whitePawn =
                board.GetPiece(
                    Position.Parse("E4"));

            Assert.NotNull(whitePawn);
            Assert.IsType<Pawn>(whitePawn);
            Assert.Equal(
                PieceColor.White,
                whitePawn.Color);

            // Destination of en passant must be empty
            Assert.Null(
                board.GetPiece(
                    Position.Parse("E3")));
        }

        [Fact]
        public void Undo_After_Normal_Move_Should_Restore_Normal_State()
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
                new Rook(
                    PieceColor.White,
                    Position.Parse("A1")));

            // Act
            game.Move(
                new Move(
                    Position.Parse("A1"),
                    Position.Parse("A2")));

            game.Undo();

            // Assert
            Assert.Equal(
                GameState.Normal,
                game.State);

            Assert.Equal(
                PieceColor.White,
                game.CurrentTurn);

            Assert.NotNull(
                board.GetPiece(
                    Position.Parse("A1")));

            Assert.Null(
                board.GetPiece(
                    Position.Parse("A2")));

            Assert.Empty(game.MoveHistory);
        }

        [Fact]
        public void Undo_After_Draw_Should_Restore_LastMove()
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

            Assert.Equal(GameState.Draw, game.State);
            Assert.Equal(move, game.LastMove);

            game.Undo();

            // Assert
            Assert.Equal(
                GameState.Normal,
                game.State);

            Assert.Null(game.LastMove);
        }

        [Fact]
        public void Undo_Multiple_Moves_Should_Restore_Previous_Game_States()
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
                new Rook(
                    PieceColor.White,
                    Position.Parse("A1")));

            board.AddPiece(
                new Rook(
                    PieceColor.Black,
                    Position.Parse("H8")));

            // Act
            game.Move(
                new Move(
                    Position.Parse("A1"),
                    Position.Parse("A2")));

            game.Move(
                new Move(
                    Position.Parse("H8"),
                    Position.Parse("H7")));

            // Assert before undo
            Assert.Equal(
                GameState.Normal,
                game.State);

            // Undo black move
            game.Undo();

            Assert.Equal(
                GameState.Normal,
                game.State);

            Assert.NotNull(
                board.GetPiece(
                    Position.Parse("H8")));

            Assert.Null(
                board.GetPiece(
                    Position.Parse("H7")));

            // Undo white move
            game.Undo();

            Assert.Equal(
                GameState.Normal,
                game.State);

            Assert.NotNull(
                board.GetPiece(
                    Position.Parse("A1")));

            Assert.Null(
                board.GetPiece(
                    Position.Parse("A2")));

            Assert.Empty(game.MoveHistory);
        }

        [Fact]
        public void King_And_Rook_Against_King_Should_Not_Be_Draw()
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
            MoveResult result =
                game.Move(
                    new Move(
                        Position.Parse("A1"),
                        Position.Parse("A2")));

            // Assert
            Assert.True(result.Success);
            Assert.False(result.Draw);
            Assert.NotEqual(GameState.Draw, game.State);
        }

        [Fact]
        public void King_And_Bishop_Against_King_Should_Be_Draw()
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
            MoveResult result =
                game.Move(
                    new Move(
                        Position.Parse("C1"),
                        Position.Parse("D2")));

            // Assert
            Assert.True(result.Success);
            Assert.True(result.Draw);

            Assert.Equal(
                GameState.Draw,
                game.State);
        }

        [Fact]
        public void King_And_Knight_Against_King_Should_Be_Draw_After_Move()
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
            MoveResult result =
                game.Move(
                    new Move(
                        Position.Parse("B1"),
                        Position.Parse("C3")));

            // Assert
            Assert.True(result.Success);
            Assert.True(result.Draw);

            Assert.Equal(
                GameState.Draw,
                game.State);

            Assert.Contains(
                result.Events,
                e => e.Type == GameEventType.Draw);
        }

        [Fact]
        public void King_And_Queen_Against_King_Should_Not_Be_Draw_After_Move()
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
            MoveResult result =
                game.Move(
                    new Move(
                        Position.Parse("D1"),
                        Position.Parse("D2")));

            // Assert
            Assert.True(result.Success);
            Assert.False(result.Draw);

            Assert.NotEqual(
                GameState.Draw,
                game.State);

            Assert.DoesNotContain(
                result.Events,
                e => e.Type == GameEventType.Draw);
        }

        [Fact]
        public void Undo_After_Draw_Should_Restore_Previous_Check_State()
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
                    Position.Parse("E6")));

            board.AddPiece(
                new King(
                    PieceColor.Black,
                    Position.Parse("E8")));

            // Белая ладья даёт шах
            game.Move(
                new Move(
                    Position.Parse("E6"),
                    Position.Parse("E7")));

            Assert.Equal(
                GameState.Check,
                game.State);

            Assert.Equal(
                PieceColor.Black,
                game.CurrentTurn);

            // Чёрный король берёт ладью.
            // Остаются два короля -> Draw.
            game.Move(
                new Move(
                    Position.Parse("E8"),
                    Position.Parse("E7")));

            Assert.Equal(
                GameState.Draw,
                game.State);

            // Act
            game.Undo();

            // Assert

            Assert.Equal(
                GameState.Check,
                game.State);

            Assert.Equal(
                PieceColor.Black,
                game.CurrentTurn);

            ChessPiece? blackKing =
                board.GetPiece(
                    Position.Parse("E8"));

            Assert.NotNull(blackKing);
            Assert.IsType<King>(blackKing);
            Assert.Equal(
                PieceColor.Black,
                blackKing.Color);

            ChessPiece? whiteRook =
                board.GetPiece(
                    Position.Parse("E7"));

            Assert.NotNull(whiteRook);
            Assert.IsType<Rook>(whiteRook);
            Assert.Equal(
                PieceColor.White,
                whiteRook.Color);

            Assert.Equal(
                new Move(
                    Position.Parse("E6"),
                    Position.Parse("E7")),
                game.LastMove);
        }

        [Fact]
        public void Move_After_Checkmate_Should_Throw_Exception()
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

            // Белые ставят мат
            MoveResult mateResult =
                game.Move(
                    new Move(
                        Position.Parse("G6"),
                        Position.Parse("G7")));

            Assert.True(mateResult.Success);

            Assert.True(mateResult.Checkmate);

            Assert.Equal(
                GameState.Checkmate,
                game.State);

            // Act + Assert
            InvalidOperationException exception =
                Assert.Throws<InvalidOperationException>(
                    () => game.Move(
                        new Move(
                            Position.Parse("H8"),
                            Position.Parse("H7"))));

            Assert.Equal(
                "Партия окончена.",
                exception.Message);
        }

        [Fact]
        public void Move_After_Stalemate_Should_Throw_Exception()
        {
            // Arrange
            Board board = new();
            Game game = new(board);

            board.AddPiece(
                new King(
                    PieceColor.White,
                    Position.Parse("C6")));

            board.AddPiece(
                new Queen(
                    PieceColor.White,
                    Position.Parse("B6")));

            board.AddPiece(
                new King(
                    PieceColor.Black,
                    Position.Parse("A8")));

            // Белые создают пат
            MoveResult stalemateResult =
                game.Move(
                    new Move(
                        Position.Parse("B6"),
                        Position.Parse("C7")));

            Assert.True(stalemateResult.Success);

            Assert.True(stalemateResult.Stalemate);

            Assert.Equal(
                GameState.Stalemate,
                game.State);

            // Act + Assert
            InvalidOperationException exception =
                Assert.Throws<InvalidOperationException>(
                    () => game.Move(
                        new Move(
                            Position.Parse("A8"),
                            Position.Parse("A7"))));

            Assert.Equal(
                "Партия окончена.",
                exception.Message);
        }

        [Theory]
        [InlineData(GameState.Normal, false)]
        [InlineData(GameState.Check, false)]
        [InlineData(GameState.Checkmate, true)]
        [InlineData(GameState.Stalemate, true)]
        [InlineData(GameState.Draw, true)]
        public void IsFinished_Should_Return_Correct_Value(GameState state, bool expected)
        {
            // Arrange
            Board board = new();
            Game game = new(board);

            typeof(Game)
                .GetProperty(nameof(Game.State))!
                .SetValue(game, state);

            // Act
            bool result = game.IsFinished;

            // Assert
            Assert.Equal(expected, result);
        }
    }
}
