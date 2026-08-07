namespace ChessGame.Core.Model
{
    public readonly record struct Move(
    Position From,
    Position To)
    {

        public override string ToString()
        {
            return $"{From} → {To}";
        }
    }
}
