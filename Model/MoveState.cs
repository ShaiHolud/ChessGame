using ChessGame.Core.Pieces;

namespace ChessGame.Core.Model;

internal sealed record MoveState
{
    public required ChessPiece Piece { get; init; }

    public required Position From { get; init; }

    public required Position To { get; init; }

    public required int PreviousMoveCount { get; init; }

    public ChessPiece? CapturedPiece { get; init; }

    public Position CapturedPiecePosition { get; init; }

    public ChessPiece? SecondaryPiece { get; init; }

    public Position? SecondaryFrom { get; init; }

    public Position? SecondaryTo { get; init; }

    public int? SecondaryPreviousMoveCount { get; init; }

    public ChessPiece? PromotedPiece { get; init; }

    public int CapturedPiecePreviousMoveCount { get; init; }

    public GameState PreviousGameState { get; init; }

    public int PreviousHalfMoveClock { get; init; }
}