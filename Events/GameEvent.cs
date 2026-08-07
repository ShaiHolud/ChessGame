namespace ChessGame.Core.Events
{
    public sealed record GameEvent
    {
        public required GameEventType Type { get; init; }
    }
}
