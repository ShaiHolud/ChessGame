namespace ChessGame.Core.Model
{
    public readonly record struct Position(int Row, int Column)
    {
        public Position Offset(Direction direction)
        {
            return new Position(
                Row + direction.RowOffset,
                Column + direction.ColumnOffset);
        }

        public override string ToString() 
        {
            char file = (char)('A' + Column);
            int rank = Row + 1;
            return $"{file}{rank}";
        }

        public static bool TryParse(string? value, out Position position)
        {
            position = default;

            if (string.IsNullOrWhiteSpace(value) || value.Length != 2)
                return false;

            char file = char.ToUpperInvariant(value[0]);
            char rank = value[1];

            if (file is < 'A' or > 'H')
                return false;

            if (rank is < '1' or > '8')
                return false;

            position = new Position(
                rank - '1',
                file - 'A');

            return true;
        }

        public static Position Parse(string value)
        {
            if (!TryParse(value, out Position position))
                throw new FormatException(
                    $"Недопустимая шахматная позиция '{value}'.");

            return position;
        }
    }
}
