using ChessGame.Pieces;

namespace ChessGame.Model;

internal sealed class MoveState
{
    /// <summary>
    /// Фигура, выполнившая ход.
    /// </summary>
    public required ChessPiece Piece { get; init; }

    /// <summary>
    /// Исходная позиция фигуры.
    /// </summary>
    public required Position From { get; init; }

    /// <summary>
    /// Конечная позиция фигуры.
    /// </summary>
    public required Position To { get; init; }

    /// <summary>
    /// Количество ходов фигуры до выполнения хода.
    /// </summary>
    public required int PreviousMoveCount { get; init; }

    /// <summary>
    /// Взятая фигура (если была).
    /// </summary>
    public ChessPiece? CapturedPiece { get; init; }

    /// <summary>
    /// Позиция, на которой находилась взятая фигура.
    /// Для обычного взятия == To.
    /// Для взятия на проходе отличается от To.
    /// </summary>
    public Position CapturedPiecePosition { get; init; }

    /// <summary>
    /// Ладья, участвовавшая в рокировке.
    /// </summary>
    public ChessPiece? SecondaryPiece { get; init; }

    /// <summary>
    /// Исходная позиция второй фигуры.
    /// </summary>
    public Position? SecondaryFrom { get; init; }

    /// <summary>
    /// Конечная позиция второй фигуры.
    /// </summary>
    public Position? SecondaryTo { get; init; }

    /// <summary>
    /// Количество ходов второй фигуры до рокировки.
    /// </summary>
    public int? SecondaryPreviousMoveCount { get; init; }

    /// <summary>
    /// Фигура, появившаяся после превращения пешки.
    /// Например Queen.
    /// </summary>
    public ChessPiece? PromotedPiece { get; init; }

    public int CapturedPiecePreviousMoveCount { get; init; }
}