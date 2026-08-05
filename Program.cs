using ChessGame;
using ChessGame.Pieces;
using ChessGame.Model;

Board board = new(); // создали доску

Game game = new(board); // создали партию

King king_b = new King(PieceColor.Black, Position.Parse("E8"));
King king_w = new King(PieceColor.White, Position.Parse("E1"));
Rook rook = new Rook(PieceColor.White, Position.Parse("A1"));

board.AddPiece(king_b);
board.AddPiece(king_w);
board.AddPiece(rook);

Console.WriteLine(board);

game.Move("E1", "C1");
Console.WriteLine(board);
Console.WriteLine(game.CurrentTurn);
Console.WriteLine(game.LastMove);

game.Undo();
Console.WriteLine(board);

game.Move("E1", "C1");
Console.WriteLine(board);
Console.WriteLine(game.CurrentTurn);
Console.WriteLine(game.LastMove);

game.Undo();
Console.WriteLine(board);
