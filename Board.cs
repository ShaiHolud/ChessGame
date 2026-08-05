using ChessGame.Model;
using ChessGame.Pieces;

namespace ChessGame
{
    public class Board
    {
        private readonly ChessPiece?[,] _cells = new ChessPiece?[8, 8];

        public void AddPiece(ChessPiece piece)
        {
            Position position = piece.Position;
            if (!Contains(position))
                throw new InvalidOperationException(
                    $"Невозможно добавить фигуру {piece.ColorCode}{piece.ShortName} в позицию {position}.");

            _cells[position.Row, position.Column] = piece;
        }

        public ChessPiece? GetPiece(Position position)
        {
            return _cells[position.Row, position.Column];
        }

        public void RemovePiece(Position position)
        {
            _cells[position.Row, position.Column] = null;
        }

        internal MoveState MovePiece(Move move)
        {
            ChessPiece? piece = GetPiece(move.From);
            ChessPiece? capturedPiece = GetPiece(move.To);

            if (piece == null)
                throw new InvalidOperationException("Фигура не найдена.");

            MoveState state = new()
            {
                Piece = piece,
                From = move.From,
                To = move.To,
                CapturedPiece = capturedPiece,
                PreviousMoveCount = piece.MoveCount
            };

            if (capturedPiece != null)
            {
                capturedPiece.Capture();
                RemovePiece(move.To);
            }

            RemovePiece(move.From);

            piece.MoveTo(move.To);

            AddPiece(piece);

            return state;
        }

        internal void UndoMove(MoveState state)
        {
            // Удаляем основную фигуру с конечной клетки
            RemovePiece(state.To);

            // Возвращаем ходившую фигуру
            state.Piece.Restore(
                state.From,
                state.PreviousMoveCount,
                true);

            AddPiece(state.Piece);

            // Возвращаем взятую фигуру
            if (state.CapturedPiece != null)
            {
                state.CapturedPiece.Restore(
                    state.CapturedPiecePosition,
                    state.CapturedPiecePreviousMoveCount,
                    true);

                AddPiece(state.CapturedPiece);
            }

            // Если была рокировка — возвращаем ладью
            if (state.SecondaryPiece != null &&
                state.SecondaryFrom != null &&
                state.SecondaryTo != null &&
                state.SecondaryPreviousMoveCount != null)
            {
                RemovePiece(state.SecondaryTo.Value);

                state.SecondaryPiece.Restore(
                    state.SecondaryFrom.Value,
                    state.SecondaryPreviousMoveCount.Value,
                    true);

                AddPiece(state.SecondaryPiece);
            }
        }

        public bool IsCellFree(Position position)
        {
            return _cells[position.Row, position.Column] == null;
        }

        public bool Contains(Position position)
        {
            return position.Row >= 0 &&
                   position.Row < _cells.GetLength(0) &&
                   position.Column >= 0 &&
                   position.Column < _cells.GetLength(1);
        }

        public bool CanMoveTo(ChessPiece piece, Position position)
        {
            if (!Contains(position))
                return false;

            ChessPiece? target = GetPiece(position);

            if (target == null)
                return true;

            return target.Color != piece.Color;
        }

        public override string ToString()
        {
            var sb = new System.Text.StringBuilder();

            for (int row = 7; row >= 0; row--)
            {
                sb.Append(row + 1).Append(' ');
                for (int column = 0; column < 8; column++)
                {
                    ChessPiece? piece = GetPiece(new Position(row, column));
                    if (piece == null)
                        sb.Append(".. ");
                    else
                        sb.Append(piece.ColorCode).Append(piece.ShortName).Append(' ');
                }
                sb.AppendLine();
            }

            sb.Append("  ");
            for (int column = 0; column < 8; column++)
                sb.Append((char)('a' + column)).Append("  ");
            sb.AppendLine();

            return sb.ToString();
        }

        public IEnumerable<ChessPiece> GetPieces(PieceColor color)
        {
            foreach (ChessPiece? piece in _cells)
            {
                if (piece != null && piece.Color == color)
                    yield return piece;
            }
        }

        public King GetKing(PieceColor color)
        {
            foreach (ChessPiece piece in GetPieces(color))
            {
                if (piece is King king)
                    return king;
            }

            throw new InvalidOperationException($"Король цвета {color} не найден на доске.");
        }

        internal MoveState MoveEnPassant(Move move, Pawn capturedPawn)
        {
            ChessPiece? piece = GetPiece(move.From);

            if (piece == null)
                throw new InvalidOperationException("Фигура не найдена.");

            MoveState state = new()
            {
                Piece = piece,
                From = move.From,
                To = move.To,
                PreviousMoveCount = piece.MoveCount,

                CapturedPiece = capturedPawn,
                CapturedPiecePosition = capturedPawn.Position,
                CapturedPiecePreviousMoveCount = capturedPawn.MoveCount
            };

            RemovePiece(capturedPawn.Position);
            capturedPawn.Capture();

            RemovePiece(move.From);

            piece.MoveTo(move.To);

            AddPiece(piece);

            return state;
        }

        internal MoveState MoveCastle(Move move, CastleInfo castleInfo)
        {
            MoveState kingState = MovePiece(move);
            MoveState rookState = MovePiece(
                new Move(
                    castleInfo.RookFrom,
                    castleInfo.RookTo));

            return kingState with
            {
                SecondaryPiece = rookState.Piece,
                SecondaryFrom = rookState.From,
                SecondaryTo = rookState.To,
                SecondaryPreviousMoveCount = rookState.PreviousMoveCount,
            };
        }
    }
}