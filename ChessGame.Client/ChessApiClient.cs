using System.Net.Http.Json;
using ChessGame.Contracts.Dto;

namespace ChessGame.Client;

public sealed class ChessApiClient
{
    private readonly HttpClient _http;

    public ChessApiClient(HttpClient http)
    {
        _http = http;
    }

    private async Task<T> ReadAsync<T>(HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
        {
            ApiProblemDetails? problem = await response.Content.ReadFromJsonAsync<ApiProblemDetails>();

            string message = problem?.Detail
                ?? problem?.Title
                ?? $"HTTP {(int)response.StatusCode}";

            throw new ApiException((int)response.StatusCode, message);
        }

        T? result = await response.Content.ReadFromJsonAsync<T>();

        if (result == null)
            throw new InvalidOperationException(
                "Сервер вернул пустой ответ.");

        return result;
    }

    private async Task EnsureSuccessAsync(
    HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
            return;

        ApiProblemDetails? problem =
            await response.Content
                .ReadFromJsonAsync<ApiProblemDetails>();

        string message =
            problem?.Detail
            ?? problem?.Title
            ?? $"HTTP {(int)response.StatusCode}";

        throw new ApiException(
            (int)response.StatusCode,
            message);
    }

    public async Task<Guid> CreateGameAsync() 
    { 
        HttpResponseMessage response = await _http.PostAsync("api/games", null);

        CreateGameResponse game = await ReadAsync<CreateGameResponse>(response);
        
        return game.Id; }

    public async Task<List<GameInfoDto>> GetGamesAsync()
    {
        HttpResponseMessage response = await _http.GetAsync("api/games");

        return await ReadAsync<List<GameInfoDto>>(response);
    }

    public async Task<GameStateResponse> GetGameAsync(Guid id)
    {
        HttpResponseMessage response = await _http.GetAsync($"api/games/{id}");

        return await ReadAsync<GameStateResponse>(response);
    }

    public async Task UndoAsync(Guid id)
    {
        HttpResponseMessage response =
            await _http.PostAsync(
                $"api/games/{id}/undo",
                null);

        await EnsureSuccessAsync(response);
    }

    public async Task<List<string>> GetLegalMovesAsync(Guid id, string square)
    {
        HttpResponseMessage response =
            await _http.GetAsync(
                $"api/games/{id}/legalmoves/{square}");

        return await ReadAsync<List<string>>(response);
    }

    public async Task<MoveResponse> MoveAsync(Guid gameId, string from, string to)
    {
        MoveRequest request = new()
        {
            From = from,
            To = to
        };

        HttpResponseMessage response = await _http.PostAsJsonAsync($"api/games/{gameId}/move", request);

        MoveResponse? result =  await response.Content.ReadFromJsonAsync<MoveResponse>();

        if (result == null)
            throw new InvalidOperationException(
                "Сервер вернул пустой ответ.");

        return result;
    }

    public async Task DeleteGameAsync(Guid id)
    {
        HttpResponseMessage response =
            await _http.DeleteAsync(
                $"api/games/{id}");

        await EnsureSuccessAsync(response);
    }
}
