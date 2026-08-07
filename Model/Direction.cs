namespace ChessGame.Core.Model
{
    public readonly record struct Direction(
          int RowOffset,
          int ColumnOffset);
}
