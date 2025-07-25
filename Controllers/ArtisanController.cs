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
    [Route("api/artisan/profile")]
    [Authorize(Roles = Roles.Artisan)]
    [Produces("application/json")]
    [ApiExplorerSettings(GroupName = "Artisan")]
    public class ArtisanProfileController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _env;
        private const long MAX_FILE_SIZE = 5 * 1024 * 1024;

        public ArtisanProfileController(ApplicationDbContext db, UserManager<ApplicationUser> userManager, IWebHostEnvironment env)
        {
            _db = db;
            _userManager = userManager;
            _env = env;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ProfileDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Get()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var profile = await _db.ArtisanProfiles.FirstOrDefaultAsync(p => p.UserId == user.Id && !p.IsDeleted);
            if (profile == null) return NotFound();

            return Ok(MapToDto(user, profile));
        }

        [HttpPost]
        [RequestSizeLimit(MAX_FILE_SIZE)]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ProfileDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Create([FromForm] ProfileCreateDto dto)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            if (await _db.ArtisanProfiles.AnyAsync(p => p.UserId == user.Id && !p.IsDeleted))
                return Conflict(new { Error = "Profile already exists." });

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var picUrl = await SaveAndValidateImage(dto.ProfilePicture);

            var profile = new ArtisanProfile
            {
                UserId = user.Id,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                DateOfBirth = DateOnly.FromDateTime(dto.DateOfBirth),
                Gender = dto.Gender,
                State = dto.State,
                ZipCode = dto.ZipCode,
                Country = dto.Country,
                Profession = dto.Profession,
                YearsOfExperience = dto.YearsOfExperience,
                ServicesOffered = dto.ServicesOffered,
                AboutYou = dto.AboutYou,
                ProfessionTags = dto.ProfessionTags,
                PhoneNumbers = dto.PhoneNumbers,
                Address = dto.Address,
                ProfilePictureUrl = picUrl
            };

            _db.ArtisanProfiles.Add(profile);
            await _db.SaveChangesAsync();
            return Ok(MapToDto(user, profile));
        }

        [HttpPut]
        [RequestSizeLimit(MAX_FILE_SIZE)]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update([FromForm] ProfileUpdateDto dto)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var profile = await _db.ArtisanProfiles.FirstOrDefaultAsync(p => p.UserId == user.Id && !p.IsDeleted);
            if (profile == null) return NotFound();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (dto.ProfilePicture != null)
                profile.ProfilePictureUrl = await SaveAndValidateImage(dto.ProfilePicture);

            profile.FirstName = dto.FirstName;
            profile.LastName = dto.LastName;
            profile.DateOfBirth = DateOnly.FromDateTime(dto.DateOfBirth);
            profile.Gender = dto.Gender;
            profile.State = dto.State;
            profile.ZipCode = dto.ZipCode;
            profile.Country = dto.Country;
            profile.Profession = dto.Profession;
            profile.YearsOfExperience = dto.YearsOfExperience;
            profile.ServicesOffered = dto.ServicesOffered;
            profile.AboutYou = dto.AboutYou;
            profile.ProfessionTags = dto.ProfessionTags;
            profile.PhoneNumbers = dto.PhoneNumbers;
            profile.Address = dto.Address;

            await _db.SaveChangesAsync();
            return NoContent();
        }

        [HttpPatch]
        [RequestSizeLimit(MAX_FILE_SIZE)]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Patch([FromForm] ProfilePatchDto dto)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var profile = await _db.ArtisanProfiles.FirstOrDefaultAsync(p => p.UserId == user.Id && !p.IsDeleted);
            if (profile == null) return NotFound();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (dto.ProfilePicture != null)
                profile.ProfilePictureUrl = await SaveAndValidateImage(dto.ProfilePicture);

            if (dto.FirstName != null) profile.FirstName = dto.FirstName;
            if (dto.LastName != null) profile.LastName = dto.LastName;
            if (dto.DateOfBirth != null) profile.DateOfBirth = DateOnly.FromDateTime(dto.DateOfBirth.Value);
            if (dto.Gender != null) profile.Gender = dto.Gender;
            if (dto.State != null) profile.State = dto.State;
            if (dto.ZipCode != null) profile.ZipCode = dto.ZipCode;
            if (dto.Country != null) profile.Country = dto.Country;
            if (dto.Profession != null) profile.Profession = dto.Profession;
            if (dto.YearsOfExperience != null) profile.YearsOfExperience = dto.YearsOfExperience;
            if (dto.ServicesOffered != null) profile.ServicesOffered = dto.ServicesOffered;
            if (dto.AboutYou != null) profile.AboutYou = dto.AboutYou;
            if (dto.ProfessionTags != null) profile.ProfessionTags = dto.ProfessionTags;
            if (dto.PhoneNumbers != null) profile.PhoneNumbers = dto.PhoneNumbers;
            if (dto.Address != null) profile.Address = dto.Address;

            await _db.SaveChangesAsync();
            return NoContent();
        }

        private async Task<string> SaveAndValidateImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("Profile picture is required.");
            if (file.Length > MAX_FILE_SIZE)
                throw new ArgumentException("Image must be ≤ 5 MB.");
            var allowedTypes = new[] { "image/jpeg", "image/png" };
            if (!allowedTypes.Contains(file.ContentType.ToLower()))
                throw new ArgumentException("Only JPG and PNG images are allowed.");
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (ext != ".jpg" && ext != ".jpeg" && ext != ".png")
                throw new ArgumentException("Only .jpg and .png extensions are allowed.");

            var uploads = Path.Combine(_env.WebRootPath, "uploads");
            Directory.CreateDirectory(uploads);
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
                p.DateOfBirth?.ToDateTime(TimeOnly.MinValue),
                p.Gender?.ToString(),
                p.State?.ToString(),
                p.ZipCode,
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