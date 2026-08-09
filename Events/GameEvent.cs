namespace ChessGame.Core.Events;

public sealed record GameEvent(
    GameEventType Type,
    string? Message = null);