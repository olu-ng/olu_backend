using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OluBackendApp.Data;
using OluBackendApp.DTOs;
using OluBackendApp.Models;

namespace OluBackendApp.Controllers
{
    [ApiController]
    [Route("api/office/profile")]
    [Authorize(Roles = Roles.OfficeOwner)]
    public class OfficeOwnerProfileController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _db;

        public OfficeOwnerProfileController(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext db)
        {
            _userManager = userManager;
            _db = db;
        }

        // GET /api/office/profile
        [HttpGet]
        public async Task<ActionResult<OfficeOwnerProfileDto>> GetProfile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var prof = await _db.OfficeOwnerProfiles
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.UserId == user.Id && !p.IsDeleted);
            if (prof == null) return NotFound();

            return Ok(MapToDto(user, prof));
        }

        // PUT /api/office/profile
        [HttpPut]
        public async Task<IActionResult> ReplaceProfile([FromBody] OfficeOwnerProfileUpdateDto dto)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var prof = await _db.OfficeOwnerProfiles
                .FirstOrDefaultAsync(p => p.UserId == user.Id && !p.IsDeleted);
            if (prof == null) return NotFound();

            prof.FirstName = dto.FirstName;
            prof.LastName = dto.LastName;
            prof.ZipCode = dto.ZipCode;
            prof.StateOfResidence = dto.StateOfResidence;
            prof.Address = dto.Address;

            await _db.SaveChangesAsync();
            return NoContent();
        }

        // PATCH /api/office/profile
        [HttpPatch]
        public async Task<IActionResult> UpdateProfile([FromBody] OfficeOwnerProfilePatchDto dto)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var prof = await _db.OfficeOwnerProfiles
                .FirstOrDefaultAsync(p => p.UserId == user.Id && !p.IsDeleted);
            if (prof == null) return NotFound();

            if (dto.FirstName != null) prof.FirstName = dto.FirstName;
            if (dto.LastName != null) prof.LastName = dto.LastName;
            if (dto.ZipCode != null) prof.ZipCode = dto.ZipCode;
            if (dto.StateOfResidence != null) prof.StateOfResidence = dto.StateOfResidence;
            if (dto.Address != null) prof.Address = dto.Address;

            await _db.SaveChangesAsync();
            return NoContent();
        }

        // POST /api/office/profile/change-email
        [HttpPost("change-email")]
        public async Task<IActionResult> ChangeEmail([FromBody] ChangeEmailDto dto)
        {
            if (dto.NewEmail != dto.ConfirmEmail)
                return BadRequest(new { Error = "Email confirmation mismatch." });

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            if (!await _userManager.CheckPasswordAsync(user, dto.CurrentPassword))
                return BadRequest(new { Error = "Current password is incorrect." });

            if (await _userManager.FindByEmailAsync(dto.NewEmail) != null)
                return Conflict(new { Error = "Email already in use." });

            var token = await _userManager.GenerateChangeEmailTokenAsync(user, dto.NewEmail);
            var result = await _userManager.ChangeEmailAsync(user, dto.NewEmail, token);
            if (!result.Succeeded) return BadRequest(result.Errors);

            user.UserName = dto.NewEmail;
            await _userManager.UpdateAsync(user);

            return NoContent();
        }

        // POST /api/office/profile/change-password
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            if (dto.NewPassword != dto.ConfirmPassword)
                return BadRequest(new { Error = "Password confirmation mismatch." });

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var result = await _userManager.ChangePasswordAsync(
                user, dto.CurrentPassword, dto.NewPassword);
            if (!result.Succeeded) return BadRequest(result.Errors);

            return NoContent();
        }

        // ─── Helper ──────────────────────────────────

        private static OfficeOwnerProfileDto MapToDto(
            ApplicationUser user,
            OfficeOwnerProfile p)
        {
            return new OfficeOwnerProfileDto(
                UserId: p.UserId,
                Email: user.Email!,
                Role: Roles.OfficeOwner,
                FirstName: p.FirstName,
                LastName: p.LastName,
                ZipCode: p.ZipCode,
                StateOfResidence: p.StateOfResidence,
                Address: p.Address
            );
        }
    }
}
