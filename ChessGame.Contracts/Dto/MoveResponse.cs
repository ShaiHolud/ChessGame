namespace ChessGame.Contracts.Dto;

public sealed class MoveResponse
{
    public bool Success { get; init; }

    public string? Error { get; init; }

    public List<GameEventDto> Events { get; init; } = [];

    public bool Check { get; init; }

    public bool Checkmate { get; init; }

    public bool Stalemate { get; init; }

    public bool Draw { get; init; }
}