using Microsoft.AspNetCore.Mvc;
using MineSweeper_MVC.Services;

namespace MineSweeper_MVC.Controllers
{
    [ApiController]
    [Route("api")]
    public class GameAPIController : ControllerBase
    {
        private readonly CoreGameServices _gameServices;

        public GameAPIController(CoreGameServices gameServices)
        {
            _gameServices = gameServices;
        }

        // GET /api/showSavedGames
        [HttpGet("showSavedGames")]
        public async Task<IActionResult> ShowSavedGames()
        {
            var games = await _gameServices.GetAllSavedGamesAsync();
            return Ok(games);
        }

        // GET /api/showSavedGames/5
        [HttpGet("showSavedGames/{id}")]
        public async Task<IActionResult> ShowOneGame(int id)
        {
            var game = await _gameServices.GetGameByIdAsync(id);

            if (game == null)
                return NotFound();

            return Ok(game);
        }

        // DELETE /api/deleteOneGame/5
        [HttpDelete("deleteOneGame/{id}")]
        public async Task<IActionResult> DeleteOneGame(int id)
        {
            bool deleted = await _gameServices.DeleteGameByIdAsync(id);

            if (!deleted)
                return NotFound();

            return Ok(new { message = "Game deleted successfully." });
        }
    }

}
