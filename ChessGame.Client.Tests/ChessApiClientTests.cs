using ChessGame.Contracts.Dto;
using ChessGame.Core.Events;
using System.Net;
using System.Net.Http.Json;

namespace ChessGame.Client.Tests;
public class ChessApiClientTests
{
    [Fact]
    public async Task CreateGameAsync_Should_Return_GameId()
    {
        // Arrange
        Guid gameId =
            Guid.Parse("9035df0e-84db-43f3-a2fc-d52daf8ae1ec");

        var handler = new TestHttpMessageHandler(
    async request =>
    {
        Assert.Equal(
            HttpMethod.Post,
            request.Method);

        Assert.Equal(
            "http://localhost/api/games",
            request.RequestUri!.ToString());

        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(
                new CreateGameResponse
                {
                    Id = gameId
                })
        };
    });

        using HttpClient httpClient = new(handler)
        {
            BaseAddress =
                new Uri("http://localhost/")
        };

        ChessApiClient client =
            new(httpClient);

        // Act
        Guid result =
            await client.CreateGameAsync();

        // Assert
        Assert.Equal(gameId, result);
    }

    internal sealed class TestHttpMessageHandler
    : HttpMessageHandler
    {
        private readonly Func<
            HttpRequestMessage,
            Task<HttpResponseMessage>> _handler;

        public TestHttpMessageHandler(
            Func<HttpRequestMessage, Task<HttpResponseMessage>> handler)
        {
            _handler = handler;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            return _handler(request);
        }
    }

    [Fact]
    public async Task MoveAsync_Should_Return_MoveResult()
    {
        // Arrange
        Guid gameId =
            Guid.Parse("9035df0e-84db-43f3-a2fc-d52daf8ae1ec");

        var handler = new TestHttpMessageHandler(
            async request =>
            {
                Assert.Equal(
                    HttpMethod.Post,
                    request.Method);

                Assert.Equal(
                    $"http://localhost/api/games/{gameId}/move",
                    request.RequestUri!.ToString());

                MoveRequest? move =
                    await request.Content!
                        .ReadFromJsonAsync<MoveRequest>();

                Assert.NotNull(move);
                Assert.Equal("E2", move.From);
                Assert.Equal("E4", move.To);

                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = JsonContent.Create(
                        new MoveResponse
                        {
                            Success = true
                        })
                };
            });

        using HttpClient httpClient = new(handler)
        {
            BaseAddress =
                new Uri("http://localhost/")
        };

        ChessApiClient client =
            new(httpClient);

        // Act
        MoveResponse result =
            await client.MoveAsync(
                gameId,
                "E2",
                "E4");

        // Assert
        Assert.True(result.Success);
        Assert.Empty(result.Events);
        Assert.False(result.Check);
        Assert.False(result.Checkmate);
        Assert.False(result.Stalemate);
    }

    [Fact]
    public async Task MoveAsync_Should_Return_Check_Event()
    {
        // Arrange
        Guid gameId =
            Guid.Parse("9035df0e-84db-43f3-a2fc-d52daf8ae1ec");

        var handler = new TestHttpMessageHandler(
            request =>
            {
                Assert.Equal(
                    HttpMethod.Post,
                    request.Method);

                Assert.Equal(
                    $"http://localhost/api/games/{gameId}/move",
                    request.RequestUri!.ToString());

                MoveResponse response = new()
                {
                    Success = true,
                    Check = true,
                    Checkmate = false,
                    Stalemate = false,
                    Events =
                    [
                        new GameEventDto{
                        Type = "Check",
                        Message = "Шах Black."
                        }
                    ]
                };

                return Task.FromResult(
                    new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = JsonContent.Create(response)
                    });
            });

        using HttpClient httpClient = new(handler)
        {
            BaseAddress =
                new Uri("http://localhost/")
        };

        ChessApiClient client =
            new(httpClient);

        // Act
        MoveResponse result =
            await client.MoveAsync(
                gameId,
                "D1",
                "H5");

        // Assert
        Assert.True(result.Success);
        Assert.True(result.Check);
        Assert.False(result.Checkmate);
        Assert.False(result.Stalemate);

        Assert.Single(result.Events);

        GameEventDto gameEvent = result.Events[0];

        Assert.Equal(GameEventType.Check, gameEvent.TypeEnum);

        Assert.Equal(
            "Шах Black.",
            gameEvent.Message);
    }

    [Fact]
    public async Task MoveAsync_Should_Return_Failed_MoveResult()
    {
        // Arrange
        Guid gameId =
            Guid.Parse("9035df0e-84db-43f3-a2fc-d52daf8ae1ec");

        var handler = new TestHttpMessageHandler(
            request =>
            {
                Assert.Equal(
                    HttpMethod.Post,
                    request.Method);

                Assert.Equal(
                    $"http://localhost/api/games/{gameId}/move",
                    request.RequestUri!.ToString());

                MoveResponse response = new()
                {
                    Success = false,
                    Error = "Недопустимый ход E2 → E5."
                };

                return Task.FromResult(
                    new HttpResponseMessage(HttpStatusCode.BadRequest)
                    {
                        Content = JsonContent.Create(response)
                    });
            });

        using HttpClient httpClient = new(handler)
        {
            BaseAddress =
                new Uri("http://localhost/")
        };

        ChessApiClient client =
            new(httpClient);

        // Act
        MoveResponse result =
            await client.MoveAsync(
                gameId,
                "E2",
                "E5");

        // Assert
        Assert.False(result.Success);

        Assert.Equal(
            "Недопустимый ход E2 → E5.",
            result.Error);

        Assert.Empty(result.Events);

        Assert.False(result.Check);
        Assert.False(result.Checkmate);
        Assert.False(result.Stalemate);
    }

    [Fact]
    public async Task GetGameAsync_Should_Return_Game()
    {
        // Arrange
        Guid gameId =
            Guid.Parse("9035df0e-84db-43f3-a2fc-d52daf8ae1ec");

        GameStateResponse expected = new()
        {
            Id = gameId,
            CurrentTurn = "White",
            Pieces = []
        };

        var handler = new TestHttpMessageHandler(
            request =>
            {
                Assert.Equal(
                    HttpMethod.Get,
                    request.Method);

                Assert.Equal(
                    $"http://localhost/api/games/{gameId}",
                    request.RequestUri!.ToString());

                return Task.FromResult(
                    new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = JsonContent.Create(expected)
                    });
            });

        using HttpClient httpClient = new(handler)
        {
            BaseAddress = new Uri("http://localhost/")
        };

        ChessApiClient client = new(httpClient);

        // Act
        GameStateResponse result =
            await client.GetGameAsync(gameId);

        // Assert
        Assert.Equal(gameId, result.Id);
        Assert.Equal("White", result.CurrentTurn);
        Assert.Empty(result.Pieces);
    }

    [Fact]
    public async Task GetGameAsync_Should_Throw_ApiException_When_GameNotFound()
    {
        // Arrange
        Guid gameId = Guid.NewGuid();

        var handler = new TestHttpMessageHandler(
            request =>
            {
                Assert.Equal(
                    HttpMethod.Get,
                    request.Method);

                Assert.Equal(
                    $"http://localhost/api/games/{gameId}",
                    request.RequestUri!.ToString());

                return Task.FromResult(
                    new HttpResponseMessage(HttpStatusCode.NotFound)
                    {
                        Content = JsonContent.Create(
                            new
                            {
                                type = "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                                title = "Not Found",
                                status = 404
                            })
                    });
            });

        using HttpClient httpClient = new(handler)
        {
            BaseAddress = new Uri("http://localhost/")
        };

        ChessApiClient client = new(httpClient);

        // Act
        ApiException exception =
            await Assert.ThrowsAsync<ApiException>(
                () => client.GetGameAsync(gameId));

        // Assert
        Assert.Equal(404, exception.StatusCode);
        Assert.Equal("Not Found", exception.Message);
    }

    [Fact]
    public async Task UndoAsync_Should_Send_Undo_Request()
    {
        // Arrange
        Guid gameId =
            Guid.Parse("9035df0e-84db-43f3-a2fc-d52daf8ae1ec");

        var handler = new TestHttpMessageHandler(
            request =>
            {
                Assert.Equal(
                    HttpMethod.Post,
                    request.Method);

                Assert.Equal(
                    $"http://localhost/api/games/{gameId}/undo",
                    request.RequestUri!.ToString());

                return Task.FromResult(
                    new HttpResponseMessage(HttpStatusCode.OK));
            });

        using HttpClient httpClient = new(handler)
        {
            BaseAddress = new Uri("http://localhost/")
        };

        ChessApiClient client = new(httpClient);

        // Act
        await client.UndoAsync(gameId);

        // Assert
        // Проверки выше внутри handler подтверждают,
        // что запрос сформирован правильно.
    }

    [Fact]
    public async Task DeleteGameAsync_Should_Send_Delete_Request()
    {
        // Arrange
        Guid gameId =
            Guid.Parse("9035df0e-84db-43f3-a2fc-d52daf8ae1ec");

        var handler = new TestHttpMessageHandler(
            request =>
            {
                Assert.Equal(
                    HttpMethod.Delete,
                    request.Method);

                Assert.Equal(
                    $"http://localhost/api/games/{gameId}",
                    request.RequestUri!.ToString());

                return Task.FromResult(
                    new HttpResponseMessage(HttpStatusCode.NoContent));
            });

        using HttpClient httpClient = new(handler)
        {
            BaseAddress = new Uri("http://localhost/")
        };

        ChessApiClient client = new(httpClient);

        // Act
        await client.DeleteGameAsync(gameId);

        // Assert
        // Проверки URL и HTTP-метода находятся внутри handler.
    }

    [Fact]
    public async Task DeleteGameAsync_Should_Throw_ApiException_When_GameNotFound()
    {
        // Arrange
        Guid gameId = Guid.NewGuid();

        var handler = new TestHttpMessageHandler(
            request =>
            {
                Assert.Equal(
                    HttpMethod.Delete,
                    request.Method);

                Assert.Equal(
                    $"http://localhost/api/games/{gameId}",
                    request.RequestUri!.ToString());

                return Task.FromResult(
                    new HttpResponseMessage(HttpStatusCode.NotFound)
                    {
                        Content = JsonContent.Create(
                            new
                            {
                                type = "https://tools.ietf.org/html/rfc9110#section-15.5.5",
                                title = "Not Found",
                                status = 404
                            })
                    });
            });

        using HttpClient httpClient = new(handler)
        {
            BaseAddress = new Uri("http://localhost/")
        };

        ChessApiClient client = new(httpClient);

        // Act
        ApiException exception =
            await Assert.ThrowsAsync<ApiException>(
                () => client.DeleteGameAsync(gameId));

        // Assert
        Assert.Equal(404, exception.StatusCode);
        Assert.Equal("Not Found", exception.Message);
    }

    [Fact]
    public async Task UndoAsync_Should_Throw_ApiException_When_GameNotFound()
    {
        // Arrange
        Guid gameId = Guid.NewGuid();

        var handler = new TestHttpMessageHandler(
            request =>
            {
                Assert.Equal(
                    HttpMethod.Post,
                    request.Method);

                Assert.Equal(
                    $"http://localhost/api/games/{gameId}/undo",
                    request.RequestUri!.ToString());

                return Task.FromResult(
                    new HttpResponseMessage(HttpStatusCode.NotFound)
                    {
                        Content = JsonContent.Create(
                            new
                            {
                                title = "Not Found",
                                status = 404
                            })
                    });
            });

        using HttpClient httpClient = new(handler)
        {
            BaseAddress = new Uri("http://localhost/")
        };

        ChessApiClient client = new(httpClient);

        // Act
        ApiException exception =
            await Assert.ThrowsAsync<ApiException>(
                () => client.UndoAsync(gameId));

        // Assert
        Assert.Equal(404, exception.StatusCode);
        Assert.Equal("Not Found", exception.Message);
    }

    [Fact]
    public async Task GetGamesAsync_Should_Return_Games()
    {
        // Arrange
        List<GameInfoDto> expected =
        [
            new GameInfoDto
        {
            Id = Guid.Parse(
                "9035df0e-84db-43f3-a2fc-d52daf8ae1ec")
        },
        new GameInfoDto
        {
            Id = Guid.Parse(
                "aafe852f-ba67-4e3c-85b5-34ef0835455f")
        }
        ];

        var handler = new TestHttpMessageHandler(
            request =>
            {
                Assert.Equal(
                    HttpMethod.Get,
                    request.Method);

                Assert.Equal(
                    "http://localhost/api/games",
                    request.RequestUri!.ToString());

                return Task.FromResult(
                    new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = JsonContent.Create(expected)
                    });
            });

        using HttpClient httpClient = new(handler)
        {
            BaseAddress = new Uri("http://localhost/")
        };

        ChessApiClient client = new(httpClient);

        // Act
        List<GameInfoDto> result =
            await client.GetGamesAsync();

        // Assert
        Assert.Equal(2, result.Count);

        Assert.Equal(
            expected[0].Id,
            result[0].Id);

        Assert.Equal(
            expected[1].Id,
            result[1].Id);
    }
}