using ChessGame;
using ChessGame.Pieces;
using ChessGame.Model;

Board board = new(); // создали доску

Game game = new(board); // создали партию

King king_b = new King(PieceColor.Black, Position.Parse("E4"));
King king_w = new King(PieceColor.White, Position.Parse("E1"));
Pawn pawn_w = new Pawn(PieceColor.White, Position.Parse("B7"));

board.AddPiece(king_b);
board.AddPiece(king_w);
board.AddPiece(pawn_w);

Console.WriteLine(board);

game.Move("B7", "B8");
Console.WriteLine(board);
Console.WriteLine($"BP живой? {pawn_w.IsAlive} Ходов: {pawn_w.MoveCount}");
Console.WriteLine(game.CurrentTurn);
Console.WriteLine(game.LastMove);

game.Undo();
Console.WriteLine("После Undo");
Console.WriteLine(board);
Console.WriteLine($"BP живой? {pawn_w.IsAlive} Ходов: {pawn_w.MoveCount}");
Console.WriteLine(game.CurrentTurn);
Console.WriteLine(game.LastMove);

game.Move("B7", "B8");
Console.WriteLine(board);
Console.WriteLine($"BP живой? {pawn_w.IsAlive} Ходов: {pawn_w.MoveCount}");
Console.WriteLine(game.CurrentTurn);
Console.WriteLine(game.LastMove);

game.Undo();
Console.WriteLine("После Undo");
Console.WriteLine(board);
Console.WriteLine($"BP живой? {pawn_w.IsAlive} Ходов: {pawn_w.MoveCount}");
Console.WriteLine(game.CurrentTurn);
Console.WriteLine(game.LastMove);