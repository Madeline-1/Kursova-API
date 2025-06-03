using Microsoft.AspNetCore.Mvc;
using Курсова_API.Clients;
using Курсова_API.Models;
namespace Курсова_API.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class songLyricsController : ControllerBase
    {
        private readonly ILogger<songLyricsController> _logger;
        private class FavoriteSong
        {
            public string SongId { get; set; }
            public string DisplayName { get; set; }
            public songLyrics.Rootobject Lyrics { get; set; }
        }

        private static readonly List<FavoriteSong> _favorites = new();

        public songLyricsController(ILogger<songLyricsController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        [ActionName("GetLyrics")]
        public async Task<songLyrics.Rootobject> GetLyrics(string songId)
        {
            var lc = new LyricsClient();
            return await lc.GetLyrics(songId);
        }

        [HttpGet]
        [ActionName("GetTime")]
        public async Task<ActionResult<IEnumerable<string>>> GetTime(string songId)
        {
            var lc = new LyricsClient();
            var sl = await lc.GetLyrics(songId);
            var formatted = sl.lyrics.lines.Select(line =>
                $"{line.words} - {line.startTimeMs}ms");
            return Ok(formatted);
        }

        [HttpPost]
        [ActionName("AddToFavorites")]
        public async Task<ActionResult> AddToFavorites(string songId, string displayName)
        {
            if (string.IsNullOrWhiteSpace(displayName))
                return BadRequest("DisplayName не може бути порожнім.");

            if (string.IsNullOrWhiteSpace(songId))
                return BadRequest("SongId не може бути порожнім.");

            var existing = _favorites.Any(f => f.SongId == songId);
            if (existing)
                return BadRequest("Пісня вже є в обраних.");

            var lc = new LyricsClient();
            songLyrics.Rootobject lyrics;

            try
            {
                lyrics = await lc.GetLyrics(songId);
            }
            catch (HttpRequestException ex)
            {
                return NotFound($"Не вдалося знайти лірики для ID = {songId}. Можливо, цей ID некоректний.");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Внутрішня помилка при отриманні лірик: {ex.Message}");
            }

            _favorites.Add(new FavoriteSong
            {
                SongId = songId,
                DisplayName = displayName,
                Lyrics = lyrics
            });

            return Ok(new { Index = _favorites.Count - 1 });
        }
        [HttpGet]
        [ActionName("GetFavorites")]
        public ActionResult<IEnumerable<object>> GetFavorites()
        {
            var list = _favorites
                .Select((f, idx) => new
                {
                    Index = idx,
                    SongId = f.SongId,
                    DisplayName = f.DisplayName
                })
                .ToList();
            return Ok(list);
        }

        [HttpPut]
        [ActionName("UpdateFavorite")]
        public async Task<ActionResult> UpdateFavorite(int index, string newSongId)
        {
            if (index < 0 || index >= _favorites.Count)
                return BadRequest("Index out of range.");

            var lc = new LyricsClient();
            var lyrics = await lc.GetLyrics(newSongId);
            var oldDisplayName = _favorites[index].DisplayName;

            _favorites[index] = new FavoriteSong
            {
                SongId = newSongId,
                DisplayName = oldDisplayName,
                Lyrics = lyrics
            };
            return Ok();
        }

        [HttpDelete]
        [ActionName("RemoveFavorite")]
        public ActionResult RemoveFavorite(int index)
        {
            if (index < 0 || index >= _favorites.Count)
                return BadRequest("Index out of range.");

            _favorites.RemoveAt(index);
            return Ok();
        }
    }
}