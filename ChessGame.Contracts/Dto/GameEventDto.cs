using System.Text.Json.Serialization;
using ChessGame.Core.Events;

namespace ChessGame.Contracts.Dto;

public sealed class GameEventDto
{
    public string Type { get; init; } = string.Empty;

    public string Message { get; init; } = string.Empty;

    [JsonIgnore]
    public GameEventType? TypeEnum =>
        Enum.TryParse<GameEventType>(
            Type,
            out var result)
                ? result
                : null;
}