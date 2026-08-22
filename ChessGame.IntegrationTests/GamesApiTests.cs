using System.Net;
using System.Net.Http.Json;
using ChessGame.Contracts.Dto;
using Microsoft.AspNetCore.Mvc.Testing;
using ChessGame.Core.Events;
using Xunit.Abstractions;
using ChessGame.Core.Model;
using ChessGame.Core.Pieces;
using ChessGame.Core.Results;
using ChessGame.Core;

namespace ChessGame.IntegrationTests;

public class GamesApiTests : IClassFixture<WebApplicationFactory<Program>>
{

    private readonly HttpClient _client;
    private readonly ITestOutputHelper _output;
    public GamesApiTests(
    WebApplicationFactory<Program> factory,
    ITestOutputHelper output)
    {
        _client = factory.CreateClient();
        _output = output;
    }

    private async Task MakeMove(Guid gameId, string from, string to)
    {
        HttpResponseMessage response =
            await _client.PostAsJsonAsync(
                $"/api/games/{gameId}/move",
                new MoveRequest
                {
                    From = from,
                    To = to
                });

        response.EnsureSuccessStatusCode();
    }

    private async Task<GameStateResponse> GetGameState(Guid gameId)
    {
        HttpResponseMessage response =
            await _client.GetAsync(
                $"/api/games/{gameId}");

        response.EnsureSuccessStatusCode();

        GameStateResponse? state =
            await response.Content
                .ReadFromJsonAsync<GameStateResponse>();

        Assert.NotNull(state);

        return state;
    }

    [Fact]
    public async Task CreateGame_Should_Return_GameId()
    {
        // Act
        HttpResponseMessage response =
            await _client.PostAsync(
                "/api/games",
                null);

        // Assert
        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        CreateGameResponse? result =
            await response.Content
                .ReadFromJsonAsync<CreateGameResponse>();

        Assert.NotNull(result);
        Assert.NotEqual(
            Guid.Empty,
            result.Id);
    }

    [Fact]
    public async Task GetGame_Should_Return_Initial_Game_State()
    {
        // Arrange
        HttpResponseMessage createResponse =
            await _client.PostAsync(
                "/api/games",
                null);

        createResponse.EnsureSuccessStatusCode();

        CreateGameResponse? created =
            await createResponse.Content
                .ReadFromJsonAsync<CreateGameResponse>();

        Assert.NotNull(created);

        // Act
        HttpResponseMessage response =
            await _client.GetAsync(
                $"/api/games/{created.Id}");

        // Assert
        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        GameStateResponse? state =
            await response.Content
                .ReadFromJsonAsync<GameStateResponse>();

        Assert.NotNull(state);

        Assert.Equal(
            created.Id,
            state.Id);

        Assert.Equal(
            "White",
            state.CurrentTurn);

        Assert.Equal(
            32,
            state.Pieces.Count);
    }

    [Fact]
    public async Task Move_Should_Return_Success_And_Change_Turn()
    {
        // Arrange
        HttpResponseMessage createResponse =
            await _client.PostAsync(
                "/api/games",
                null);

        createResponse.EnsureSuccessStatusCode();

        CreateGameResponse? created =
            await createResponse.Content
                .ReadFromJsonAsync<CreateGameResponse>();

        Assert.NotNull(created);

        MoveRequest request = new()
        {
            From = "E2",
            To = "E4"
        };

        // Act
        HttpResponseMessage moveResponse =
            await _client.PostAsJsonAsync(
                $"/api/games/{created.Id}/move",
                request);

        // Assert
        Assert.Equal(
            HttpStatusCode.OK,
            moveResponse.StatusCode);

        MoveResponse? result =
            await moveResponse.Content
                .ReadFromJsonAsync<MoveResponse>();

        Assert.NotNull(result);

        Assert.True(result.Success);
        Assert.False(result.Check);
        Assert.False(result.Checkmate);
        Assert.False(result.Stalemate);

        // Проверяем состояние игры после хода
        HttpResponseMessage stateResponse =
            await _client.GetAsync(
                $"/api/games/{created.Id}");

        stateResponse.EnsureSuccessStatusCode();

        GameStateResponse? state =
            await stateResponse.Content
                .ReadFromJsonAsync<GameStateResponse>();

        Assert.NotNull(state);

        Assert.Equal(
            "Black",
            state.CurrentTurn);

        Assert.Equal(
            32,
            state.Pieces.Count);
    }

    [Fact]
    public async Task Undo_Should_Restore_Game_State()
    {
        // Arrange
        HttpResponseMessage createResponse =
            await _client.PostAsync(
                "/api/games",
                null);

        createResponse.EnsureSuccessStatusCode();

        CreateGameResponse? created =
            await createResponse.Content
                .ReadFromJsonAsync<CreateGameResponse>();

        Assert.NotNull(created);

        MoveRequest moveRequest = new()
        {
            From = "E2",
            To = "E4"
        };

        HttpResponseMessage moveResponse =
            await _client.PostAsJsonAsync(
                $"/api/games/{created.Id}/move",
                moveRequest);

        moveResponse.EnsureSuccessStatusCode();

        // Act
        HttpResponseMessage undoResponse =
            await _client.PostAsync(
                $"/api/games/{created.Id}/undo",
                null);

        // Assert
        Assert.Equal(
            HttpStatusCode.OK,
            undoResponse.StatusCode);

        HttpResponseMessage stateResponse =
            await _client.GetAsync(
                $"/api/games/{created.Id}");

        stateResponse.EnsureSuccessStatusCode();

        GameStateResponse? state =
            await stateResponse.Content
                .ReadFromJsonAsync<GameStateResponse>();

        Assert.NotNull(state);

        // После Undo снова ход белых
        Assert.Equal(
            "White",
            state.CurrentTurn);

        // Все 32 фигуры должны быть на месте
        Assert.Equal(
            32,
            state.Pieces.Count);

        // Пешка должна вернуться на E2
        PieceDto? pawn =
            state.Pieces.FirstOrDefault(
                p => p.Position == "E2");

        Assert.NotNull(pawn);
    }

    [Fact]
    public async Task Capture_And_Undo_Should_Restore_Captured_Piece()
    {
        // Arrange
        HttpResponseMessage createResponse =
            await _client.PostAsync(
                "/api/games",
                null);

        createResponse.EnsureSuccessStatusCode();

        CreateGameResponse? created =
            await createResponse.Content
                .ReadFromJsonAsync<CreateGameResponse>();

        Assert.NotNull(created);

        // E2 -> E4
        await MakeMove(created.Id, "E2", "E4");

        // D7 -> D5
        await MakeMove(
            created.Id,
            "D7",
            "D5");

        // E4 -> D5 — взятие
        HttpResponseMessage captureResponse =
            await _client.PostAsJsonAsync(
                $"/api/games/{created.Id}/move",
                new MoveRequest
                {
                    From = "E4",
                    To = "D5"
                });

        captureResponse.EnsureSuccessStatusCode();

        MoveResponse? captureResult =
            await captureResponse.Content
                .ReadFromJsonAsync<MoveResponse>();

        Assert.NotNull(captureResult);
        Assert.True(captureResult.Success);

        // Проверяем Capture event
        GameEventDto? captureEvent = captureResult.Events.FirstOrDefault(e => e.TypeEnum == GameEventType.Capture);


        Assert.NotNull(captureEvent);

        Assert.NotNull(captureEvent);

        // Проверяем состояние после взятия
        GameStateResponse stateAfterCapture =
            await GetGameState(created.Id);

        Assert.Equal(
            31,
            stateAfterCapture.Pieces.Count);

        Assert.Equal(
            "Black",
            stateAfterCapture.CurrentTurn);

        // Act — Undo
        HttpResponseMessage undoResponse =
            await _client.PostAsync(
                $"/api/games/{created.Id}/undo",
                null);

        // Assert
        Assert.Equal(
            HttpStatusCode.OK,
            undoResponse.StatusCode);

        GameStateResponse stateAfterUndo =
            await GetGameState(created.Id);

        // Все фигуры восстановлены
        Assert.Equal(
            32,
            stateAfterUndo.Pieces.Count);

        // После Undo снова ход белых
        Assert.Equal(
            "White",
            stateAfterUndo.CurrentTurn);

        // Белая пешка вернулась на E4
        PieceDto? whitePawn =
            stateAfterUndo.Pieces.FirstOrDefault(
                p => p.Position == "E4");

        Assert.NotNull(whitePawn);

        // Чёрная пешка вернулась на D5
        PieceDto? blackPawn =
            stateAfterUndo.Pieces.FirstOrDefault(
                p => p.Position == "D5");

        Assert.NotNull(blackPawn);
    }

    [Fact]
    public async Task Move_InvalidMove_Should_Return_BadRequest()
    {
        // Arrange
        HttpResponseMessage createResponse =
            await _client.PostAsync(
                "/api/games",
                null);

        createResponse.EnsureSuccessStatusCode();

        CreateGameResponse? created =
            await createResponse.Content
                .ReadFromJsonAsync<CreateGameResponse>();

        Assert.NotNull(created);

        MoveRequest request = new()
        {
            From = "E2",
            To = "E5"
        };

        // Act
        HttpResponseMessage response =
            await _client.PostAsJsonAsync(
                $"/api/games/{created.Id}/move",
                request);

        // Assert
        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }

    [Fact]
    public async Task GetGame_NotFound_Should_Return_404()
    {
        // Arrange
        Guid id = Guid.NewGuid();

        // Act
        HttpResponseMessage response =
            await _client.GetAsync(
                $"/api/games/{id}");

        // Assert
        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);
    }

    [Fact]
    public async Task UndoGame_NotFound_Should_Return_404()
    {
        // Arrange
        Guid id = Guid.NewGuid();

        // Act
        HttpResponseMessage response =
            await _client.PostAsync(
                $"/api/games/{id}/undo",
                null);

        // Assert
        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);
    }

    [Fact]
    public async Task DeleteGame_Should_Remove_Game()
    {
        // Arrange
        HttpResponseMessage createResponse =
            await _client.PostAsync(
                "/api/games",
                null);

        createResponse.EnsureSuccessStatusCode();

        CreateGameResponse? created =
            await createResponse.Content
                .ReadFromJsonAsync<CreateGameResponse>();

        Assert.NotNull(created);

        // Act
        HttpResponseMessage deleteResponse =
            await _client.DeleteAsync(
                $"/api/games/{created.Id}");

        // Assert
        Assert.Equal(
            HttpStatusCode.NoContent,
            deleteResponse.StatusCode);

        // Игра после удаления должна быть недоступна
        HttpResponseMessage getResponse =
            await _client.GetAsync(
                $"/api/games/{created.Id}");

        Assert.Equal(
            HttpStatusCode.NotFound,
            getResponse.StatusCode);
    }

    [Fact]
    public async Task GetLegalMoves_Should_Return_Pawn_Moves()
    {
        // Arrange
        HttpResponseMessage createResponse =
            await _client.PostAsync(
                "/api/games",
                null);

        createResponse.EnsureSuccessStatusCode();

        CreateGameResponse? created =
            await createResponse.Content
                .ReadFromJsonAsync<CreateGameResponse>();

        Assert.NotNull(created);

        // Act
        HttpResponseMessage response =
            await _client.GetAsync(
                $"/api/games/{created.Id}/legalmoves/E2");

        // Assert
        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        List<string>? moves =
            await response.Content
                .ReadFromJsonAsync<List<string>>();

        Assert.NotNull(moves);

        Assert.Contains("E3", moves);
        Assert.Contains("E4", moves);
    }

    [Fact]
    public async Task GetLegalMoves_Should_Return_Moves_For_Moved_Pawn()
    {
        // Arrange
        HttpResponseMessage createResponse =
            await _client.PostAsync(
                "/api/games",
                null);

        createResponse.EnsureSuccessStatusCode();

        CreateGameResponse? created =
            await createResponse.Content
                .ReadFromJsonAsync<CreateGameResponse>();

        Assert.NotNull(created);

        // E2 → E4
        HttpResponseMessage moveResponse =
            await _client.PostAsJsonAsync(
                $"/api/games/{created.Id}/move",
                new MoveRequest
                {
                    From = "E2",
                    To = "E4"
                });

        moveResponse.EnsureSuccessStatusCode();

        // Act
        HttpResponseMessage legalMovesResponse =
            await _client.GetAsync(
                $"/api/games/{created.Id}/legalmoves/E4");

        // Assert
        Assert.Equal(
            HttpStatusCode.OK,
            legalMovesResponse.StatusCode);

        List<string>? moves =
            await legalMovesResponse.Content
                .ReadFromJsonAsync<List<string>>();

        Assert.NotNull(moves);

        Assert.Contains("E5", moves);
    }

    [Fact]
    public async Task Move_And_Undo_Should_Restore_Exact_Game_State()
    {
        // Arrange
        HttpResponseMessage createResponse =
            await _client.PostAsync(
                "/api/games",
                null);

        createResponse.EnsureSuccessStatusCode();

        CreateGameResponse? created =
            await createResponse.Content
                .ReadFromJsonAsync<CreateGameResponse>();

        Assert.NotNull(created);

        GameStateResponse initialState =
            await GetGameState(created.Id);

        Assert.Equal(
            "White",
            initialState.CurrentTurn);

        Assert.Equal(
            32,
            initialState.Pieces.Count);

        // Act — Move
        HttpResponseMessage moveResponse =
            await _client.PostAsJsonAsync(
                $"/api/games/{created.Id}/move",
                new MoveRequest
                {
                    From = "E2",
                    To = "E4"
                });

        moveResponse.EnsureSuccessStatusCode();

        MoveResponse? moveResult =
            await moveResponse.Content
                .ReadFromJsonAsync<MoveResponse>();

        Assert.NotNull(moveResult);
        Assert.True(moveResult.Success);

        // Проверяем состояние после Move
        GameStateResponse afterMove =
            await GetGameState(created.Id);

        Assert.Equal(
            "Black",
            afterMove.CurrentTurn);

        Assert.Equal(
            32,
            afterMove.Pieces.Count);

        Assert.Contains(
            afterMove.Pieces,
            p => p.Position == "E4");

        Assert.DoesNotContain(
            afterMove.Pieces,
            p => p.Position == "E2");

        // Act — Undo
        HttpResponseMessage undoResponse =
            await _client.PostAsync(
                $"/api/games/{created.Id}/undo",
                null);

        Assert.Equal(
            HttpStatusCode.OK,
            undoResponse.StatusCode);

        // Assert — состояние восстановлено
        GameStateResponse afterUndo =
            await GetGameState(created.Id);

        Assert.Equal(
            "White",
            afterUndo.CurrentTurn);

        Assert.Equal(
            32,
            afterUndo.Pieces.Count);

        Assert.Contains(
            afterUndo.Pieces,
            p => p.Position == "E2");

        Assert.DoesNotContain(
            afterUndo.Pieces,
            p => p.Position == "E4");
    }

    [Fact]
    public async Task EnPassant_Should_Return_Event_And_Remove_Captured_Pawn()
    {
        // Arrange
        HttpResponseMessage createResponse =
            await _client.PostAsync(
                "/api/games",
                null);

        createResponse.EnsureSuccessStatusCode();

        CreateGameResponse? created =
            await createResponse.Content
                .ReadFromJsonAsync<CreateGameResponse>();

        Assert.NotNull(created);

        // White: E2 -> E4
        await MakeMove(
            created.Id,
            "E2",
            "E4");

        // Black: A7 -> A6
        await MakeMove(
            created.Id,
            "A7",
            "A6");

        // White: E4 -> E5
        await MakeMove(
            created.Id,
            "E4",
            "E5");

        // Black: D7 -> D5
        await MakeMove(
            created.Id,
            "D7",
            "D5");

        // Act
        HttpResponseMessage enPassantResponse =
            await _client.PostAsJsonAsync(
                $"/api/games/{created.Id}/move",
                new MoveRequest
                {
                    From = "E5",
                    To = "D6"
                });

        // Assert
        Assert.Equal(
            HttpStatusCode.OK,
            enPassantResponse.StatusCode);

        MoveResponse? result =
            await enPassantResponse.Content
                .ReadFromJsonAsync<MoveResponse>();

        Assert.NotNull(result);
        Assert.True(result.Success);

        // Проверяем EnPassant event
        //GameEventDto? enPassantEvent = result.Events.FirstOrDefault(e => e.Type == GameEventType.EnPassant);
        GameEventDto? enPassantEvent = result.Events.FirstOrDefault( e => e.TypeEnum == GameEventType.EnPassant);

        Assert.NotNull(enPassantEvent);

        // Получаем состояние игры
        GameStateResponse state =
            await GetGameState(created.Id);

        // После хода White -> Black
        Assert.Equal(
            "Black",
            state.CurrentTurn);

        // Одна пешка была взята
        Assert.Equal(
            31,
            state.Pieces.Count);

        // Белая пешка должна оказаться на D6
        PieceDto? whitePawn =
            state.Pieces.FirstOrDefault(
                p => p.Position == "D6");

        Assert.NotNull(whitePawn);
        Assert.Equal(
            "White",
            whitePawn.Color);

        // Черной пешки на D5 больше нет
        PieceDto? blackPawn =
            state.Pieces.FirstOrDefault(
                p => p.Position == "D5");

        Assert.Null(blackPawn);
    }

    [Fact]
    public async Task EnPassant_And_Undo_Should_Restore_Captured_Pawn()
    {
        // Arrange
        HttpResponseMessage createResponse =
            await _client.PostAsync(
                "/api/games",
                null);

        createResponse.EnsureSuccessStatusCode();

        CreateGameResponse? created =
            await createResponse.Content
                .ReadFromJsonAsync<CreateGameResponse>();

        Assert.NotNull(created);

        // White: E2 -> E4
        await MakeMove(
            created.Id,
            "E2",
            "E4");

        // Black: A7 -> A6
        await MakeMove(
            created.Id,
            "A7",
            "A6");

        // White: E4 -> E5
        await MakeMove(
            created.Id,
            "E4",
            "E5");

        // Black: D7 -> D5
        await MakeMove(
            created.Id,
            "D7",
            "D5");

        // White: E5 -> D6 en passant
        HttpResponseMessage enPassantResponse =
            await _client.PostAsJsonAsync(
                $"/api/games/{created.Id}/move",
                new MoveRequest
                {
                    From = "E5",
                    To = "D6"
                });

        enPassantResponse.EnsureSuccessStatusCode();

        MoveResponse? result =
            await enPassantResponse.Content
                .ReadFromJsonAsync<MoveResponse>();

        Assert.NotNull(result);
        Assert.True(result.Success);

        Assert.NotNull(
            result.Events.FirstOrDefault(
                e => e.TypeEnum == GameEventType.EnPassant));

        // Проверяем состояние после En Passant
        GameStateResponse afterEnPassant =
            await GetGameState(created.Id);

        Assert.Equal(
            31,
            afterEnPassant.Pieces.Count);

        Assert.Contains(
            afterEnPassant.Pieces,
            p => p.Position == "D6");

        Assert.DoesNotContain(
            afterEnPassant.Pieces,
            p => p.Position == "D5");

        Assert.Equal(
            "Black",
            afterEnPassant.CurrentTurn);

        // Act — Undo
        HttpResponseMessage undoResponse =
            await _client.PostAsync(
                $"/api/games/{created.Id}/undo",
                null);

        Assert.Equal(
            HttpStatusCode.OK,
            undoResponse.StatusCode);

        // Assert — состояние должно полностью восстановиться
        GameStateResponse afterUndo =
            await GetGameState(created.Id);

        Assert.Equal(
            32,
            afterUndo.Pieces.Count);

        Assert.Equal(
            "White",
            afterUndo.CurrentTurn);

        // Белая пешка вернулась на E5
        Assert.Contains(
            afterUndo.Pieces,
            p => p.Position == "E5");

        // Черная пешка вернулась на D5
        Assert.Contains(
            afterUndo.Pieces,
            p => p.Position == "D5");

        // D6 снова свободна
        Assert.DoesNotContain(
            afterUndo.Pieces,
            p => p.Position == "D6");
    }

    [Fact]
    public async Task Castle_And_Undo_Should_Move_And_Restore_King_And_Rook()
    {
        // Arrange
        HttpResponseMessage createResponse =
            await _client.PostAsync(
                "/api/games",
                null);

        createResponse.EnsureSuccessStatusCode();

        CreateGameResponse? created =
            await createResponse.Content
                .ReadFromJsonAsync<CreateGameResponse>();

        Assert.NotNull(created);

        // White: E2 -> E4
        await MakeMove(
            created.Id,
            "E2",
            "E4");

        // Black: E7 -> E5
        await MakeMove(
            created.Id,
            "E7",
            "E5");

        // White: G1 -> F3
        await MakeMove(
            created.Id,
            "G1",
            "F3");

        // Black: B8 -> C6
        await MakeMove(
            created.Id,
            "B8",
            "C6");

        // White: F1 -> E2
        await MakeMove(
            created.Id,
            "F1",
            "E2");

        // Black: G8 -> F6
        await MakeMove(
            created.Id,
            "G8",
            "F6");

        // Act — White castles kingside
        HttpResponseMessage castleResponse =
            await _client.PostAsJsonAsync(
                $"/api/games/{created.Id}/move",
                new MoveRequest
                {
                    From = "E1",
                    To = "G1"
                });

        // Assert
        Assert.Equal(
            HttpStatusCode.OK,
            castleResponse.StatusCode);

        MoveResponse? result =
            await castleResponse.Content
                .ReadFromJsonAsync<MoveResponse>();

        Assert.NotNull(result);
        Assert.True(result.Success);

        // Проверяем Castle event
        GameEventDto? castleEvent =
            result.Events.FirstOrDefault(
                e => e.TypeEnum == GameEventType.Castle);

        Assert.NotNull(castleEvent);

        // Проверяем состояние после рокировки
        GameStateResponse afterCastle =
            await GetGameState(created.Id);

        Assert.Equal(
            "Black",
            afterCastle.CurrentTurn);

        // Король на G1
        Assert.Contains(
            afterCastle.Pieces,
            p => p.Position == "G1" &&
                 p.Color == "White");

        // Ладья на F1
        Assert.Contains(
            afterCastle.Pieces,
            p => p.Position == "F1" &&
                 p.Color == "White");

        // E1 и H1 должны быть свободны
        Assert.DoesNotContain(
            afterCastle.Pieces,
            p => p.Position == "E1");

        Assert.DoesNotContain(
            afterCastle.Pieces,
            p => p.Position == "H1");

        // Act — Undo
        HttpResponseMessage undoResponse =
            await _client.PostAsync(
                $"/api/games/{created.Id}/undo",
                null);

        Assert.Equal(
            HttpStatusCode.OK,
            undoResponse.StatusCode);

        // Assert — рокировка отменена
        GameStateResponse afterUndo =
            await GetGameState(created.Id);

        Assert.Equal(
            "White",
            afterUndo.CurrentTurn);

        // Король вернулся на E1
        Assert.Contains(
            afterUndo.Pieces,
            p => p.Position == "E1" &&
                 p.Color == "White");

        // Ладья вернулась на H1
        Assert.Contains(
            afterUndo.Pieces,
            p => p.Position == "H1" &&
                 p.Color == "White");

        // G1 и F1 должны быть свободны
        Assert.DoesNotContain(
            afterUndo.Pieces,
            p => p.Position == "G1");

        Assert.DoesNotContain(
            afterUndo.Pieces,
            p => p.Position == "F1");
    }

    [Fact]
    public async Task Move_InvalidMove_Should_Return_MoveResponse_Error()
    {
        // Arrange
        HttpResponseMessage createResponse =
            await _client.PostAsync(
                "/api/games",
                null);

        createResponse.EnsureSuccessStatusCode();

        CreateGameResponse? created =
            await createResponse.Content
                .ReadFromJsonAsync<CreateGameResponse>();

        Assert.NotNull(created);

        MoveRequest request = new()
        {
            From = "E2",
            To = "E5"
        };

        // Act
        HttpResponseMessage response =
            await _client.PostAsJsonAsync(
                $"/api/games/{created.Id}/move",
                request);

        // Assert
        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);

        Assert.Equal(
            "application/json",
            response.Content.Headers.ContentType?.MediaType);

        MoveResponse? result =
            await response.Content
                .ReadFromJsonAsync<MoveResponse>();

        Assert.NotNull(result);

        Assert.False(result.Success);

        Assert.NotNull(result.Error);

        Assert.Equal(
            "Недопустимый ход E2 → E5.",
            result.Error);

        Assert.Empty(result.Events);

        Assert.False(result.Check);
        Assert.False(result.Checkmate);
        Assert.False(result.Stalemate);
    }

    [Fact]
    public async Task InvalidMove_Should_Not_Change_Game_State()
    {
        // Arrange
        HttpResponseMessage createResponse =
            await _client.PostAsync(
                "/api/games",
                null);

        createResponse.EnsureSuccessStatusCode();

        CreateGameResponse? created =
            await createResponse.Content
                .ReadFromJsonAsync<CreateGameResponse>();

        Assert.NotNull(created);

        GameStateResponse before =
            await GetGameState(created.Id);

        Assert.Equal(
            "White",
            before.CurrentTurn);

        Assert.Equal(
            32,
            before.Pieces.Count);

        Assert.Contains(
            before.Pieces,
            p => p.Position == "E2" &&
                 p.Color == "White");

        // Act
        HttpResponseMessage response =
            await _client.PostAsJsonAsync(
                $"/api/games/{created.Id}/move",
                new MoveRequest
                {
                    From = "E2",
                    To = "E5"
                });

        // Assert — ход отклонён
        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);

        MoveResponse? result =
            await response.Content
                .ReadFromJsonAsync<MoveResponse>();

        Assert.NotNull(result);
        Assert.False(result.Success);

        // Проверяем состояние после ошибки
        GameStateResponse after =
            await GetGameState(created.Id);

        Assert.Equal(
            "White",
            after.CurrentTurn);

        Assert.Equal(
            32,
            after.Pieces.Count);

        // Пешка всё ещё на E2
        Assert.Contains(
            after.Pieces,
            p => p.Position == "E2" &&
                 p.Color == "White");

        // E5 должна быть пустой
        Assert.DoesNotContain(
            after.Pieces,
            p => p.Position == "E5");
    }

    [Fact]
    public async Task Undo_Without_Moves_Should_Leave_Game_State_Unchanged()
    {
        // Arrange
        HttpResponseMessage createResponse =
            await _client.PostAsync(
                "/api/games",
                null);

        createResponse.EnsureSuccessStatusCode();

        CreateGameResponse? created =
            await createResponse.Content
                .ReadFromJsonAsync<CreateGameResponse>();

        Assert.NotNull(created);

        GameStateResponse before =
            await GetGameState(created.Id);

        Assert.Equal(
            "White",
            before.CurrentTurn);

        Assert.Equal(
            32,
            before.Pieces.Count);

        // Act
        HttpResponseMessage undoResponse =
            await _client.PostAsync(
                $"/api/games/{created.Id}/undo",
                null);

        // Assert
        Assert.Equal(
            HttpStatusCode.OK,
            undoResponse.StatusCode);

        // Проверяем состояние после Undo
        GameStateResponse after =
            await GetGameState(created.Id);

        Assert.Equal(
            before.CurrentTurn,
            after.CurrentTurn);

        Assert.Equal(
            before.Pieces.Count,
            after.Pieces.Count);

        // Начальная позиция должна остаться прежней
        Assert.Contains(
            after.Pieces,
            p => p.Position == "E2" &&
                 p.Color == "White");

        Assert.Contains(
            after.Pieces,
            p => p.Position == "E7" &&
                 p.Color == "Black");
    }

    [Fact]
    public async Task Two_Moves_And_Two_Undos_Should_Restore_Initial_State()
    {
        // Arrange
        HttpResponseMessage createResponse =
            await _client.PostAsync(
                "/api/games",
                null);

        createResponse.EnsureSuccessStatusCode();

        CreateGameResponse? created =
            await createResponse.Content
                .ReadFromJsonAsync<CreateGameResponse>();

        Assert.NotNull(created);

        // White: E2 -> E4
        await MakeMove(
            created.Id,
            "E2",
            "E4");

        // Black: E7 -> E5
        await MakeMove(
            created.Id,
            "E7",
            "E5");

        GameStateResponse afterTwoMoves =
            await GetGameState(created.Id);

        Assert.Equal(
            "White",
            afterTwoMoves.CurrentTurn);

        Assert.Contains(
            afterTwoMoves.Pieces,
            p => p.Position == "E4" &&
                 p.Color == "White");

        Assert.Contains(
            afterTwoMoves.Pieces,
            p => p.Position == "E5" &&
                 p.Color == "Black");

        // Act — Undo Black move
        HttpResponseMessage firstUndo =
            await _client.PostAsync(
                $"/api/games/{created.Id}/undo",
                null);

        Assert.Equal(
            HttpStatusCode.OK,
            firstUndo.StatusCode);

        // Assert
        GameStateResponse afterFirstUndo =
            await GetGameState(created.Id);

        Assert.Equal(
            "Black",
            afterFirstUndo.CurrentTurn);

        Assert.Contains(
            afterFirstUndo.Pieces,
            p => p.Position == "E4" &&
                 p.Color == "White");

        Assert.Contains(
            afterFirstUndo.Pieces,
            p => p.Position == "E7" &&
                 p.Color == "Black");

        Assert.DoesNotContain(
            afterFirstUndo.Pieces,
            p => p.Position == "E5");

        // Act — Undo White move
        HttpResponseMessage secondUndo =
            await _client.PostAsync(
                $"/api/games/{created.Id}/undo",
                null);

        Assert.Equal(
            HttpStatusCode.OK,
            secondUndo.StatusCode);

        // Assert — initial state restored
        GameStateResponse afterSecondUndo =
            await GetGameState(created.Id);

        Assert.Equal(
            "White",
            afterSecondUndo.CurrentTurn);

        Assert.Equal(
            32,
            afterSecondUndo.Pieces.Count);

        Assert.Contains(
            afterSecondUndo.Pieces,
            p => p.Position == "E2" &&
                 p.Color == "White");

        Assert.Contains(
            afterSecondUndo.Pieces,
            p => p.Position == "E7" &&
                 p.Color == "Black");

        Assert.DoesNotContain(
            afterSecondUndo.Pieces,
            p => p.Position == "E4");

        Assert.DoesNotContain(
            afterSecondUndo.Pieces,
            p => p.Position == "E5");
    }

    [Fact]
    public async Task Undo_Then_New_Move_Should_Create_New_Game_History_Branch()
    {
        // Arrange
        HttpResponseMessage createResponse =
            await _client.PostAsync(
                "/api/games",
                null);

        createResponse.EnsureSuccessStatusCode();

        CreateGameResponse? created =
            await createResponse.Content
                .ReadFromJsonAsync<CreateGameResponse>();

        Assert.NotNull(created);

        // White: E2 -> E4
        await MakeMove(
            created.Id,
            "E2",
            "E4");

        // Black: E7 -> E5
        await MakeMove(
            created.Id,
            "E7",
            "E5");

        // Act — Undo Black move
        HttpResponseMessage undoResponse =
            await _client.PostAsync(
                $"/api/games/{created.Id}/undo",
                null);

        Assert.Equal(
            HttpStatusCode.OK,
            undoResponse.StatusCode);

        // Black теперь снова должен ходить
        GameStateResponse afterUndo =
            await GetGameState(created.Id);

        Assert.Equal(
            "Black",
            afterUndo.CurrentTurn);

        Assert.Contains(
            afterUndo.Pieces,
            p => p.Position == "E4" &&
                 p.Color == "White");

        Assert.Contains(
            afterUndo.Pieces,
            p => p.Position == "E7" &&
                 p.Color == "Black");

        // Новый ход вместо E7-E5
        await MakeMove(
            created.Id,
            "C7",
            "C5");

        // Assert
        GameStateResponse afterNewMove =
            await GetGameState(created.Id);

        Assert.Equal(
            "White",
            afterNewMove.CurrentTurn);

        Assert.Equal(
            32,
            afterNewMove.Pieces.Count);

        // Старый ход E7-E5 не должен существовать
        Assert.Contains(
            afterNewMove.Pieces,
            p => p.Position == "E7" &&
                 p.Color == "Black");

        Assert.DoesNotContain(
            afterNewMove.Pieces,
            p => p.Position == "E5");

        // Новый ход C7-C5 должен существовать
        Assert.Contains(
            afterNewMove.Pieces,
            p => p.Position == "C5" &&
                 p.Color == "Black");

        // Белая пешка всё ещё на E4
        Assert.Contains(
            afterNewMove.Pieces,
            p => p.Position == "E4" &&
                 p.Color == "White");
    }

    [Fact]
    public async Task Move_Should_Return_Correct_MoveResponse()
    {
        // Arrange
        HttpResponseMessage createResponse =
            await _client.PostAsync(
                "/api/games",
                null);

        createResponse.EnsureSuccessStatusCode();

        CreateGameResponse? created =
            await createResponse.Content
                .ReadFromJsonAsync<CreateGameResponse>();

        Assert.NotNull(created);

        // Act
        HttpResponseMessage moveResponse =
            await _client.PostAsJsonAsync(
                $"/api/games/{created.Id}/move",
                new MoveRequest
                {
                    From = "E2",
                    To = "E4"
                });

        // Assert
        Assert.Equal(
            HttpStatusCode.OK,
            moveResponse.StatusCode);

        MoveResponse? result =
            await moveResponse.Content
                .ReadFromJsonAsync<MoveResponse>();

        Assert.NotNull(result);

        Assert.True(result.Success);

        Assert.Null(result.Error);

        Assert.False(result.Check);

        Assert.False(result.Checkmate);

        Assert.False(result.Stalemate);

        Assert.Empty(result.Events);
    }

    [Fact]
    public async Task ScholarMate_Should_Return_Checkmate()
    {
        // Arrange
        HttpResponseMessage createResponse =
            await _client.PostAsync(
                "/api/games",
                null);

        createResponse.EnsureSuccessStatusCode();

        CreateGameResponse? created =
            await createResponse.Content
                .ReadFromJsonAsync<CreateGameResponse>();

        Assert.NotNull(created);

        // White: E2 -> E4
        await MakeMove(
            created.Id,
            "E2",
            "E4");

        // Black: E7 -> E5
        await MakeMove(
            created.Id,
            "E7",
            "E5");

        // White: D1 -> H5
        await MakeMove(
            created.Id,
            "D1",
            "H5");

        // Black: B8 -> C6
        await MakeMove(
            created.Id,
            "B8",
            "C6");

        // White: F1 -> C4
        await MakeMove(
            created.Id,
            "F1",
            "C4");

        // Black: G8 -> F6
        await MakeMove(
            created.Id,
            "G8",
            "F6");

        // Act — Qxf7#
        HttpResponseMessage response =
            await _client.PostAsJsonAsync(
                $"/api/games/{created.Id}/move",
                new MoveRequest
                {
                    From = "H5",
                    To = "F7"
                });

        // Assert
        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        MoveResponse? result =
            await response.Content
                .ReadFromJsonAsync<MoveResponse>();

        Assert.NotNull(result);

        Assert.True(result.Success);

        Assert.False(result.Check);

        Assert.True(result.Checkmate);

        Assert.False(result.Stalemate);

        GameEventDto? checkmateEvent =
            result.Events.FirstOrDefault(
                e => e.TypeEnum == GameEventType.Checkmate);

        Assert.NotNull(checkmateEvent);

        // Проверяем состояние игры
        GameStateResponse state =
            await GetGameState(created.Id);

        Assert.Equal(
            "Black",
            state.CurrentTurn);

        // Белая ферзь должна быть на F7
        Assert.Contains(
            state.Pieces,
            p => p.Position == "F7" &&
                 p.Color == "White");
    }

    [Fact]
    public async Task Checkmate_Undo_Should_Restore_Previous_Game_State()
    {
        // Arrange
        HttpResponseMessage createResponse =
            await _client.PostAsync(
                "/api/games",
                null);

        createResponse.EnsureSuccessStatusCode();

        CreateGameResponse? created =
            await createResponse.Content
                .ReadFromJsonAsync<CreateGameResponse>();

        Assert.NotNull(created);

        // White: E2 -> E4
        await MakeMove(
            created.Id,
            "E2",
            "E4");

        // Black: E7 -> E5
        await MakeMove(
            created.Id,
            "E7",
            "E5");

        // White: D1 -> H5
        await MakeMove(
            created.Id,
            "D1",
            "H5");

        // Black: B8 -> C6
        await MakeMove(
            created.Id,
            "B8",
            "C6");

        // White: F1 -> C4
        await MakeMove(
            created.Id,
            "F1",
            "C4");

        // Black: G8 -> F6
        await MakeMove(
            created.Id,
            "G8",
            "F6");

        // Act — H5 -> F7# (мат)
        HttpResponseMessage mateResponse =
            await _client.PostAsJsonAsync(
                $"/api/games/{created.Id}/move",
                new MoveRequest
                {
                    From = "H5",
                    To = "F7"
                });

        // Assert — мат
        Assert.Equal(
            HttpStatusCode.OK,
            mateResponse.StatusCode);

        MoveResponse? mateResult =
            await mateResponse.Content
                .ReadFromJsonAsync<MoveResponse>();

        Assert.NotNull(mateResult);

        Assert.True(mateResult.Success);
        Assert.True(mateResult.Checkmate);
        Assert.False(mateResult.Check);
        Assert.False(mateResult.Stalemate);

        GameEventDto? checkmateEvent =
            mateResult.Events.FirstOrDefault(
                e => e.TypeEnum == GameEventType.Checkmate);

        Assert.NotNull(checkmateEvent);

        // Состояние после мата
        GameStateResponse beforeUndo =
            await GetGameState(created.Id);

        Assert.Equal(
            "Black",
            beforeUndo.CurrentTurn);

        // После взятия на F7 должно остаться 31 фигура
        Assert.Equal(
            31,
            beforeUndo.Pieces.Count);

        // Белая ферзь на F7
        Assert.Contains(
            beforeUndo.Pieces,
            p => p.Position == "F7" &&
                 p.Color == "White" &&
                 p.Type == "Queen");

        // Черной пешки на F7 больше нет
        Assert.DoesNotContain(
            beforeUndo.Pieces,
            p => p.Position == "F7" &&
                 p.Color == "Black" &&
                 p.Type == "Pawn");

        // Act — Undo
        HttpResponseMessage undoResponse =
            await _client.PostAsync(
                $"/api/games/{created.Id}/undo",
                null);

        Assert.Equal(
            HttpStatusCode.OK,
            undoResponse.StatusCode);

        // Assert — состояние до мата восстановлено
        GameStateResponse afterUndo =
            await GetGameState(created.Id);

        // После Undo снова ход White
        Assert.Equal(
            "White",
            afterUndo.CurrentTurn);

        // Все 32 фигуры должны вернуться
        Assert.Equal(
            32,
            afterUndo.Pieces.Count);

        // Белая ферзь вернулась на H5
        Assert.Contains(
            afterUndo.Pieces,
            p => p.Position == "H5" &&
                 p.Color == "White" &&
                 p.Type == "Queen");

        // Черная пешка вернулась на F7
        Assert.Contains(
            afterUndo.Pieces,
            p => p.Position == "F7" &&
                 p.Color == "Black" &&
                 p.Type == "Pawn");

        // F7 больше не занята белой ферзью
        Assert.DoesNotContain(
            afterUndo.Pieces,
            p => p.Position == "F7" &&
                 p.Color == "White" &&
                 p.Type == "Queen");
    }

    [Fact]
    public async Task Check_Undo_Should_Restore_Previous_Game_State()
    {
        // Arrange
        HttpResponseMessage createResponse =
            await _client.PostAsync(
                "/api/games",
                null);

        createResponse.EnsureSuccessStatusCode();

        CreateGameResponse? created =
            await createResponse.Content
                .ReadFromJsonAsync<CreateGameResponse>();

        Assert.NotNull(created);

        // White: E2 -> E4
        await MakeMove(
            created.Id,
            "E2",
            "E4");

        // Black: F7 -> F6
        await MakeMove(
            created.Id,
            "F7",
            "F6");

        // Act — Qh5+
        HttpResponseMessage checkResponse =
            await _client.PostAsJsonAsync(
                $"/api/games/{created.Id}/move",
                new MoveRequest
                {
                    From = "D1",
                    To = "H5"
                });

        // Assert — ход успешен
        Assert.Equal(
            HttpStatusCode.OK,
            checkResponse.StatusCode);

        MoveResponse? checkResult =
            await checkResponse.Content
                .ReadFromJsonAsync<MoveResponse>();

        Assert.NotNull(checkResult);

        Assert.True(checkResult.Success);
        Assert.True(checkResult.Check);
        Assert.False(checkResult.Checkmate);
        Assert.False(checkResult.Stalemate);

        GameEventDto? checkEvent =
            checkResult.Events.FirstOrDefault(
                e => e.TypeEnum == GameEventType.Check);

        Assert.NotNull(checkEvent);

        // Проверяем состояние после шаха
        GameStateResponse beforeUndo =
            await GetGameState(created.Id);

        Assert.Equal(
            "Black",
            beforeUndo.CurrentTurn);

        Assert.Contains(
            beforeUndo.Pieces,
            p => p.Position == "H5" &&
                 p.Color == "White" &&
                 p.Type == "Queen");

        // Act — Undo
        HttpResponseMessage undoResponse =
            await _client.PostAsync(
                $"/api/games/{created.Id}/undo",
                null);

        Assert.Equal(
            HttpStatusCode.OK,
            undoResponse.StatusCode);

        // Assert — шах отменён
        GameStateResponse afterUndo =
            await GetGameState(created.Id);

        Assert.Equal(
            "White",
            afterUndo.CurrentTurn);

        // Ферзь вернулся на D1
        Assert.Contains(
            afterUndo.Pieces,
            p => p.Position == "D1" &&
                 p.Color == "White" &&
                 p.Type == "Queen");

        // H5 свободна
        Assert.DoesNotContain(
            afterUndo.Pieces,
            p => p.Position == "H5");
    }

    [Fact]
    public async Task Move_After_Checkmate_Should_Be_Rejected()
    {
        // Arrange
        HttpResponseMessage createResponse =
            await _client.PostAsync(
                "/api/games",
                null);

        createResponse.EnsureSuccessStatusCode();

        CreateGameResponse? created =
            await createResponse.Content
                .ReadFromJsonAsync<CreateGameResponse>();

        Assert.NotNull(created);

        // Scholar's Mate

        await MakeMove(
            created.Id,
            "E2",
            "E4");

        await MakeMove(
            created.Id,
            "E7",
            "E5");

        await MakeMove(
            created.Id,
            "D1",
            "H5");

        await MakeMove(
            created.Id,
            "B8",
            "C6");

        await MakeMove(
            created.Id,
            "F1",
            "C4");

        await MakeMove(
            created.Id,
            "G8",
            "F6");

        // H5 -> F7#
        HttpResponseMessage mateResponse =
            await _client.PostAsJsonAsync(
                $"/api/games/{created.Id}/move",
                new MoveRequest
                {
                    From = "H5",
                    To = "F7"
                });

        Assert.Equal(
            HttpStatusCode.OK,
            mateResponse.StatusCode);

        MoveResponse? mateResult =
            await mateResponse.Content
                .ReadFromJsonAsync<MoveResponse>();

        Assert.NotNull(mateResult);
        Assert.True(mateResult.Success);
        Assert.True(mateResult.Checkmate);

        // Act — пытаемся сделать ход после мата
        HttpResponseMessage nextResponse =
            await _client.PostAsJsonAsync(
                $"/api/games/{created.Id}/move",
                new MoveRequest
                {
                    From = "C7",
                    To = "C6"
                });

        // Assert
        Assert.Equal(
            HttpStatusCode.BadRequest,
            nextResponse.StatusCode);

        MoveResponse? result =
            await nextResponse.Content
                .ReadFromJsonAsync<MoveResponse>();

        Assert.NotNull(result);

        Assert.False(result.Success);
        Assert.False(result.Check);
        Assert.False(result.Checkmate);
        Assert.False(result.Stalemate);

        // Состояние партии не должно измениться
        GameStateResponse state =
            await GetGameState(created.Id);

        Assert.Equal(
            "Black",
            state.CurrentTurn);

        // Ферзь всё ещё на F7
        Assert.Contains(
            state.Pieces,
            p => p.Position == "F7" &&
                 p.Color == "White" &&
                 p.Type == "Queen");

        // Пешка C7 всё ещё на C7
        Assert.Contains(
            state.Pieces,
            p => p.Position == "C7" &&
                 p.Color == "Black" &&
                 p.Type == "Pawn");
    }

    [Fact]
    public void Promotion_Should_Create_Rook()
    {
        // Arrange
        Board board = new();
        Game game = new(board);

        board.AddPiece(
            new King(
                PieceColor.White,
                Position.Parse("E1")));

        board.AddPiece(
            new King(
                PieceColor.Black,
                Position.Parse("E8")));

        board.AddPiece(
            new Pawn(
                PieceColor.White,
                Position.Parse("B7")));

        // Act
        MoveResult result =
            game.Move(
                new Move(
                    Position.Parse("B7"),
                    Position.Parse("B8"),
                    PromotionPiece.Rook));

        // Assert
        Assert.True(result.Success);

        ChessPiece? promotedPiece =
            board.GetPiece(
                Position.Parse("B8"));

        Assert.NotNull(promotedPiece);
        Assert.IsType<Rook>(promotedPiece);
        Assert.Equal(
            PieceColor.White,
            promotedPiece.Color);

        Assert.Null(
            board.GetPiece(
                Position.Parse("B7")));

        Assert.Contains(
            result.Events,
            e => e.Type == GameEventType.Promotion);
    }

    [Fact]
    public void Promotion_Should_Create_Bishop()
    {
        // Arrange
        Board board = new();
        Game game = new(board);

        board.AddPiece(
            new King(
                PieceColor.White,
                Position.Parse("E1")));

        board.AddPiece(
            new King(
                PieceColor.Black,
                Position.Parse("E8")));

        board.AddPiece(
            new Pawn(
                PieceColor.White,
                Position.Parse("B7")));

        // Act
        MoveResult result =
            game.Move(
                new Move(
                    Position.Parse("B7"),
                    Position.Parse("B8"),
                    PromotionPiece.Bishop));

        // Assert
        Assert.True(result.Success);

        ChessPiece? promotedPiece =
            board.GetPiece(
                Position.Parse("B8"));

        Assert.NotNull(promotedPiece);
        Assert.IsType<Bishop>(promotedPiece);
        Assert.Equal(
            PieceColor.White,
            promotedPiece.Color);

        Assert.Null(
            board.GetPiece(
                Position.Parse("B7")));

        Assert.Contains(
            result.Events,
            e => e.Type == GameEventType.Promotion);
    }

    [Fact]
    public void Promotion_Should_Create_Knight()
    {
        // Arrange
        Board board = new();
        Game game = new(board);

        board.AddPiece(
            new King(
                PieceColor.White,
                Position.Parse("E1")));

        board.AddPiece(
            new King(
                PieceColor.Black,
                Position.Parse("E8")));

        board.AddPiece(
            new Pawn(
                PieceColor.White,
                Position.Parse("B7")));

        // Act
        MoveResult result =
            game.Move(
                new Move(
                    Position.Parse("B7"),
                    Position.Parse("B8"),
                    PromotionPiece.Knight));

        // Assert
        Assert.True(result.Success);

        ChessPiece? promotedPiece =
            board.GetPiece(
                Position.Parse("B8"));

        Assert.NotNull(promotedPiece);
        Assert.IsType<Knight>(promotedPiece);
        Assert.Equal(
            PieceColor.White,
            promotedPiece.Color);

        Assert.Null(
            board.GetPiece(
                Position.Parse("B7")));

        Assert.Contains(
            result.Events,
            e => e.Type == GameEventType.Promotion);
    }

    [Fact]
    public async Task Move_Invalid_Promotion_Position_Should_Return_BadRequest()
    {
        // Arrange
        HttpResponseMessage createResponse =
            await _client.PostAsync(
                "/api/games",
                null);

        createResponse.EnsureSuccessStatusCode();

        CreateGameResponse? created =
            await createResponse.Content
                .ReadFromJsonAsync<CreateGameResponse>();

        Assert.NotNull(created);

        MoveRequest request = new()
        {
            From = "B7",
            To = "B8",
            Promotion = PromotionPiece.Rook
        };

        // Act
        HttpResponseMessage response =
            await _client.PostAsJsonAsync(
                $"/api/games/{created.Id}/move",
                request);

        // Assert
        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);

        string json =
            await response.Content.ReadAsStringAsync();

        Console.WriteLine(json);
    }
}