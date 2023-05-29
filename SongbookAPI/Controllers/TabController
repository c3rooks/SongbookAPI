using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SongbookAPI.Scrapers;

namespace SongbookAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TabController : ControllerBase
    {
        private readonly UltimateGuitarScraper _scraper;

        public TabController(UltimateGuitarScraper scraper)
        {
            _scraper = scraper;
        }

        [HttpGet("{url}")]
        public async Task<IActionResult> GetTab(string url)
        {
            var html = await _scraper.DownloadHtml(url);
            var tab = _scraper.ParseHtml(html);
            return Ok(tab);
        }
    }
}
