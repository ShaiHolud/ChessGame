using ChessGame.Core;
using ChessGame.Server.Models;

namespace ChessGame.Server.Services
{
    public sealed class GameManager
    {
        private readonly Dictionary<Guid, GameSession> _games = [];

        public GameSession CreateGame()
        {
            return CreateGame(
                InitialBoardFactory.Create());
        }

        internal GameSession CreateGame(Board board)
        {
            Game game = new(board);

            GameSession session = new(
                Guid.NewGuid(),
                game);

            _games.Add(
                session.Id,
                session);

            return session;
        }

        public GameSession? Get(Guid id)
        {
            _games.TryGetValue(id, out GameSession? session);

            return session;
        }

        public bool Remove(Guid id)
        {
            return _games.Remove(id);
        }

        public IReadOnlyCollection<GameSession> GetAll()
        {
            return _games.Values;
        }
    }
}
