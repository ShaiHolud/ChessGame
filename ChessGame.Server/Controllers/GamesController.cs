using ChessGame.Core;
using ChessGame.Core.Model;
using ChessGame.Core.Pieces;
using ChessGame.Contracts.Dto;
using ChessGame.Server.Models;
using ChessGame.Server.Services;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using ChessGame.Core.Results;
using static ChessGame.Core.Game;

namespace ChessGame.Server.Controllers
{
    [ApiController]
    [Route("api/games")]
    public class GamesController : ControllerBase
    {
        private readonly GameManager _manager;

        public GamesController(GameManager manager)
        {
            _manager = manager;
        }

        [HttpPost]
        [SwaggerOperation(
            Summary = "Создает партию",
            Description = "Создает новую шахматную партию, возвращает Id партии",
            OperationId = "Create new game",
            Tags = new[] { "Games" })]
        [SwaggerResponse(200, "Шахматная партия успешно создана")]
        public IActionResult Create()
        {
            GameSession game = _manager.CreateGame();

            return Ok(new
            {
                game.Id
            });
        }

        [SwaggerOperation(
            Summary = "Получает партию",
            Description = "Предоставляет данные по Id партии",
            OperationId = "Get game data",
            Tags = new[] { "Games" })]
        [SwaggerResponse(200, "Данные шахматной партии успешно получены")]
        [SwaggerResponse(404, "Данные шахматной партии не найдены")]
        [HttpGet("{id:guid}")]
        public IActionResult Get(Guid id)
        {
            GameSession? session = _manager.Get(id);

            if (session == null)
                return NotFound();

            Game game = session.Game;

            GameStateResponse response = new()
            {
                Id = session.Id,

                CurrentTurn = game.CurrentTurn.ToString(),

                Finished = game.IsFinished,

                State = game.State.ToString(),

                Pieces = game
                    .GetAllPieces()
                    .Select(p => new PieceDto
                    {
                        Type = p.GetType().Name,
                        Color = p.Color.ToString(),
                        Position = p.Position.ToString(),
                        MoveCount = p.MoveCount
                    })
                    .ToList()
            };

            return Ok(response);
        }

        [HttpPost("{id:guid}/move")]
        public IActionResult Move(Guid id, MoveRequest request)
        {
            GameSession? session = _manager.Get(id);

            if (session == null)
                return NotFound();

            try
            {
                MoveResult result = session.Game.Move(new Move(Position.Parse(request.From), Position.Parse(request.To), request.Promotion));

                session.LastActivity = DateTime.UtcNow;

                return Ok(new MoveResponse
                {
                    Success = result.Success,

                    Events = result.Events.Select(e => new GameEventDto
                    {
                        Type = e.Type.ToString(),
                        Message = e.Message
                    })
                    .ToList(),

                    Check = result.Check,
                    Checkmate = result.Checkmate,
                    Stalemate = result.Stalemate
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new MoveResponse
                {
                    Success = false,
                    Error = ex.Message
                });
            }
        }

        [HttpPost("{id:guid}/undo")]
        public IActionResult Undo(Guid id)
        {
            GameSession? session = _manager.Get(id);

            if (session == null)
                return NotFound();

            session.Game.Undo();
            session.LastActivity = DateTime.UtcNow;
            return Ok();
        }

        [HttpGet("{id:guid}/legalmoves/{square}")]
        public IActionResult LegalMoves(Guid id, string square)
        {
            GameSession? session = _manager.Get(id);

            if (session == null)
                return NotFound();

            try
            {
                Position from = Position.Parse(square);

                IReadOnlyCollection<Move> moves =
                    session.Game.GetLegalMoves(from);

                return Ok(
                    moves.Select(m => m.To.ToString()));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id:guid}")]
        public IActionResult Delete(Guid id)
        {
            bool removed = _manager.Remove(id);

            if (!removed)
                return NotFound();

            return NoContent();
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            List<GameInfoDto> games = _manager.GetAll().Select(session => new GameInfoDto
            {
                Id = session.Id,
                CurrentTurn = session.Game.CurrentTurn.ToString(),
                LastActivity = session.LastActivity,
                WhitePieces = session.Game.GetPieces(PieceColor.White).Count(),
                BlackPieces = session.Game.GetPieces(PieceColor.Black).Count(),
                Finished = session.Game.IsFinished,
                State = session.Game.State.ToString()
            })
            .ToList();

            return Ok(games);
        }
    }
}
