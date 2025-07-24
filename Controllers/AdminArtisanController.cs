using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OluBackendApp.Data;
using OluBackendApp.DTOs;
using OluBackendApp.Models;

namespace OluBackendApp.Controllers
{
    [ApiController]
    [Route("api/admin/artisans")]
    [Authorize(Roles = Roles.SuperAdmin)]
    public class AdminArtisanController : ControllerBase
    {
        private const long MAX_FILE_SIZE = 5 * 1024 * 1024; // 5 MB
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _env;

        public AdminArtisanController(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext db,
            IWebHostEnvironment env)
        {
            _userManager = userManager;
            _db = db;
            _env = env;
        }

        // POST /api/admin/artisans
        [HttpPost, RequestSizeLimit(MAX_FILE_SIZE)]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Create([FromForm] AdminCreateArtisanDto dto)
        {
            if (await _userManager.FindByEmailAsync(dto.Email) != null)
                return Conflict(new { Error = "Email already in use." });
            if (dto.Password != dto.ConfirmPassword)
                return BadRequest(new { Error = "Passwords do not match." });

            var user = new ApplicationUser { UserName = dto.Email, Email = dto.Email };
            var res = await _userManager.CreateAsync(user, dto.Password);
            if (!res.Succeeded) return BadRequest(res.Errors);

            await _userManager.AddToRoleAsync(user, Roles.Artisan);

            var picUrl = await SaveAndValidateImage(dto.ProfilePicture);
            var prof = new ArtisanProfile
            {
                UserId = user.Id,
                ProfilePictureUrl = picUrl,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                DateOfBirth = dto.DateOfBirth,
                Gender = dto.Gender,
                StateOfOrigin = dto.StateOfOrigin,
                ZipCode = dto.ZipCode,
                StateOfResidence = dto.StateOfResidence,
                Country = dto.Country,
                Profession = dto.Profession,
                YearsOfExperience = dto.YearsOfExperience,
                ServicesOffered = dto.ServicesOffered,
                AboutYou = dto.AboutYou,
                ProfessionTags = dto.ProfessionTags,
                PhoneNumbers = dto.PhoneNumbers,
                Address = dto.Address
            };
            _db.ArtisanProfiles.Add(prof);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = user.Id }, MapToDto(user, prof));
        }

        // GET /api/admin/artisans
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var list = await _db.ArtisanProfiles
                .AsNoTracking()
                .Where(p => !p.IsDeleted)
                .Include(p => p.User)
                .Select(p => MapToDto(p.User, p))
                .ToListAsync();
            return Ok(list);
        }

        // GET /api/admin/artisans/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var p = await _db.ArtisanProfiles
                .AsNoTracking()
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.UserId == id && !a.IsDeleted);
            if (p == null) return NotFound();
            return Ok(MapToDto(p.User, p));
        }

        // PUT /api/admin/artisans/{id}
        [HttpPut("{id}"), RequestSizeLimit(MAX_FILE_SIZE)]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Update(string id, [FromForm] AdminUpdateArtisanDto dto)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();
            var prof = await _db.ArtisanProfiles.FirstOrDefaultAsync(p => p.UserId == id && !p.IsDeleted);
            if (prof == null) return NotFound();

            // Email
            if (!string.IsNullOrWhiteSpace(dto.Email) && dto.Email != user.Email)
            {
                if (await _userManager.FindByEmailAsync(dto.Email) != null)
                    return Conflict(new { Error = "Email already in use." });
                user.Email = dto.Email;
                user.UserName = dto.Email;
                await _userManager.UpdateAsync(user);
            }

            // Password
            if (!string.IsNullOrEmpty(dto.Password))
            {
                if (dto.Password != dto.ConfirmPassword)
                    return BadRequest(new { Error = "Passwords do not match." });
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var reset = await _userManager.ResetPasswordAsync(user, token, dto.Password);
                if (!reset.Succeeded) return BadRequest(reset.Errors);
            }

            // Profile picture
            if (dto.ProfilePicture != null)
                prof.ProfilePictureUrl = await SaveAndValidateImage(dto.ProfilePicture);

            // Other fields
            if (dto.FirstName != null) prof.FirstName = dto.FirstName;
            if (dto.LastName != null) prof.LastName = dto.LastName;
            if (dto.DateOfBirth != null) prof.DateOfBirth = dto.DateOfBirth;
            if (dto.Gender != null) prof.Gender = dto.Gender;
            if (dto.StateOfOrigin != null) prof.StateOfOrigin = dto.StateOfOrigin;
            if (dto.ZipCode != null) prof.ZipCode = dto.ZipCode;
            if (dto.StateOfResidence != null) prof.StateOfResidence = dto.StateOfResidence;
            if (dto.Country != null) prof.Country = dto.Country;
            if (dto.Profession != null) prof.Profession = dto.Profession;
            if (dto.YearsOfExperience != null) prof.YearsOfExperience = dto.YearsOfExperience;
            if (dto.ServicesOffered != null) prof.ServicesOffered = dto.ServicesOffered;
            if (dto.AboutYou != null) prof.AboutYou = dto.AboutYou;
            if (dto.ProfessionTags != null) prof.ProfessionTags = dto.ProfessionTags;
            if (dto.PhoneNumbers != null) prof.PhoneNumbers = dto.PhoneNumbers;
            if (dto.Address != null) prof.Address = dto.Address;

            await _db.SaveChangesAsync();
            return NoContent();
        }

        // PATCH /api/admin/artisans/{id}
        [HttpPatch("{id}"), RequestSizeLimit(MAX_FILE_SIZE)]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Patch(string id, [FromForm] AdminPatchArtisanDto dto)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();
            var prof = await _db.ArtisanProfiles.FirstOrDefaultAsync(p => p.UserId == id && !p.IsDeleted);
            if (prof == null) return NotFound();

            // Email
            if (!string.IsNullOrWhiteSpace(dto.Email) && dto.Email != user.Email)
            {
                if (await _userManager.FindByEmailAsync(dto.Email) != null)
                    return Conflict(new { Error = "Email already in use." });
                user.Email = dto.Email;
                user.UserName = dto.Email;
                await _userManager.UpdateAsync(user);
            }

            // Password
            if (!string.IsNullOrEmpty(dto.Password))
            {
                if (dto.Password != dto.ConfirmPassword)
                    return BadRequest(new { Error = "Passwords do not match." });
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var reset = await _userManager.ResetPasswordAsync(user, token, dto.Password);
                if (!reset.Succeeded) return BadRequest(reset.Errors);
            }

            // Profile picture
            if (dto.ProfilePicture != null)
                prof.ProfilePictureUrl = await SaveAndValidateImage(dto.ProfilePicture);

            // Other fields
            if (dto.FirstName != null) prof.FirstName = dto.FirstName;
            if (dto.LastName != null) prof.LastName = dto.LastName;
            if (dto.DateOfBirth != null) prof.DateOfBirth = dto.DateOfBirth;
            if (dto.Gender != null) prof.Gender = dto.Gender;
            if (dto.StateOfOrigin != null) prof.StateOfOrigin = dto.StateOfOrigin;
            if (dto.ZipCode != null) prof.ZipCode = dto.ZipCode;
            if (dto.StateOfResidence != null) prof.StateOfResidence = dto.StateOfResidence;
            if (dto.Country != null) prof.Country = dto.Country;
            if (dto.Profession != null) prof.Profession = dto.Profession;
            if (dto.YearsOfExperience != null) prof.YearsOfExperience = dto.YearsOfExperience;
            if (dto.ServicesOffered != null) prof.ServicesOffered = dto.ServicesOffered;
            if (dto.AboutYou != null) prof.AboutYou = dto.AboutYou;
            if (dto.ProfessionTags != null) prof.ProfessionTags = dto.ProfessionTags;
            if (dto.PhoneNumbers != null) prof.PhoneNumbers = dto.PhoneNumbers;
            if (dto.Address != null) prof.Address = dto.Address;

            await _db.SaveChangesAsync();
            return NoContent();
        }

        // DELETE /api/admin/artisans/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            var prof = await _db.ArtisanProfiles.FirstOrDefaultAsync(p => p.UserId == id && !p.IsDeleted);
            if (user == null || prof == null) return NotFound();

            prof.IsDeleted = true;
            prof.DeletedAt = DateTime.UtcNow;
            user.LockoutEnabled = true;
            user.LockoutEnd = DateTimeOffset.MaxValue;

            await _db.SaveChangesAsync();
            await _userManager.UpdateAsync(user);

            return NoContent();
        }

        private async Task<string> SaveAndValidateImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("Profile picture is required.");
            if (!file.ContentType.StartsWith("image/"))
                throw new ArgumentException("Only image files are allowed.");
            if (file.Length > MAX_FILE_SIZE)
                throw new ArgumentException("Image must be ≤ 5 MB.");

            var uploads = Path.Combine(_env.WebRootPath, "uploads");
            Directory.CreateDirectory(uploads);

            var ext = Path.GetExtension(file.FileName);
            var fileName = $"{Guid.NewGuid()}{ext}";
            var path = Path.Combine(uploads, fileName);

            await using var stream = new FileStream(path, FileMode.Create);
            await file.CopyToAsync(stream);

            return $"/uploads/{fileName}";
        }

        private static ProfileDto MapToDto(ApplicationUser user, ArtisanProfile p)
            => new(
                p.UserId,
                user.Email!,
                Roles.Artisan,
                p.FirstName,
                p.LastName,
                p.DateOfBirth,
                p.Gender?.ToString(),
                p.StateOfOrigin?.ToString(),
                p.ZipCode,
                p.StateOfResidence,
                p.Country?.ToString(),
                p.Profession,
                p.YearsOfExperience,
                p.ServicesOffered,
                p.AboutYou,
                p.ProfessionTags,
                p.PhoneNumbers,
                p.Address,
                p.ProfilePictureUrl
            );
    }
}
