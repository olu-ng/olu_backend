using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OluBackendApp.Data;
using OluBackendApp.DTOs;

namespace OluBackendApp.Controllers
{
    [ApiController]
    [Route("api/analytics")]
    public class AnalyticsController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        public AnalyticsController(ApplicationDbContext db) => _db = db;

        [HttpGet("chats/{chatId}")]
        public async Task<ActionResult<ChatMetricsDto>> GetChatMetrics(int chatId)
        {
            var today = DateTime.UtcNow.Date;
            var msgs = await _db.Messages
                .Where(m => m.ChatId == chatId && m.SentAt.Date == today)
                .OrderBy(m => m.SentAt)
                .ToListAsync();

            if (!msgs.Any()) return NotFound();

            var total = msgs.Count;
            var diffs = msgs.Zip(msgs.Skip(1),
                (a, b) => (b.SentAt - a.SentAt).TotalSeconds).ToList();
            var avg = diffs.Any() ? diffs.Average() : 0.0;

            return new ChatMetricsDto(chatId, total, avg, today);
        }
    }
}
