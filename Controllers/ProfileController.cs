// NOT NEEDED ANY MORE

//using System.Linq;
//using System.Threading.Tasks;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Mvc;
//using OluBackendApp.Data;
//using OluBackendApp.Models;

//namespace OluBackendApp.Controllers
//{
//    [ApiController]
//    [Route("api/profile")]
//    [Authorize]
//    public class ProfileController : ControllerBase
//    {
//        private readonly UserManager<ApplicationUser> _userManager;
//        private readonly ApplicationDbContext _db;

//        public ProfileController(
//            UserManager<ApplicationUser> userManager,
//            ApplicationDbContext db)
//        {
//            _userManager = userManager;
//            _db = db;
//        }

//        /// <summary>
//        /// GET api/profile
//        /// Returns the current user's profile (Artisan or OfficeOwner).
//        /// </summary>
//        [HttpGet]
//        public async Task<IActionResult> GetProfile()
//        {
//            var user = await _userManager.GetUserAsync(User);
//            if (user == null)
//                return Unauthorized();

//            var roles = await _userManager.GetRolesAsync(user);
//            var role = roles.FirstOrDefault();

//            return role switch
//            {
//                Roles.Artisan => await GetArtisanProfile(user.Id, user.Email!),
//                Roles.OfficeOwner => await GetOfficeOwnerProfile(user.Id, user.Email!),
//                _ => BadRequest("Profile not supported for this role.")
//            };
//        }

//        /// <summary>
//        /// PUT api/profile
//        /// Updates the current user's profile fields.
//        /// </summary>
//        [HttpPut]
//        public async Task<IActionResult> UpdateProfile(UpdateProfileDto dto)
//        {
//            var user = await _userManager.GetUserAsync(User);
//            if (user == null)
//                return Unauthorized();

//            var roles = await _userManager.GetRolesAsync(user);
//            var role = roles.FirstOrDefault();

//            switch (role)
//            {
//                case Roles.Artisan:
//                    var art = await _db.ArtisanProfiles.FindAsync(user.Id);
//                    if (art == null) return NotFound("Artisan profile not found.");
//                    art.ProfilePictureUrl = dto.ProfilePictureUrl;
//                    art.Address = dto.Address;
//                    art.PhoneNumber = dto.PhoneNumber;
//                    art.State = dto.State;
//                    break;

//                case Roles.OfficeOwner:
//                    var off = await _db.OfficeOwnerProfiles.FindAsync(user.Id);
//                    if (off == null) return NotFound("OfficeOwner profile not found.");
//                    off.ProfilePictureUrl = dto.ProfilePictureUrl;
//                    off.Address = dto.Address;
//                    off.PhoneNumber = dto.PhoneNumber;
//                    off.State = dto.State;
//                    break;

//                default:
//                    return BadRequest("Profile not supported for this role.");
//            }

//            await _db.SaveChangesAsync();
//            return NoContent();
//        }

//        // ——— Helpers for GET ———

//        private async Task<IActionResult> GetArtisanProfile(string userId, string email)
//        {
//            var p = await _db.ArtisanProfiles.FindAsync(userId);
//            if (p == null)
//                return NotFound("Artisan profile not found.");

//            var dto = new ProfileDto(
//                Email: email,
//                Role: Roles.Artisan,
//                ProfilePictureUrl: p.ProfilePictureUrl ?? "",
//                Address: p.Address ?? "",
//                PhoneNumber: p.PhoneNumber ?? "",
//                State: p.State ?? ""
//            );
//            return Ok(dto);
//        }

//        private async Task<IActionResult> GetOfficeOwnerProfile(string userId, string email)
//        {
//            var p = await _db.OfficeOwnerProfiles.FindAsync(userId);
//            if (p == null)
//                return NotFound("OfficeOwner profile not found.");

//            var dto = new ProfileDto(
//                Email: email,
//                Role: Roles.OfficeOwner,
//                ProfilePictureUrl: p.ProfilePictureUrl ?? "",
//                Address: p.Address ?? "",
//                PhoneNumber: p.PhoneNumber ?? "",
//                State: p.State ?? ""
//            );
//            return Ok(dto);
//        }

//        // ——— DTOs ———

//        public record ProfileDto(
//            string Email,
//            string Role,
//            string ProfilePictureUrl,
//            string Address,
//            string PhoneNumber,
//            string State
//        );

//        public record UpdateProfileDto(
//            string ProfilePictureUrl,
//            string Address,
//            string PhoneNumber,
//            string State
//        );
//    }
//}
