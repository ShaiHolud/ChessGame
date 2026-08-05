namespace ChessGame.Model
{
    public readonly record struct CastleInfo(
        Position RookFrom,
        Position RookTo);
}
