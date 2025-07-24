
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using global::OluBackendApp.Data;
    using global::OluBackendApp.DTOs;
    using global::OluBackendApp.Models;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;


    namespace OluBackendApp.Controllers
    {
        [ApiController]
        [Route("api/artisan/profile")]
        [Authorize(Roles = Roles.Artisan)]
        public class ArtisanProfileController : ControllerBase
        {
            private const long MAX_FILE_SIZE = 5 * 1024 * 1024; // 5 MB
            private readonly UserManager<ApplicationUser> _userManager;
            private readonly ApplicationDbContext _db;
            private readonly IWebHostEnvironment _env;

            public ArtisanProfileController(
                UserManager<ApplicationUser> userManager,
                ApplicationDbContext db,
                IWebHostEnvironment env)
            {
                _userManager = userManager;
                _db = db;
                _env = env;
            }

            /// <summary>Retrieve current artisan profile</summary>
            [HttpGet]
            public async Task<ActionResult<ProfileDto>> GetOwn()
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return Unauthorized();

                var prof = await _db.ArtisanProfiles
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.UserId == user.Id && !p.IsDeleted);
                if (prof == null) return NotFound(new { Error = "Profile not found." });

                return Ok(MapToDto(user, prof));
            }

            /// <summary>Create artisan profile</summary>
            [HttpPost]
            [RequestSizeLimit(MAX_FILE_SIZE)]
            [Consumes("multipart/form-data")]
            public async Task<ActionResult<ProfileDto>> Create([FromForm] ProfileCreateDto form)
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);

                var user = await _userManager.GetUserAsync(User);
                if (user == null) return Unauthorized();

                if (await _db.ArtisanProfiles.AnyAsync(p => p.UserId == user.Id && !p.IsDeleted))
                    return Conflict(new { Error = "Profile already exists." });

                var picUrl = await SaveAndValidateImage(form.ProfilePicture);

                var prof = new ArtisanProfile
                {
                    UserId = user.Id,
                    ProfilePictureUrl = picUrl,
                    FirstName = form.FirstName,
                    LastName = form.LastName,
                    DateOfBirth = form.DateOfBirth,
                    Gender = form.Gender,
                    StateOfOrigin = form.StateOfOrigin,
                    ZipCode = form.ZipCode,
                    StateOfResidence = form.StateOfResidence,
                    Country = form.Country,
                    Profession = form.Profession,
                    YearsOfExperience = form.YearsOfExperience,
                    ServicesOffered = form.ServicesOffered,
                    AboutYou = form.AboutYou,
                    ProfessionTags = form.ProfessionTags,
                    PhoneNumbers = form.PhoneNumbers,
                    Address = form.Address
                };
                _db.ArtisanProfiles.Add(prof);
                await _db.SaveChangesAsync();

                return CreatedAtAction(nameof(GetOwn), MapToDto(user, prof));
            }

            /// <summary>Replace artisan profile</summary>
            [HttpPut]
            [RequestSizeLimit(MAX_FILE_SIZE)]
            [Consumes("multipart/form-data")]
            public async Task<IActionResult> Update([FromForm] ProfileUpdateDto form)
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);

                var user = await _userManager.GetUserAsync(User);
                if (user == null) return Unauthorized();

                var prof = await _db.ArtisanProfiles
                    .FirstOrDefaultAsync(p => p.UserId == user.Id && !p.IsDeleted);
                if (prof == null) return NotFound(new { Error = "Profile not found." });

                prof.ProfilePictureUrl = await SaveAndValidateImage(form.ProfilePicture);
                prof.FirstName = form.FirstName;
                prof.LastName = form.LastName;
                prof.DateOfBirth = form.DateOfBirth;
                prof.Gender = form.Gender;
                prof.StateOfOrigin = form.StateOfOrigin;
                prof.ZipCode = form.ZipCode;
                prof.StateOfResidence = form.StateOfResidence;
                prof.Country = form.Country;
                prof.Profession = form.Profession;
                prof.YearsOfExperience = form.YearsOfExperience;
                prof.ServicesOffered = form.ServicesOffered;
                prof.AboutYou = form.AboutYou;
                prof.ProfessionTags = form.ProfessionTags;
                prof.PhoneNumbers = form.PhoneNumbers;
                prof.Address = form.Address;

                await _db.SaveChangesAsync();
                return NoContent();
            }

            /// <summary>Partially update artisan profile</summary>
            [HttpPatch]
            [RequestSizeLimit(MAX_FILE_SIZE)]
            [Consumes("multipart/form-data")]
            public async Task<IActionResult> Patch([FromForm] ProfilePatchDto form)
            {
                if (!ModelState.IsValid) return BadRequest(ModelState);

                var user = await _userManager.GetUserAsync(User);
                if (user == null) return Unauthorized();

                var prof = await _db.ArtisanProfiles
                    .FirstOrDefaultAsync(p => p.UserId == user.Id && !p.IsDeleted);
                if (prof == null) return NotFound(new { Error = "Profile not found." });

                if (form.ProfilePicture != null) prof.ProfilePictureUrl = await SaveAndValidateImage(form.ProfilePicture);
                if (form.FirstName != null) prof.FirstName = form.FirstName;
                if (form.LastName != null) prof.LastName = form.LastName;
                if (form.DateOfBirth != null) prof.DateOfBirth = form.DateOfBirth;
                if (form.Gender != null) prof.Gender = form.Gender;
                if (form.StateOfOrigin != null) prof.StateOfOrigin = form.StateOfOrigin;
                if (form.ZipCode != null) prof.ZipCode = form.ZipCode;
                if (form.StateOfResidence != null) prof.StateOfResidence = form.StateOfResidence;
                if (form.Country != null) prof.Country = form.Country;
                if (form.Profession != null) prof.Profession = form.Profession;
                if (form.YearsOfExperience != null) prof.YearsOfExperience = form.YearsOfExperience;
                if (form.ServicesOffered != null) prof.ServicesOffered = form.ServicesOffered;
                if (form.AboutYou != null) prof.AboutYou = form.AboutYou;
                if (form.ProfessionTags != null) prof.ProfessionTags = form.ProfessionTags;
                if (form.PhoneNumbers != null) prof.PhoneNumbers = form.PhoneNumbers;
                if (form.Address != null) prof.Address = form.Address;

                await _db.SaveChangesAsync();
                return NoContent();
            }

            /// <summary>Soft‑delete artisan profile</summary>
            [HttpDelete]
            public async Task<IActionResult> Delete()
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null) return Unauthorized();

                var prof = await _db.ArtisanProfiles
                    .FirstOrDefaultAsync(p => p.UserId == user.Id && !p.IsDeleted);
                if (prof == null) return NotFound(new { Error = "Profile not found." });

                prof.IsDeleted = true;
                prof.DeletedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();
                return NoContent();
            }

            /// <summary>Save/upload and validate image (≤5 MB, image/*)</summary>
            private async Task<string> SaveAndValidateImage(IFormFile file)
            {
                if (file == null || file.Length == 0)
                    throw new ArgumentException("Profile picture is required.");
                if (!file.ContentType.StartsWith("image/"))
                    throw new ArgumentException("Only image files are allowed.");
                if (file.Length > MAX_FILE_SIZE)
                    throw new ArgumentException("Image must be ≤ 5 MB.");

                var uploads = Path.Combine(_env.WebRootPath, "uploads");
                Directory.CreateDirectory(uploads);

                var ext = Path.GetExtension(file.FileName);
                var fileName = $"{Guid.NewGuid()}{ext}";
                var path = Path.Combine(uploads, fileName);

                await using var stream = new FileStream(path, FileMode.Create);
                await file.CopyToAsync(stream);

                return $"/uploads/{fileName}";
            }

            /// <summary>Map entity → DTO</summary>
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


