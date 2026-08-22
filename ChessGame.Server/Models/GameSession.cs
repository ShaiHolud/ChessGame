using ChessGame.Core;

namespace ChessGame.Server.Models
{
    public sealed class GameSession
    {
        public Guid Id { get; }

        public Game Game { get; }

        public GameSession(Guid id, Game game)
        {
            Id = id;
            Game = game;
            Created = DateTime.UtcNow;
            LastActivity = Created;
        }

        public DateTime Created { get; } = DateTime.UtcNow;

        public DateTime LastActivity { get; set; } = DateTime.UtcNow;
    }
}
