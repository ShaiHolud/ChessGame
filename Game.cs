using ChessGame;
using ChessGame.Model;
using ChessGame.Pieces;

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

    public void Move(Move move)
    {
        ChessPiece? piece = _board.GetPiece(move.From);

        if (piece == null)
            throw new InvalidOperationException("Фигура не найдена.");

        if (piece.Color != CurrentTurn)
            throw new InvalidOperationException("Сейчас ход другого игрока.");

        if (TryCastle(move))
        {
            LastMove = move;
            SwitchTurnAndCheckState();
            return;
        }

        if (IsEnPassant(move, out Pawn enemyPawn))
        {
            MoveState enPassantState = _board.MoveEnPassant(move, enemyPawn);
            _history.Push(enPassantState);
            LastMove = move;
            SwitchTurnAndCheckState();

            return;
        }

        IReadOnlyCollection<Move> legalMoves = GetLegalMoves(move.From);

        if (!legalMoves.Contains(move))
            throw new InvalidOperationException($"Недопустимый ход {move}.");

        MoveState normalMoveState = _board.MovePiece(move);
        _history.Push(normalMoveState);
        TryPromotePawn(move.To);
        LastMove = move;
        SwitchTurnAndCheckState();
    }

    public void Move(string from, string to)
    {
        Move(new Move(
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

    private void TryPromotePawn(Position position)
    {
        ChessPiece? piece = _board.GetPiece(position);

        if (piece is not Pawn pawn)
            return;

        if (pawn.Color == PieceColor.White && position.Row == 7)
        {
            _board.RemovePiece(position);
            _board.AddPiece(new Queen(PieceColor.White, position));
        }
        else if (pawn.Color == PieceColor.Black && position.Row == 0)
        {
            _board.RemovePiece(position);
            _board.AddPiece(new Queen(PieceColor.Black, position));
        }
    }

    private bool TryCastle(Move move)
    {
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
        Position rookPosition = new(row, rookColumn);

        ChessPiece? rook = _board.GetPiece(rookPosition);

        if (rook is not Rook || rook.MoveCount != 0)
            return false;

        // Клетки между королём и ладьёй должны быть пусты.
        int step = kingSide ? 1 : -1;
        for (int col = move.From.Column + step; col != rookColumn; col += step)
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

        _board.MovePiece(move);

        Position rookTo = kingSide
            ? new Position(row, move.To.Column - 1)
            : new Position(row, move.To.Column + 1);

        _board.MovePiece(new Move(rookPosition, rookTo));

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

    private void SwitchTurnAndCheckState()
    {
        CurrentTurn = CurrentTurn == PieceColor.White
            ? PieceColor.Black
            : PieceColor.White;

        if (IsCheckmate(CurrentTurn))
            State = GameState.Checkmate;
        else if (IsStalemate(CurrentTurn))
            State = GameState.Stalemate;
        else if (IsCheck(CurrentTurn))
            State = GameState.Check;
        else
            State = GameState.Normal;
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
        //Console.WriteLine($"History = {_history.Count}");
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
}