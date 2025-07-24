using System;
using System.Collections.Generic;
using System.Linq;
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
    [Route("api/admin/officeowners")]
    [Authorize(Roles = Roles.SuperAdmin)]
    public class AdminOfficeOwnerController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _db;

        public AdminOfficeOwnerController(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext db)
        {
            _userManager = userManager;
            _db = db;
        }

        // POST api/admin/officeowners
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AdminCreateOfficeOwnerDto dto)
        {
            if (await _userManager.FindByEmailAsync(dto.Email) != null)
                return Conflict(new { Error = "Email already in use." });
            if (dto.Password != dto.ConfirmPassword)
                return BadRequest(new { Error = "Passwords do not match." });

            var user = new ApplicationUser
            {
                UserName = dto.Email,
                Email = dto.Email
            };
            var createRes = await _userManager.CreateAsync(user, dto.Password);
            if (!createRes.Succeeded)
                return BadRequest(createRes.Errors);

            await _userManager.AddToRoleAsync(user, Roles.OfficeOwner);

            var prof = new OfficeOwnerProfile
            {
                UserId = user.Id,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                ZipCode = dto.ZipCode,
                StateOfResidence = dto.StateOfResidence,
                Address = dto.Address
            };
            _db.OfficeOwnerProfiles.Add(prof);
            await _db.SaveChangesAsync();

            var resultDto = MapToDto(user, prof);
            return CreatedAtAction(nameof(GetById), new { id = user.Id }, resultDto);
        }

        // GET api/admin/officeowners
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OfficeOwnerProfileDto>>> GetAll()
        {
            var list = await _db.OfficeOwnerProfiles
                .AsNoTracking()
                .Where(p => !p.IsDeleted)
                .Include(p => p.User)
                .Select(p => MapToDto(p.User, p))
                .ToListAsync();

            return Ok(list);
        }

        // GET api/admin/officeowners/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<OfficeOwnerProfileDto>> GetById(string id)
        {
            var prof = await _db.OfficeOwnerProfiles
                .AsNoTracking()
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.UserId == id && !p.IsDeleted);

            if (prof == null)
                return NotFound();

            return Ok(MapToDto(prof.User, prof));
        }

        // PUT api/admin/officeowners/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] AdminUpdateOfficeOwnerDto dto)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var prof = await _db.OfficeOwnerProfiles
                .FirstOrDefaultAsync(p => p.UserId == id && !p.IsDeleted);
            if (prof == null) return NotFound();

            // Update email if provided
            if (!string.IsNullOrWhiteSpace(dto.Email) && dto.Email != user.Email)
            {
                if (await _userManager.FindByEmailAsync(dto.Email) != null)
                    return Conflict(new { Error = "Email already in use." });

                user.Email = dto.Email;
                user.UserName = dto.Email;
                await _userManager.UpdateAsync(user);
            }

            // Update password if provided
            if (!string.IsNullOrEmpty(dto.Password))
            {
                if (dto.Password != dto.ConfirmPassword)
                    return BadRequest(new { Error = "Passwords do not match." });

                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var rs = await _userManager.ResetPasswordAsync(user, token, dto.Password);
                if (!rs.Succeeded)
                    return BadRequest(rs.Errors);
            }

            // Update profile fields
            if (dto.FirstName != null) prof.FirstName = dto.FirstName;
            if (dto.LastName != null) prof.LastName = dto.LastName;
            if (dto.ZipCode != null) prof.ZipCode = dto.ZipCode;
            if (dto.StateOfResidence != null) prof.StateOfResidence = dto.StateOfResidence;
            if (dto.Address != null) prof.Address = dto.Address;

            await _db.SaveChangesAsync();
            return NoContent();
        }

        // PATCH api/admin/officeowners/{id}
        [HttpPatch("{id}")]
        public async Task<IActionResult> Patch(string id, [FromBody] AdminPatchOfficeOwnerDto dto)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var prof = await _db.OfficeOwnerProfiles
                .FirstOrDefaultAsync(p => p.UserId == id && !p.IsDeleted);
            if (prof == null) return NotFound();

            // Partial email update
            if (!string.IsNullOrWhiteSpace(dto.Email) && dto.Email != user.Email)
            {
                if (await _userManager.FindByEmailAsync(dto.Email) != null)
                    return Conflict(new { Error = "Email already in use." });

                user.Email = dto.Email;
                user.UserName = dto.Email;
                await _userManager.UpdateAsync(user);
            }

            // Partial password update
            if (!string.IsNullOrEmpty(dto.Password))
            {
                if (dto.Password != dto.ConfirmPassword)
                    return BadRequest(new { Error = "Passwords do not match." });

                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var rs = await _userManager.ResetPasswordAsync(user, token, dto.Password);
                if (!rs.Succeeded)
                    return BadRequest(rs.Errors);
            }

            // Partial profile update
            if (dto.FirstName != null) prof.FirstName = dto.FirstName;
            if (dto.LastName != null) prof.LastName = dto.LastName;
            if (dto.ZipCode != null) prof.ZipCode = dto.ZipCode;
            if (dto.StateOfResidence != null) prof.StateOfResidence = dto.StateOfResidence;
            if (dto.Address != null) prof.Address = dto.Address;

            await _db.SaveChangesAsync();
            return NoContent();
        }

        // DELETE api/admin/officeowners/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            var prof = await _db.OfficeOwnerProfiles
                .FirstOrDefaultAsync(p => p.UserId == id && !p.IsDeleted);

            if (user == null || prof == null)
                return NotFound();

            prof.IsDeleted = true;
            prof.DeletedAt = DateTime.UtcNow;
            user.LockoutEnabled = true;
            user.LockoutEnd = DateTimeOffset.MaxValue;

            await _db.SaveChangesAsync();
            await _userManager.UpdateAsync(user);

            return NoContent();
        }

        // Helper: maps EF entity + user -> DTO
        private static OfficeOwnerProfileDto MapToDto(
            ApplicationUser user,
            OfficeOwnerProfile profile)
            => new(
                UserId: profile.UserId,
                Email: user.Email!,
                Role: Roles.OfficeOwner,
                FirstName: profile.FirstName,
                LastName: profile.LastName,
                ZipCode: profile.ZipCode,
                StateOfResidence: profile.StateOfResidence,
                Address: profile.Address
            );
    }
}
