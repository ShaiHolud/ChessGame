using ChessGame.Core.Model;
using ChessGame.Core.Pieces;

namespace ChessGame.Core.Factories
{
    public static class PromotionPieceFactory
    {
        public static ChessPiece Create(
            PieceColor color,
            Position position,
            PromotionPiece promotionPiece)
        {
            return promotionPiece switch
            {
                PromotionPiece.Queen =>
                    new Queen(color, position),

                PromotionPiece.Rook =>
                    new Rook(color, position),

                PromotionPiece.Bishop =>
                    new Bishop(color, position),

                PromotionPiece.Knight =>
                    new Knight(color, position),

                _ => throw new ArgumentOutOfRangeException(
                    nameof(promotionPiece),
                    promotionPiece,
                    null)
            };
        }
    }
}