namespace ChessGame.Core.Events
{
    public enum GameEventType
    {
        Capture,
        Check,
        Checkmate,
        Stalemate,
        Castle,
        Promotion,
        EnPassant,
        Draw
    }
}
