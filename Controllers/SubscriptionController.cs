using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using OluBackendApp.Data;
using OluBackendApp.DTOs;
using OluBackendApp.Models;

namespace OluBackendApp.Controllers
{
    [ApiController]
    [Route("api/subscriptions")]
    [Authorize]
    public class SubscriptionController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _um;

        public SubscriptionController(
            ApplicationDbContext db,
            UserManager<ApplicationUser> um)
        {
            _db = db; _um = um;
        }

        [HttpPost]
        public async Task<IActionResult> Subscribe([FromBody] CreatePushSubscriptionDto dto)
        {
            var user = await _um.GetUserAsync(User)!;
            _db.PushSubscriptions.Add(new PushSubscription
            {
                UserId = user.Id,
                Endpoint = dto.Endpoint,
                P256DHKey = dto.P256DHKey
            });
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
