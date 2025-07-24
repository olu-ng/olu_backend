using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using OluBackendApp.DTOs;
using OluBackendApp.Models;

namespace OluBackendApp.Controllers
{
    [ApiController]
    public class PresenceController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _um;

        public PresenceController(UserManager<ApplicationUser> um)
            => _um = um;

        [HttpGet("api/users/{id}/presence")]
        public async Task<ActionResult<UserPresenceDto>> GetPresence(string id)
        {
            var u = await _um.FindByIdAsync(id);
            if (u == null) return NotFound();
            return Ok(new UserPresenceDto(
                u.Id, u.CurrentStatus, u.LastSeenAt, null
            ));
        }
    }
}
