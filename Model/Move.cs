namespace ChessGame.Core.Model
{

    public readonly record struct Move(Position From, Position To, PromotionPiece? Promotion = null)
    {
        public override string ToString()
        {
            if (Promotion.HasValue)
                return $"{From} → {To}={Promotion.Value}";

            return $"{From} → {To}";
        }
    }
}
