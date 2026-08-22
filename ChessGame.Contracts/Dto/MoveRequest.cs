using ChessGame.Core.Model;
namespace ChessGame.Contracts.Dto;

public sealed class MoveRequest
{
    public required string From { get; init; }

    public required string To { get; init; }

    public PromotionPiece? Promotion { get; init; }
}