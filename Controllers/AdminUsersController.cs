using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OluBackendApp.Models;
using OluBackendApp.Services;
using OluBackendApp.DTOs;

namespace OluBackendApp.Controllers
{
    [ApiController]
    [Route("api/admin/users")]
    [Authorize(Roles = Roles.SuperAdmin)]
    public class AdminUsersController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IEmailService _emailService;

        public AdminUsersController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager, IEmailService emailService
            )
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _emailService = emailService;

        }

        // GET api/admin/users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetAll()
        {
            var users = await _userManager.Users.ToListAsync();
            var dtos = new List<UserDto>(users.Count);

            foreach (var u in users)
            {
                var roles = await _userManager.GetRolesAsync(u);
                dtos.Add(new UserDto(
                    UserId: u.Id,
                    Email: u.Email!,
                    Roles: roles,
                    EmailConfirmed: u.EmailConfirmed,
                    LockoutEnabled: u.LockoutEnabled,
                    LockoutEnd: u.LockoutEnd
                ));
            }

            return Ok(dtos);
        }

        // GET api/admin/users/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<UserDto>> GetById(string id)
        {
            var u = await _userManager.FindByIdAsync(id);
            if (u == null) return NotFound();

            var roles = await _userManager.GetRolesAsync(u);
            return Ok(new UserDto(
                UserId: u.Id,
                Email: u.Email!,
                Roles: roles,
                EmailConfirmed: u.EmailConfirmed,
                LockoutEnabled: u.LockoutEnabled,
                LockoutEnd: u.LockoutEnd
            ));
        }

        // PUT api/admin/users/{id}/roles
        [HttpPut("{id}/roles")]
        public async Task<IActionResult> UpdateRoles(string id, [FromBody] UpdateUserRolesDto dto)
        {
            var u = await _userManager.FindByIdAsync(id);
            if (u == null) return NotFound();

            var validRoles = _roleManager.Roles.Select(r => r.Name!).ToHashSet();
            var invalid = dto.Roles.Except(validRoles);
            if (invalid.Any())
                return BadRequest(new { Error = $"Invalid roles: {string.Join(", ", invalid)}" });

            var current = await _userManager.GetRolesAsync(u);
            var remove = current.Except(dto.Roles).ToArray();
            var add = dto.Roles.Except(current).ToArray();

            if (remove.Length > 0)
            {
                var res = await _userManager.RemoveFromRolesAsync(u, remove);
                if (!res.Succeeded) return BadRequest(res.Errors);
            }

            if (add.Length > 0)
            {
                var res = await _userManager.AddToRolesAsync(u, add);
                if (!res.Succeeded) return BadRequest(res.Errors);
            }

            return NoContent();
        }

        // POST api/admin/users/{id}/lockout
        [HttpPost("{id}/lockout")]
        public async Task<IActionResult> SetLockout(string id, [FromBody] LockoutDto dto)
        {
            var u = await _userManager.FindByIdAsync(id);
            if (u == null) return NotFound();

            u.LockoutEnabled = true;
            u.LockoutEnd = dto.Lock
                ? (dto.End ?? DateTimeOffset.MaxValue)
                : null;  // unlock

            var res = await _userManager.UpdateAsync(u);
            if (!res.Succeeded) return BadRequest(res.Errors);
            return NoContent();
        }

        // POST api/admin/users/{id}/reset-password
        [HttpPost("{id}/reset-password")]
        public async Task<IActionResult> ResetPassword(string id, [FromBody] ResetUserPasswordDto dto)
        {
            if (dto.NewPassword != dto.ConfirmPassword)
                return BadRequest(new { Error = "Passwords do not match." });

            var u = await _userManager.FindByIdAsync(id);
            if (u == null) return NotFound();

            var token = await _userManager.GeneratePasswordResetTokenAsync(u);
            var result = await _userManager.ResetPasswordAsync(u, token, dto.NewPassword);
            if (!result.Succeeded) return BadRequest(result.Errors);

            // optional: invalidate existing tokens
            await _userManager.UpdateSecurityStampAsync(u);

            return NoContent();
        }

        // GET api/admin/users/roles
        [HttpGet("roles")]
        public async Task<ActionResult<IEnumerable<RoleDto>>> GetRoles()
        {
            var roles = await _roleManager.Roles
                .Select(r => new RoleDto(r.Name!))
                .ToListAsync();
            return Ok(roles);
        }

        // POST api/admin/users/roles
        [HttpPost("roles")]
        public async Task<IActionResult> CreateRole([FromBody] CreateRoleDto dto)
        {
            if (await _roleManager.RoleExistsAsync(dto.RoleName))
                return Conflict(new { Error = "Role already exists." });

            var res = await _roleManager.CreateAsync(new IdentityRole(dto.RoleName));
            if (!res.Succeeded) return BadRequest(res.Errors);

            return CreatedAtAction(nameof(GetRoles), null, new RoleDto(dto.RoleName));
        }

        // DELETE api/admin/users/roles/{roleName}
        [HttpDelete("roles/{roleName}")]
        public async Task<IActionResult> DeleteRole(string roleName)
        {
            if (new[] { Roles.SuperAdmin, Roles.Admin, Roles.Artisan, Roles.OfficeOwner }
                    .Contains(roleName))
                return BadRequest(new { Error = "Cannot delete core roles." });

            var role = await _roleManager.FindByNameAsync(roleName);
            if (role == null) return NotFound();

            var res = await _roleManager.DeleteAsync(role);
            if (!res.Succeeded) return BadRequest(res.Errors);

            return NoContent();
        }

        [HttpPost("{id}/resend-confirmation")]
        public async Task<IActionResult> ResendConfirmation(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            if (user.EmailConfirmed)
                return BadRequest(new { Error = "Email already confirmed." });

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var link = Url.Action("ConfirmEmail", "Auth", new { userId = user.Id, token }, Request.Scheme);
            await _emailService.SendAsync(user.Email!, "Confirm your email", $"Click to confirm: {link}");
            return NoContent();
        }

        // — DTOs —

        public record UserDto(
            string UserId,
            string Email,
            IList<string> Roles,
            bool EmailConfirmed,
            bool LockoutEnabled,
            DateTimeOffset? LockoutEnd
        );

        public record UpdateUserRolesDto(
            IList<string> Roles
        );

        public record LockoutDto(
            bool Lock,
            DateTimeOffset? End
        );

        public record ResetUserPasswordDto(
            string NewPassword,
            string ConfirmPassword
        );

        public record CreateRoleDto(
            string RoleName
        );

        public record RoleDto(
            string RoleName
        );
    }
}
