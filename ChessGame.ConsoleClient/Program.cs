using ChessGame.Client;
using ChessGame.Contracts.Dto;
using ChessGame.Core.Events;
using static System.Net.WebRequestMethods;

HttpClientHandler handler = new() 
{ 
    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator 
}; 

HttpClient http = new(handler) 
{ 
    BaseAddress = new Uri("https://localhost:7059/") 
}; 

ChessApiClient client = new(http);

Guid id = await client.CreateGameAsync();

Console.WriteLine($"Game created: {id}");

GameStateResponse state =
    await client.GetGameAsync(id);

Console.WriteLine(
    $"Turn: {state.CurrentTurn}");

Console.WriteLine(
    $"Pieces: {state.Pieces.Count}");

//MoveResponse move1 =
//    await client.MoveAsync(id, "E2", "E4");

//GameStateResponse state1 =
//    await client.GetGameAsync(id);

//Console.WriteLine(
//    $"After E2-E4: turn={state1.CurrentTurn}, pieces={state1.Pieces.Count}");


//MoveResponse move2 =
//    await client.MoveAsync(id, "D7", "D5");

//GameStateResponse state2 =
//    await client.GetGameAsync(id);

//Console.WriteLine(
//    $"After D7-D5: turn={state2.CurrentTurn}, pieces={state2.Pieces.Count}");


//MoveResponse move3 =
//    await client.MoveAsync(id, "E4", "D5");

//GameStateResponse state3 =
//    await client.GetGameAsync(id);

//Console.WriteLine(
//    $"After E4-D5: turn={state3.CurrentTurn}, pieces={state3.Pieces.Count}");

//await client.UndoAsync(id);

//Console.WriteLine("Undo capture completed.");

//GameStateResponse undoState =
//    await client.GetGameAsync(id);

//Console.WriteLine(
//    $"After undo: turn={undoState.CurrentTurn}, " +
//    $"pieces={undoState.Pieces.Count}");

//foreach (GameEventDto gameEvent in move3.Events)
//{
//    Console.WriteLine(
//        $"{gameEvent.Type}: {gameEvent.Message}");
//}