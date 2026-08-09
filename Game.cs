using ChessGame.Core.Events;
using ChessGame.Core.Model;
using ChessGame.Core.Pieces;
using ChessGame.Core.Results;

namespace ChessGame.Core
{
    public class Game
    {
        private readonly Board _board;

        private readonly Stack<MoveState> _history = [];

        public Move? LastMove { get; private set; }

        public Game(Board board)
        {
            _board = board;
        }

        public PieceColor CurrentTurn { get; private set; } = PieceColor.White;

        public MoveResult Move(Move move)
        {
            MoveResult result = new()
            {
                Success = true
            };

            ChessPiece? piece = _board.GetPiece(move.From);

            if (piece == null)
                throw new InvalidOperationException("Фигура не найдена.");

            if (piece.Color != CurrentTurn)
                throw new InvalidOperationException("Сейчас ход другого игрока.");

            if (IsCastle(move, out CastleInfo castleInfo))
            {
                MoveState state =
                    _board.MoveCastle(move, castleInfo);

                _history.Push(state);

                result.Events.Add(
                    new GameEvent(
                        GameEventType.Castle,
                        "Рокировка"));

                LastMove = move;

                CompleteMove(result);

                return result;
            }

            if (IsEnPassant(move, out Pawn enemyPawn))
            {
                MoveState enPassantState = _board.MoveEnPassant(move, enemyPawn);
                _history.Push(enPassantState);

                result.Events.Add(
                    new GameEvent(
                        GameEventType.EnPassant,
                        $"{piece.Color} Pawn {move.From} " +
                        $"Взятие {enemyPawn.Color} пешки на проходе"));

                LastMove = move;

                CompleteMove(result);

                return result;
            }

            IReadOnlyCollection<Move> legalMoves = GetLegalMoves(move.From);

            if (!legalMoves.Contains(move))
                throw new InvalidOperationException($"Недопустимый ход {move}.");

            MoveState normalMoveState = _board.MovePiece(move);

            _history.Push(normalMoveState);

            if (normalMoveState.CapturedPiece != null)
            {
                result.Events.Add(
                    new GameEvent(
                        GameEventType.Capture,
                        $"{piece.Color} {piece.GetType().Name} {move.From} " +
                        $"captures {normalMoveState.CapturedPiece.Color} " +
                        $"{normalMoveState.CapturedPiece.GetType().Name} {move.To}"));
            }

            GameEvent? promotion = TryPromotePawn(move.To);

            if (promotion != null)
            {
                result.Events.Add(promotion);
            }

            LastMove = move;

            CompleteMove(result);

            return result;
        }

        public MoveResult Move(string from, string to)
        {
            return Move(
                new Move(
                    Position.Parse(from),
                    Position.Parse(to)));
        }

        public IReadOnlyCollection<Move> GetPossibleMoves(Position position)
        {
            ChessPiece? piece = _board.GetPiece(position);

            if (piece == null)
                return [];

            return piece
                .GetPossibleMoves(_board)
                .Select(to => new Move(position, to))
                .ToList();
        }

        public IReadOnlyCollection<Move> GetPossibleMoves(string position)
        {
            return GetPossibleMoves(Position.Parse(position));
        }

        public bool IsCheck(PieceColor color)
        {
            return IsCheck(_board, color);
        }

        private static bool IsCheck(Board board, PieceColor color)
        {
            King king = board.GetKing(color);
            PieceColor opponentColor = color == PieceColor.White ? PieceColor.Black : PieceColor.White;

            foreach (ChessPiece piece in board.GetPieces(opponentColor))
            {
                if (piece.GetPossibleMoves(board).Contains(king.Position))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Возвращает все легальные ходы фигуры.
        /// Легальным считается ход, после которого собственный король
        /// не находится под шахом.
        /// </summary>
        /// <param name="position">
        /// Позиция фигуры.
        /// </param>
        /// <returns>
        /// Коллекция разрешённых ходов.
        /// </returns>
        public IReadOnlyCollection<Move> GetLegalMoves(Position position)
        {
            ChessPiece? piece = _board.GetPiece(position);

            if (piece == null)
                return [];

            IReadOnlyCollection<Position> possibleMoves =
                piece.GetPossibleMoves(_board);

            List<Move> legalMoves = [];

            foreach (Position to in possibleMoves)
            {
                Move move = new(position, to);

                MoveState state = _board.MovePiece(move);

                bool check = IsCheck(piece.Color);

                _board.UndoMove(state);

                if (!check)
                    legalMoves.Add(move);
            }

            return legalMoves;
        }

        public bool HasLegalMoves(PieceColor color)
        {
            foreach (ChessPiece piece in _board.GetPieces(color))
            {
                if (GetLegalMoves(piece.Position).Count > 0)
                    return true;
            }

            return false;
        }

        public bool IsCheckmate(PieceColor color)
        {
            return IsCheck(color)
                && !HasLegalMoves(color);
        }

        public bool IsStalemate(PieceColor color)
        {
            return !IsCheck(color)
                && !HasLegalMoves(color);
        }

        private GameEvent? TryPromotePawn(Position position)
        {
            ChessPiece? piece = _board.GetPiece(position);

            if (piece is not Pawn pawn)
                return null;

            if (pawn.Color == PieceColor.White && position.Row == 7)
            {
                _board.RemovePiece(position);

                _board.AddPiece(new Queen(
                    PieceColor.White,
                    position));

                return new GameEvent(GameEventType.Promotion, "Белая пешка превращена в ферзя");
            }

            if (pawn.Color == PieceColor.Black && position.Row == 0)
            {
                _board.RemovePiece(position);

                _board.AddPiece(new Queen(
                    PieceColor.Black,
                    position));

                return new GameEvent(GameEventType.Promotion, "Черная пешка превращена в ферзя");
            }

            return null;
        }

        private bool IsCastle(Move move, out CastleInfo castleInfo)
        {
            castleInfo = default;

            ChessPiece? king = _board.GetPiece(move.From);

            if (king is not King)
                return false;

            int deltaColumn = move.To.Column - move.From.Column;

            if (Math.Abs(deltaColumn) != 2)
                return false;

            if (king.MoveCount != 0)
                return false;

            bool kingSide = deltaColumn > 0;

            int row = move.From.Row;

            int rookColumn = kingSide ? 7 : 0;

            Position rookFrom = new(row, rookColumn);

            ChessPiece? rook = _board.GetPiece(rookFrom);

            if (rook is not Rook || rook.MoveCount != 0)
                return false;

            int step = kingSide ? 1 : -1;

            for (
                int col = move.From.Column + step;
                col != rookColumn;
                col += step)
            {
                if (_board.GetPiece(new Position(row, col)) != null)
                    return false;
            }

            // Король не может быть под шахом, проходить через битое поле
            // или вставать под шах.
            Position[] pathSquares = kingSide
                ? [move.From, new Position(row, move.From.Column + 1), move.To]
                : [move.From, new Position(row, move.From.Column - 1), move.To];


            foreach (Position square in pathSquares)
            {
                if (IsSquareAttacked(square, king.Color))
                    return false;
            }

            Position rookTo = kingSide
                ? new(row, move.To.Column - 1)
                : new(row, move.To.Column + 1);

            castleInfo = new CastleInfo(
                rookFrom,
                rookTo);

            return true;
        }

        private bool IsSquareAttacked(Position square, PieceColor color)
        {
            PieceColor opponentColor = color == PieceColor.White
                ? PieceColor.Black
                : PieceColor.White;

            foreach (ChessPiece piece in _board.GetPieces(opponentColor))
            {
                if (piece.GetPossibleMoves(_board).Contains(square))
                    return true;
            }

            return false;
        }

        private GameEvent? SwitchTurnAndCheckState()
        {
            CurrentTurn = CurrentTurn == PieceColor.White
                ? PieceColor.Black
                : PieceColor.White;

            if (IsCheckmate(CurrentTurn))
            {
                State = GameState.Checkmate;

                return new GameEvent(
                    GameEventType.Checkmate,
                    $"Мат. {CurrentTurn} проиграл.");
            }

            if (IsStalemate(CurrentTurn))
            {
                State = GameState.Stalemate;

                return new GameEvent(
                    GameEventType.Stalemate,
                    $"Пат. Ход {CurrentTurn}.");
            }

            if (IsCheck(CurrentTurn))
            {
                State = GameState.Check;

                return new GameEvent(
                    GameEventType.Check,
                    $"Шах {CurrentTurn}.");
            }

            State = GameState.Normal;

            return null;
        }

        public GameState State { get; private set; }

        public enum GameState
        {
            Normal,
            Check,
            Checkmate,
            Stalemate
        }

        private bool IsEnPassant(Move move, out Pawn capturedPawn)
        {
            capturedPawn = null!;

            if (LastMove is not Move lastMove)
                return false;

            ChessPiece? piece = _board.GetPiece(move.From);

            if (piece is not Pawn pawn)
                return false;

            ChessPiece? lastMovedPiece = _board.GetPiece(lastMove.To);

            if (lastMovedPiece is not Pawn enemyPawn)
                return false;

            if (enemyPawn.Color == pawn.Color)
                return false;

            // Последний ход должен быть двойным ходом пешки
            int lastMoveDelta = lastMove.To.Row - lastMove.From.Row;

            if (Math.Abs(lastMoveDelta) != 2)
                return false;

            // Пешки должны стоять рядом
            if (lastMove.To.Row != move.From.Row)
                return false;

            if (Math.Abs(lastMove.To.Column - move.From.Column) != 1)
                return false;

            // Ход по диагонали вперед
            int direction = pawn.Color == PieceColor.White ? 1 : -1;

            if (move.To.Row != move.From.Row + direction)
                return false;

            if (move.To.Column != lastMove.To.Column)
                return false;

            // Целевая клетка должна быть пустой
            if (_board.GetPiece(move.To) != null)
                return false;

            capturedPawn = enemyPawn;

            return true;
        }

        public void Undo()
        {
            if (_history.Count == 0)
                return;

            MoveState state = _history.Pop();

            _board.UndoMove(state);

            LastMove = _history.Count > 0
                ? new Move(_history.Peek().From, _history.Peek().To)
                : null;

            CurrentTurn = CurrentTurn == PieceColor.White
                ? PieceColor.Black
                : PieceColor.White;
        }

        public IEnumerable<ChessPiece> GetPieces(PieceColor pieceColor)
        {
            return _board.GetPieces(PieceColor.White)
                .Concat(_board.GetPieces(PieceColor.Black));
        }

        public IEnumerable<ChessPiece> GetAllPieces()
        {
            return _board.GetPieces(PieceColor.White)
                         .Concat(_board.GetPieces(PieceColor.Black));
        }

        private void CompleteMove(MoveResult result)
        {
            GameEvent? stateEvent =
                SwitchTurnAndCheckState();

            if (stateEvent != null)
            {
                result.Events.Add(stateEvent);
            }

            result.Check = State == GameState.Check;
            result.Checkmate = State == GameState.Checkmate;
            result.Stalemate = State == GameState.Stalemate;
        }
    }
}