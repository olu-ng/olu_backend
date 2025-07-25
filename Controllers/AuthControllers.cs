//using System;
//using System.Collections.Generic;
//using System.IdentityModel.Tokens.Jwt;
//using System.Linq;
//using System.Security.Claims;
//using System.Security.Cryptography;
//using System.Text;
//using System.Threading.Tasks;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.Extensions.Configuration;
//using Microsoft.IdentityModel.Tokens;
//using OluBackendApp.Data;
//using OluBackendApp.DTOs;
//using OluBackendApp.Models;
//using OluBackendApp.Services;

//namespace OluBackendApp.Controllers
//{
//    [ApiController]
//    [Route("api/auth")]
//    public class AuthController : ControllerBase
//    {
//        private readonly UserManager<ApplicationUser> _userManager;
//        private readonly SignInManager<ApplicationUser> _signInManager;
//        private readonly IOtpService _otpService;
//        private readonly IConfiguration _config;
//        private readonly ApplicationDbContext _db;

//        public AuthController(
//            UserManager<ApplicationUser> userManager,
//            SignInManager<ApplicationUser> signInManager,
//            IOtpService otpService,
//            IConfiguration config,
//            ApplicationDbContext db)
//        {
//            _userManager = userManager;
//            _signInManager = signInManager;
//            _otpService = otpService;
//            _config = config;
//            _db = db;
//        }

//        // 1) Register: only create after ALL guards pass
//        [HttpPost("register")]
//        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
//        {
//            // 1a) Basic model validation
//            if (!ModelState.IsValid)
//                return BadRequest(ModelState);

//            // 1b) Role whitelist
//            var allowedRoles = new[] { Roles.Artisan, Roles.OfficeOwner, Roles.Admin, Roles.SuperAdmin };
//            if (!allowedRoles.Contains(dto.Role))
//                return BadRequest(new { Error = $"Role must be one of: {string.Join(", ", allowedRoles)}" });

//            // 1c) Duplicate email?
//            if (await _userManager.FindByEmailAsync(dto.Email) != null)
//                return Conflict(new { Error = "Email already registered." });

//            // 1d) Passwords match?
//            if (dto.Password != dto.ConfirmPassword)
//                return BadRequest(new { Error = "Passwords do not match." });

//            // — All checks passed! Now we can safely create —

//            var user = new ApplicationUser
//            {
//                UserName = dto.Email,
//                Email = dto.Email
//            };

//            var createResult = await _userManager.CreateAsync(user, dto.Password);
//            if (!createResult.Succeeded)
//                return BadRequest(createResult.Errors);

//            var roleResult = await _userManager.AddToRoleAsync(user, dto.Role);
//            if (!roleResult.Succeeded)
//                return BadRequest(roleResult.Errors);

//            // Create an empty profile record based on role
//            // Create an empty profile record based on the assigned role
//            if (dto.Role == Roles.Artisan)
//            {
//                _db.ArtisanProfiles.Add(new ArtisanProfile { UserId = user.Id });
//            }
//            else if (dto.Role == Roles.OfficeOwner)
//            {
//                _db.OfficeOwnerProfiles.Add(new OfficeOwnerProfile { UserId = user.Id });
//            }
//            else if (dto.Role == Roles.Admin)
//            {
//                _db.AdminProfiles.Add(new AdminProfile { UserId = user.Id });
//            }
//            else if (dto.Role == Roles.SuperAdmin)
//            {
//                _db.SuperAdminProfiles.Add(new SuperAdminProfile { UserId = user.Id });
//            }

//            await _db.SaveChangesAsync();

//            // Generate registration OTP
//            await _otpService.GenerateAsync(
//                user.Id,
//                OtpPurpose.Registration,
//                GetFingerprint());

//            //return Ok(new { RequiresOtp = true });
//            // Build a detailed response
//            var response = new RegisterResponseDto
//            {
//                RequiresOtp = true,
//                UserId = user.Id,
//                Email = user.Email!,
//                Role = dto.Role,
//                OtpPurpose = OtpPurpose.Registration.ToString(),
//                OtpSentAt = DateTime.UtcNow,
//                NextStep = "Please verify the OTP sent to your email."
//            };

//            return Ok(response);
//        }


//        [HttpPost("verify")]
//        public async Task<IActionResult> Verify([FromBody] OtpVerifyDto dto)
//        {
//            if (!ModelState.IsValid)
//                return BadRequest(ModelState);

//            var user = await _userManager.FindByEmailAsync(dto.Email);
//            if (user == null)
//                return BadRequest(new { Error = "Invalid user." });

//            // — DEVICE‑CHANGE DETECTION —
//            // If this user has a saved fingerprint and it doesn't match the current one,
//            // we block even OTP verification.
//            var currentFp = GetFingerprint();
//            if (user.LastDeviceFingerprint != null && user.LastDeviceFingerprint != currentFp)
//            {
//                // 403 Forbidden — fingerprint mismatch
//                return StatusCode(StatusCodes.Status403Forbidden,
//                    new { Error = "Device change detected. OTP verification denied." });
//            }

//            // — OTP VALIDATION (assumes Registration OTP) —
//            const OtpPurpose purpose = OtpPurpose.Registration;
//            var valid = await _otpService.ValidateAsync(
//                user.Id,
//                purpose,
//                dto.Code,
//                currentFp);

//            if (!valid)
//                return BadRequest(new { Error = "Invalid or expired OTP." });

//            // Persist fingerprint on first‑time registration
//            user.EmailConfirmed = true;
//            user.LastDeviceFingerprint = currentFp;
//            await _userManager.UpdateAsync(user);

//            // Build detailed response
//            var roles = await _userManager.GetRolesAsync(user);
//            var response = new OtpVerifyResponseDto
//            {
//                Verified = true,
//                UserId = user.Id,
//                Email = user.Email!,
//                Role = roles.FirstOrDefault() ?? "",
//                Message = "OTP verified successfully. Please proceed to log in.",
//                NextStep = "login"
//            };

//            return Ok(response);
//        }



//        // 3) Login → maybe new-device OTP or JWT

//        [HttpPost("login")]
//        public async Task<IActionResult> Login([FromBody] LoginDto dto)
//        {
//            if (!ModelState.IsValid)
//                return BadRequest(ModelState);

//            var user = await _userManager.FindByEmailAsync(dto.Email);
//            if (user == null)
//                return Unauthorized(new
//                {
//                    Authenticated = false,
//                    Message = "Invalid credentials."
//                });

//            var currentFp = GetFingerprint();

//            // 1) Device‑change: send OTP if fingerprint mismatch
//            if (user.LastDeviceFingerprint != null && user.LastDeviceFingerprint != currentFp)
//            {
//                await _otpService.GenerateAsync(user.Id, OtpPurpose.NewDevice, currentFp);

//                return Ok(new LoginResponseDto
//                {
//                    RequiresOtp = true,
//                    OtpPurpose = OtpPurpose.NewDevice.ToString(),
//                    Authenticated = false,
//                    Message = "New device detected. OTP sent to your email.",
//                    NextStep = "verify-device"
//                });
//            }

//            // 2) Password check
//            var signIn = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, false);
//            if (!signIn.Succeeded)
//                return Unauthorized(new
//                {
//                    Authenticated = false,
//                    Message = "Invalid credentials."
//                });

//            // 3) Successful login → generate token
//            var token = await GenerateJwt(user);
//            var expiresRaw = _config.GetSection("Jwt")["ExpireMinutes"];
//            var expiresMin = int.TryParse(expiresRaw, out var m) ? m : 0;
//            var expiresAt = DateTime.UtcNow.AddMinutes(expiresMin).ToString("o");

//            // 4) Fetch role
//            var roles = await _userManager.GetRolesAsync(user);
//            var role = roles.FirstOrDefault() ?? string.Empty;

//            return Ok(new LoginResponseDto
//            {
//                RequiresOtp = false,
//                OtpPurpose = null,
//                Authenticated = true,
//                Token = token,
//                TokenExpiresAt = expiresAt,
//                UserId = user.Id,
//                Email = user.Email,
//                Role = role,
//                Message = "Login successful.",
//                NextStep = "use-token"
//            });
//        }


//        // 4) Forgot → OTP (no user enumeration)

//        [HttpPost("forgot-password")]
//        public async Task<IActionResult> Forgot([FromBody] ForgotDto dto)
//        {
//            if (!ModelState.IsValid)
//                return BadRequest(ModelState);

//            var user = await _userManager.FindByEmailAsync(dto.Email);
//            if (user != null)
//            {
//                // Only generate OTP if the account exists
//                await _otpService.GenerateAsync(
//                    user.Id,
//                    OtpPurpose.ForgotPassword,
//                    GetFingerprint());
//            }

//            // Always return the same payload, so we don't leak account existence
//            var response = new ForgotPasswordResponseDto
//            {
//                RequiresOtp = true,
//                Message = "If an account with that email exists, an OTP has been sent for password reset.",
//                NextStep = "verify-forgot"
//            };

//            return Ok(response);
//        }


//        // 5) Reset password (Identity token)
//        [HttpPost("reset-password-token")]
//        public async Task<IActionResult> Reset([FromBody] ResetDto dto)
//        {
//            if (!ModelState.IsValid)
//                return BadRequest(ModelState);

//            var user = await _userManager.FindByEmailAsync(dto.Email);
//            if (user == null)
//                return BadRequest(new { Error = "Invalid user." });

//            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
//            var resetResult = await _userManager.ResetPasswordAsync(user, resetToken, dto.NewPassword);
//            if (!resetResult.Succeeded)
//                return BadRequest(resetResult.Errors);

//            //return Ok();
//            return Ok(new GenericResponseDto
//            {
//                Message = "Token Refreshed successfully."
//            });
//        }

//        // 6a) Send OTP to change password
//        [Authorize]
//        [HttpPost("send-change-otp")]
//        public async Task<IActionResult> SendChangeOtp()
//        {
//            var user = await _userManager.GetUserAsync(User)!;
//            await _otpService.GenerateAsync(
//                user.Id,
//                OtpPurpose.ChangePassword,
//                GetFingerprint());
//            //return Ok();
//            return Ok(new GenericResponseDto
//            {
//                Message = "If you are a user an OTP has been sent to your email address."
//            });
//        }

//        // 6b) Change password
//        [Authorize]
//        [HttpPost("change-password")]
//        public async Task<IActionResult> Change([FromBody] ChangeDto dto)
//        {
//            if (!ModelState.IsValid)
//                return BadRequest(ModelState);

//            var user = await _userManager.GetUserAsync(User)!;
//            var result = await _userManager.ChangePasswordAsync(
//                user,
//                dto.CurrentPassword,
//                dto.NewPassword);

//            if (!result.Succeeded)
//                return BadRequest(result.Errors);

//            return Ok(new GenericResponseDto
//            {
//                Message = "Password changed successfully."
//            });
//        }



//        [Authorize]
//        [HttpGet("me")]
//        public async Task<ActionResult<UserProfileDto>> Me()
//        {
//            var user = await _userManager.GetUserAsync(User);
//            var roles = await _userManager.GetRolesAsync(user);
//            return Ok(new UserProfileDto
//            {
//                UserId = user.Id,
//                Email = user.Email,
//                Role = roles.FirstOrDefault() ?? ""
//                // …any other fields…
//            });
//        }


//        // RESEND OTP

//        [HttpPost("resend-otp")]
//        public async Task<IActionResult> ResendOtp([FromBody] ResendOtpRequestDto dto)
//        {
//            var user = await _userManager.FindByEmailAsync(dto.Email);
//            if (user != null)
//            {
//                var purpose = dto.Flow switch
//                {
//                    "forgot" => OtpPurpose.ForgotPassword,
//                    "new-device" => OtpPurpose.NewDevice,
//                    _ => OtpPurpose.Registration
//                };
//                await _otpService.GenerateAsync(user.Id, purpose, GetFingerprint());
//            }
//            return Ok(new ResendOtpResponseDto
//            {
//                RequiresOtp = true,
//                NextStep = $"verify-{dto.Flow}"
//            });
//        }


//        [HttpPost("refresh-token")]
//        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto dto)
//        {
//            if (!ModelState.IsValid)
//                return BadRequest(ModelState);

//            // 1) Extract principal (even if JWT expired)
//            var principal = _tokenService.GetPrincipalFromExpiredToken(dto.RefreshToken);
//            var userId = principal.FindFirstValue(JwtRegisteredClaimNames.Sub);
//            if (userId == null)
//                return Unauthorized();

//            var user = await _userManager.FindByIdAsync(userId);
//            if (user == null)
//                return Unauthorized();

//            // 2) Validate the incoming refresh token against store
//            if (!await _refreshTokenService.ValidateRefreshTokenAsync(userId, dto.RefreshToken))
//                return Unauthorized();

//            // 3) Generate new tokens
//            var newJwt = await GenerateJwt(user);
//            var newRefreshTok = _refreshTokenService.GenerateRefreshToken();
//            await _refreshTokenService.SaveRefreshTokenAsync(userId, newRefreshTok);

//            // 4) Compute expiry
//            var expiresMin = int.Parse(_config["Jwt:ExpireMinutes"]!);
//            var expiresAt = DateTime.UtcNow.AddMinutes(expiresMin).ToString("o");

//            return Ok(new RefreshTokenResponseDto
//            {
//                Token = newJwt,
//                ExpiresAt = expiresAt,
//                RefreshToken = newRefreshTok
//            });
//        }


//        // 3a) POST /api/auth/verify-device
//        [HttpPost("verify-device")]
//        public async Task<IActionResult> VerifyDevice([FromBody] OtpVerifyDto dto)
//        {
//            if (!ModelState.IsValid)
//                return BadRequest(ModelState);

//            var user = await _userManager.FindByEmailAsync(dto.Email);
//            if (user == null)
//                return BadRequest(new { Error = "Invalid user." });

//            // Validate NewDevice OTP
//            var fingerprint = GetFingerprint();
//            var valid = await _otpService.ValidateAsync(
//                user.Id,
//                OtpPurpose.NewDevice,
//                dto.Code,
//                fingerprint);

//            if (!valid)
//                return BadRequest(new { Error = "Invalid or expired OTP." });

//            // Persist new device fingerprint
//            user.LastDeviceFingerprint = fingerprint;
//            await _userManager.UpdateAsync(user);

//            var response = new VerifyDeviceResponseDto
//            {
//                Verified = true,
//                Message = "Device verified successfully. Please proceed to log in.",
//                NextStep = "login"
//            };

//            return Ok(response);
//        }

//        // 3b) POST /api/auth/verify-forgot
//        [HttpPost("verify-forgot-password")]
//        public async Task<IActionResult> VerifyForgot([FromBody] OtpVerifyDto dto)
//        {
//            if (!ModelState.IsValid)
//                return BadRequest(ModelState);

//            var user = await _userManager.FindByEmailAsync(dto.Email);
//            if (user == null)
//                return BadRequest(new { Error = "Invalid user." });

//            // Validate ForgotPassword OTP
//            var fingerprint = GetFingerprint();
//            var valid = await _otpService.ValidateAsync(
//                user.Id,
//                OtpPurpose.ForgotPassword,
//                dto.Code,
//                fingerprint);

//            if (!valid)
//                return BadRequest(new { Error = "Invalid or expired OTP." });

//            // Issue a password‑reset token (short‑lived)
//            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);

//            var response = new VerifyForgotResponseDto
//            {
//                Verified = true,
//                ResetToken = resetToken,
//                Message = "OTP verified. Use the provided token to reset your password.",
//                NextStep = "reset-password"
//            };

//            return Ok(response);
//        }

//        // 3c) POST /api/auth/logout
//        [Authorize]
//        [HttpPost("logout")]
//        public async Task<IActionResult> Logout()
//        {
//            // Invalidate all existing tokens by bumping the security stamp
//            var user = await _userManager.GetUserAsync(User)!;
//            await _userManager.UpdateSecurityStampAsync(user);

//            var response = new LogoutResponseDto
//            {
//                Message = "Logged out successfully."
//            };

//            return Ok(response);
//        }


//        // — Helpers —

//        private string GetFingerprint()
//        {
//            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "";
//            Request.Headers.TryGetValue("User-Agent", out var ua);
//            var raw = $"{ip}|{ua}";
//            var hash = SHA256.HashData(Encoding.UTF8.GetBytes(raw));
//            return Convert.ToHexString(hash);
//        }

//        private async Task<string> GenerateJwt(ApplicationUser user)
//        {
//            var section = _config.GetSection("Jwt");

//            var keyRaw = section["Key"];
//            var issuer = section["Issuer"];
//            var audience = section["Audience"];
//            var expireRaw = section["ExpireMinutes"];

//            // Validate config
//            if (string.IsNullOrWhiteSpace(keyRaw)
//             || string.IsNullOrWhiteSpace(issuer)
//             || string.IsNullOrWhiteSpace(audience)
//             || string.IsNullOrWhiteSpace(expireRaw)
//             || !int.TryParse(expireRaw, out var expireMinutes))
//            {
//                throw new InvalidOperationException(
//                    "The JWT configuration is invalid. " +
//                    "Please ensure 'Jwt:Key', 'Jwt:Issuer', 'Jwt:Audience', and " +
//                    "'Jwt:ExpireMinutes' are present and valid integers.");
//            }

//            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyRaw));
//            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
//            var roles = await _userManager.GetRolesAsync(user);

//            var claims = new List<Claim>
//    {
//        new(JwtRegisteredClaimNames.Sub,   user.Id),
//        new(JwtRegisteredClaimNames.Email, user.Email!)
//    };
//            claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

//            var token = new JwtSecurityToken(
//                issuer: issuer,
//                audience: audience,
//                claims: claims,
//                expires: DateTime.UtcNow.AddMinutes(expireMinutes),
//                signingCredentials: creds);

//            return new JwtSecurityTokenHandler().WriteToken(token);
//        }

//    }
//}






using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using OluBackendApp.Data;
using OluBackendApp.DTOs;
using OluBackendApp.Models;
using OluBackendApp.Services;

namespace OluBackendApp.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IOtpService _otpService;
        private readonly ITokenService _tokenService;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly IConfiguration _config;
        private readonly ApplicationDbContext _db;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IOtpService otpService,
            ITokenService tokenService,
            IRefreshTokenService refreshTokenService,
            IConfiguration config,
            ApplicationDbContext db)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _otpService = otpService;
            _tokenService = tokenService;
            _refreshTokenService = refreshTokenService;
            _config = config;
            _db = db;
        }


        /// <summary>
        /// Registers a new user, creates their profile, and sends a registration OTP.
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     POST /api/auth/register
        ///     {
        ///         "email": "user@example.com",
        ///         "password": "P@ssw0rd!",
        ///         "confirmPassword": "P@ssw0rd!",
        ///         "role": "Artisan"
        ///     }
        ///
        /// Valid values for <c>role</c>:
        /// - "Artisan"
        /// - "OfficeOwner"
        /// - "Admin"
        /// - "SuperAdmin"
        /// </remarks>
        /// <param name="dto">The user’s registration details.</param>
        /// <returns>
        /// Returns a <see cref="RegisterResponseDto"/> indicating that an OTP was sent.
        /// </returns>
        /// <response code="200">Registration accepted; OTP has been sent.</response>
        /// <response code="400">Invalid request (validation errors).</response>
        /// <response code="409">Email already registered.</response>

        // 1) Register → send registration OTP
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            var email = dto.Email?.Trim().ToLowerInvariant();
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var allowed = new[] {
                Roles.Artisan, Roles.OfficeOwner,
                Roles.Admin, Roles.SuperAdmin
            };
            if (!allowed.Contains(dto.Role))
                return BadRequest(new { Error = $"Role must be one of: {string.Join(", ", allowed)}" });

            if (await _userManager.FindByEmailAsync(email) != null)
                return Conflict(new { Error = "Email already registered." });

            if (dto.Password != dto.ConfirmPassword)
                return BadRequest(new { Error = "Passwords do not match." });

            var user = new ApplicationUser
            {
                UserName = dto.Email,
                Email = dto.Email
            };
            var create = await _userManager.CreateAsync(user, dto.Password);
            if (!create.Succeeded)
                return BadRequest(create.Errors);

            var roleAdd = await _userManager.AddToRoleAsync(user, dto.Role);
            if (!roleAdd.Succeeded)
                return BadRequest(roleAdd.Errors);

            // Profile stub
            switch (dto.Role)
            {
                case Roles.Artisan: _db.ArtisanProfiles.Add(new ArtisanProfile { UserId = user.Id }); break;
                case Roles.OfficeOwner: _db.OfficeOwnerProfiles.Add(new OfficeOwnerProfile { UserId = user.Id }); break;
                case Roles.Admin: _db.AdminProfiles.Add(new AdminProfile { UserId = user.Id }); break;
                case Roles.SuperAdmin: _db.SuperAdminProfiles.Add(new SuperAdminProfile { UserId = user.Id }); break;
            }
            await _db.SaveChangesAsync();

            await _otpService.GenerateAsync(
                user.Id,
                OtpPurpose.Registration,
                GetFingerprint());

            return Ok(new RegisterResponseDto
            {
                RequiresOtp = true,
                UserId = user.Id,
                Email = user.Email!,
                Role = dto.Role,
                OtpPurpose = OtpPurpose.Registration.ToString(),
                OtpSentAt = DateTime.UtcNow,
                NextStep = "verify-registration"
            });
        }


        /// <summary>
        /// Verifies the registration OTP, confirms the user’s email, and records the device fingerprint.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /api/auth/verify-registration
        ///     {
        ///         "email": "user@example.com",
        ///         "code": "123456"
        ///     }
        ///
        /// This endpoint checks the OTP under the <c>Registration</c> purpose.  
        /// If the OTP is valid and the device fingerprint matches (or is first‐time),  
        /// it marks <c>EmailConfirmed = true</c> and saves the fingerprint.
        /// </remarks>
        /// <param name="dto">The email and OTP code to verify.</param>
        /// <returns>
        /// Returns an <see cref="OtpVerifyResponseDto"/> indicating success or failure,
        /// along with the next step for the client.
        /// </returns>
        /// <response code="200">OTP verified successfully.</response>
        /// <response code="400">Invalid request or OTP.</response>
        /// <response code="403">Device fingerprint mismatch detected.</response>
        // 2) Verify registration OTP → confirm email & fingerprint
        [HttpPost("verify-registration")]
        public async Task<IActionResult> VerifyRegistration([FromBody] OtpVerifyDto dto)
        {
            var email = dto.Email?.Trim().ToLowerInvariant();
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return BadRequest(new { Error = "Invalid user." });

            var fp = GetFingerprint();
            if (user.LastDeviceFingerprint != null && user.LastDeviceFingerprint != fp)
                return StatusCode(StatusCodes.Status403Forbidden, new { Error = "Device change detected." });

            var ok = await _otpService.ValidateAsync(
                user.Id,
                OtpPurpose.Registration,
                dto.Code,
                fp);
            if (!ok)
                return BadRequest(new { Error = "Invalid or expired OTP." });

            user.EmailConfirmed = true;
            user.LastDeviceFingerprint = fp;
            await _userManager.UpdateAsync(user);

            var role = (await _userManager.GetRolesAsync(user)).FirstOrDefault() ?? "";

            return Ok(new OtpVerifyResponseDto
            {
                Verified = true,
                UserId = user.Id,
                Email = user.Email!,
                Role = role,
                Message = "Registration verified. Please log in.",
                NextStep = "login"
            });
        }

        // 3a) Verify new‐device OTP → persist fingerprint
        /// <summary>
        /// Verifies the OTP for a new device login and records the device fingerprint.
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     POST /api/auth/verify-device
        ///     {
        ///         "email": "user@example.com",
        ///         "code": "123456"
        ///     }
        ///
        /// This endpoint checks the OTP under the <c>NewDevice</c> purpose.  
        /// On success, the user’s <c>LastDeviceFingerprint</c> is updated so future logins from this device are recognized.
        /// </remarks>
        /// <param name="dto">Contains the user’s email and the OTP code sent for new‑device verification.</param>
        /// <returns>
        /// Returns a <see cref="VerifyDeviceResponseDto"/> indicating success and the next step.
        /// </returns>
        /// <response code="200">OTP verified; device fingerprint recorded.</response>
        /// <response code="400">Invalid request or OTP.</response>
        [HttpPost("verify-device")]
        public async Task<IActionResult> VerifyDevice([FromBody] OtpVerifyDto dto)
        {

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                return BadRequest(new { Error = "Invalid user." });

            var fp = GetFingerprint();
            var ok = await _otpService.ValidateAsync(
                user.Id,
                OtpPurpose.NewDevice,
                dto.Code,
                fp);
            if (!ok)
                return BadRequest(new { Error = "Invalid or expired OTP." });

            user.LastDeviceFingerprint = fp;
            await _userManager.UpdateAsync(user);

            return Ok(new VerifyDeviceResponseDto
            {
                Verified = true,
                Message = "Device verified. Please log in.",
                NextStep = "login"
            });
        }

        // 3b) Verify forgot‐password OTP → issue reset token
        /// <summary>
        /// Verifies the OTP for a forgot‑password request and issues a password reset token.
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     POST /api/auth/verify-forgot
        ///     {
        ///         "email": "user@example.com",
        ///         "code": "123456"
        ///     }
        ///
        /// This endpoint validates the OTP under the <c>ForgotPassword</c> purpose.  
        /// On success, it returns a short‑lived reset token which the client uses with the /reset-password endpoint.
        /// </remarks>
        /// <param name="dto">Contains the user’s email and the OTP code sent for password reset.</param>
        /// <returns>
        /// Returns a <see cref="VerifyForgotResponseDto"/> containing the reset token and the next step.
        /// </returns>
        /// <response code="200">OTP verified; reset token issued.</response>
        /// <response code="400">Invalid request or OTP.</response>
        [HttpPost("verify-forgot")]
        public async Task<IActionResult> VerifyForgot([FromBody] OtpVerifyDto dto)
        {
            var email = dto.Email?.Trim().ToLowerInvariant();
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return BadRequest(new { Error = "Invalid user." });

            var fp = GetFingerprint();
            var ok = await _otpService.ValidateAsync(
                user.Id,
                OtpPurpose.ForgotPassword,
                dto.Code,
                fp);
            if (!ok)
                return BadRequest(new { Error = "Invalid or expired OTP." });

            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            return Ok(new VerifyForgotResponseDto
            {
                Verified = true,
                ResetToken = resetToken,
                Message = "OTP verified. Use token to reset password.",
                NextStep = "reset-password"
            });
        }

        // 4) Login → new‐device OTP or JWT
        /// <summary>
        /// Authenticates a user. If the device is unrecognized, sends a new‐device OTP instead of issuing a token.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /api/auth/login
        ///     {
        ///         "email": "user@example.com",
        ///         "password": "P@ssw0rd!"
        ///     }
        ///
        /// If the user logs in from a new device, an OTP will be sent and the response will indicate the next step.
        /// Otherwise, a JWT will be returned in the response.
        /// </remarks>
        /// <param name="dto">The user credentials for login.</param>
        /// <returns>
        /// <see cref="LoginResponseDto"/> containing either OTP instructions or the JWT and user details.
        /// </returns>
        /// <response code="200">
        /// Returns a <see cref="LoginResponseDto"/> indicating success (with token) or OTP required.
        /// </response>
        /// <response code="400">Request validation failed.</response>
        /// <response code="401">Invalid credentials.</response>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                return Unauthorized(new { Authenticated = false, Message = "Invalid credentials." });

            var fp = GetFingerprint();
            if (user.LastDeviceFingerprint != null && user.LastDeviceFingerprint != fp)
            {
                await _otpService.GenerateAsync(user.Id, OtpPurpose.NewDevice, fp);
                return Ok(new LoginResponseDto
                {
                    RequiresOtp = true,
                    OtpPurpose = OtpPurpose.NewDevice.ToString(),
                    Authenticated = false,
                    Message = "New device detected. OTP sent.",
                    NextStep = "verify-device"
                });
            }

            var signIn = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, false);
            if (!signIn.Succeeded)
                return Unauthorized(new { Authenticated = false, Message = "Invalid credentials." });

            var token = await GenerateJwt(user);
            var expiresMin = int.Parse(_config["Jwt:ExpireMinutes"]!);
            var expiresAt = DateTime.UtcNow.AddMinutes(expiresMin).ToString("o");
            var role = (await _userManager.GetRolesAsync(user)).FirstOrDefault() ?? "";

            return Ok(new LoginResponseDto
            {
                RequiresOtp = false,
                Authenticated = true,
                Token = token,
                TokenExpiresAt = expiresAt,
                UserId = user.Id,
                Email = user.Email!,
                Role = role,
                Message = "Login successful.",
                NextStep = "use-token"
            });
        }

        // 5) Forgot‐password → send OTP
        /// <summary>
        /// Sends an OTP for a password reset request without revealing if the email exists.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /api/auth/forgot-password
        ///     {
        ///         "email": "user@example.com"
        ///     }
        ///
        /// This endpoint will always return a 200 response with instructions,  
        /// ensuring attackers cannot probe for valid email addresses.
        /// </remarks>
        /// <param name="dto">The user’s email for which to send the reset OTP.</param>
        /// <returns>
        /// Returns a <see cref="ForgotPasswordResponseDto"/> indicating that an OTP has been sent if the account exists.
        /// </returns>
        /// <response code="200">OTP request accepted; check your email.</response>
        /// <response code="400">Invalid request data.</response>
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user != null)
                await _otpService.GenerateAsync(user.Id, OtpPurpose.ForgotPassword, GetFingerprint());

            return Ok(new ForgotPasswordResponseDto
            {
                RequiresOtp = true,
                Message = "If your email exists, OTP has been sent.",
                NextStep = "verify-forgot"
            });
        }

        // 6) Reset password
        /// <summary>
        /// Resets a user’s password using a valid OTP-issued reset token or generates one if missing.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /api/auth/reset-password
        ///     {
        ///         "email": "user@example.com",
        ///         "newPassword": "NewP@ssw0rd!",
        ///         "resetToken": "optional-reset-token"
        ///     }
        ///
        /// If <c>resetToken</c> is provided (from <c>verify-forgot</c>), it is used;  
        /// otherwise, a fresh Identity password‑reset token is generated and applied.
        /// </remarks>
        /// <param name="dto">Contains the user’s email, desired new password, and optional reset token.</param>
        /// <returns>
        /// Returns a <see cref="GenericResponseDto"/> indicating success or failure.
        /// </returns>
        /// <response code="200">Password reset successful.</response>
        /// <response code="400">Invalid request, user not found, or reset token invalid.</response>
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                return BadRequest(new { Error = "Invalid user." });

            var token = dto.ResetToken
                ?? await _userManager.GeneratePasswordResetTokenAsync(user);
            var res = await _userManager.ResetPasswordAsync(user, token, dto.NewPassword);
            if (!res.Succeeded)
                return BadRequest(res.Errors);

            return Ok(new GenericResponseDto { Message = "Password reset successful." });
        }

        // 7a) Send OTP to change password
        /// <summary>
        /// Sends an OTP to the user’s email to confirm a password change request.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /api/auth/send-change-otp
        ///     (Authorization: Bearer &lt;access_token&gt;)
        ///
        /// This endpoint requires a valid JWT. It generates and sends an OTP under the
        /// <c>ChangePassword</c> purpose for the authenticated user, without returning the code.
        /// </remarks>
        /// <returns>
        /// A <see cref="GenericResponseDto"/> confirming that the OTP was sent.
        /// </returns>
        /// <response code="200">OTP generation succeeded; check your email.</response>
        /// <response code="401">Authentication failed or missing.</response>
        //[Authorize]
        //[HttpPost("send-change-otp")]
        //public async Task<IActionResult> SendChangeOtp()
        //{
        //    var user = await _userManager.GetUserAsync(User)!;
        //    await _otpService.GenerateAsync(user.Id, OtpPurpose.ChangePassword, GetFingerprint());
        //    return Ok(new GenericResponseDto
        //    {
        //        Message = "OTP for password change sent to your email."
        //    });
        //}

        // 7b) Change password
        /// <summary>
        /// Changes the authenticated user’s password after verifying their current password.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /api/auth/change-password
        ///     {
        ///         "currentPassword": "OldP@ssw0rd!",
        ///         "newPassword": "NewP@ssw0rd!"
        ///     }
        ///
        /// This endpoint requires a valid JWT. It will return an error if the current password is incorrect.
        /// </remarks>
        /// <param name="dto">Contains the user’s current and new passwords.</param>
        /// <returns>
        /// A <see cref="GenericResponseDto"/> indicating whether the password change was successful.
        /// </returns>
        /// <response code="200">Password changed successfully.</response>
        /// <response code="400">Invalid request data or current password incorrect.</response>
        /// <response code="401">Authentication failed or missing.</response>
        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangeDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.GetUserAsync(User)!;
            var res = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
            if (!res.Succeeded)
                return BadRequest(res.Errors);

            return Ok(new GenericResponseDto { Message = "Password changed successfully." });
        }

        // 8) Current user profile
        /// <summary>
        /// Retrieves the profile of the currently authenticated user.
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     GET /api/auth/me
        ///     Authorization: Bearer &lt;access_token&gt;
        /// </remarks>
        /// <returns>
        /// A <see cref="UserProfileDto"/> containing the user’s ID, email, and role.
        /// </returns>
        /// <response code="200">Returns the current user’s profile.</response>
        /// <response code="401">Unauthorized if no valid JWT is provided.</response>
        //[Authorize]
        //[HttpGet("me")]
        //public async Task<ActionResult<UserProfileDto>> Me()
        //{
        //    var user = await _userManager.GetUserAsync(User);
        //    var role = (await _userManager.GetRolesAsync(user)).FirstOrDefault() ?? "";
        //    return Ok(new UserProfileDto
        //    {
        //        UserId = user.Id,
        //        Email = user.Email!,
        //        Role = role
        //    });
        //}

        // 9) Resend OTP for any flow
        /// <summary>
        /// Resend an OTP code for verification flows (e.g., registration, forgot password, or new device login).
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     POST /api/auth/resend-otp
        ///     {
        ///         "email": "user@example.com",
        ///         "whyResentOtp": "forgot"
        ///     }
        ///
        /// Valid values for <c>whyResentOtp</c>:
        /// - "Forgot" for password reset
        /// - "new-device" for login from a new device
        /// - "registration" for login from a new device
        /// - any other value will default to registration
        /// </remarks>
        /// <param name="dto">The email and reason for resending OTP.</param>
        /// <returns>Returns a message indicating that the OTP was resent and the next step for the user.</returns>
        /// <response code="200">OTP resent successfully</response>
        /// <response code="400">Invalid request</response>
        [HttpPost("resend-otp")]
        public async Task<IActionResult> ResendOtp([FromBody] ResendOtpRequestDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user != null)
            {
                var purpose = dto.WhyResentOtp switch
                {
                    "forgot" => OtpPurpose.ForgotPassword,
                    "new-device" => OtpPurpose.NewDevice,
                    _ => OtpPurpose.Registration
                };
                await _otpService.GenerateAsync(user.Id, purpose, GetFingerprint());
            }
            return Ok(new OluBackendApp.DTOs.ResendOtpResponseDto
            {
                RequiresOtp = true,
                NextStep = $"Otp Code Sent. verify-{dto.WhyResentOtp}"
            });
        }

        //// 10) Refresh token
        //[HttpPost("refresh-token")]
        //public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto dto)
        //{
        //    if (!ModelState.IsValid)
        //        return BadRequest(ModelState);

        //    var principal = _tokenService.GetPrincipalFromExpiredToken(dto.RefreshToken);
        //    var userId = principal.FindFirstValue(JwtRegisteredClaimNames.Sub);
        //    if (userId == null)
        //        return Unauthorized();

        //    var user = await _userManager.FindByIdAsync(userId);
        //    if (user == null || !await _refreshTokenService.ValidateRefreshTokenAsync(userId, dto.RefreshToken))
        //        return Unauthorized();

        //    var newJwt = await GenerateJwt(user);
        //    var newRt = _refreshTokenService.GenerateRefreshToken();
        //    await _refreshTokenService.SaveRefreshTokenAsync(userId, newRt);

        //    var expiresMin = int.Parse(_config["Jwt:ExpireMinutes"]!);
        //    var expiresAt = DateTime.UtcNow.AddMinutes(expiresMin).ToString("o");

        //    return Ok(new RefreshTokenResponseDto
        //    {
        //        Token = newJwt,
        //        ExpiresAt = expiresAt,
        //        RefreshToken = newRt
        //    });
        //}

        // 11) Logout
        /// <summary>
        /// Logs out the current user by invalidating all existing tokens.
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///
        ///     POST /api/auth/logout
        ///     Authorization: Bearer &lt;access_token&gt;
        ///
        /// This endpoint bumps the user’s security stamp so that any previously issued JWTs
        /// and refresh tokens become invalid. The user will need to log in again to obtain new tokens.
        /// </remarks>
        /// <returns>
        /// A <see cref="LogoutResponseDto"/> confirming the logout.
        /// </returns>
        /// <response code="200">Logout successful.</response>
        /// <response code="401">Unauthorized if no valid JWT is provided.</response>
        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var user = await _userManager.GetUserAsync(User)!;
            await _userManager.UpdateSecurityStampAsync(user);
            return Ok(new LogoutResponseDto { Message = "Logged out successfully." });
        }

        // — Helpers —

        private string GetFingerprint()
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "";
            Request.Headers.TryGetValue("User-Agent", out var ua);
            var raw = $"{ip}|{ua}";
            var hash = SHA256.HashData(Encoding.UTF8.GetBytes(raw));
            return Convert.ToHexString(hash);
        }

        //private async Task<string> GenerateJwt(ApplicationUser user)
        //{
        //    var section = _config.GetSection("Jwt");
        //    var keyRaw = section["Key"];
        //    var issuer = section["Issuer"];
        //    var audience = section["Audience"];
        //    var expireRaw = section["ExpireMinutes"];

        //    if (string.IsNullOrWhiteSpace(keyRaw)
        //     || string.IsNullOrWhiteSpace(issuer)
        //     || string.IsNullOrWhiteSpace(audience)
        //     || !int.TryParse(expireRaw, out var mins))
        //    {
        //        throw new InvalidOperationException(
        //            "Invalid JWT config. Ensure Key, Issuer, Audience, ExpireMinutes are set.");
        //    }

        //    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyRaw));
        //    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        //    var roles = await _userManager.GetRolesAsync(user);

        //    var claims = new List<Claim> {
        //        new(JwtRegisteredClaimNames.Sub,   user.Id),
        //        new(JwtRegisteredClaimNames.Email, user.Email!)
        //    };
        //    claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        //    var token = new JwtSecurityToken(
        //        issuer: issuer,
        //        audience: audience,
        //        claims: claims,
        //        expires: DateTime.UtcNow.AddMinutes(mins),
        //        signingCredentials: creds);

        //    return new JwtSecurityTokenHandler().WriteToken(token);
        //}

        //        private async Task<string> GenerateJwt(ApplicationUser user)
        //        {
        //            var section = _config.GetSection("Jwt");
        //            var keyRaw = section["Key"];
        //            var issuer = section["Issuer"];
        //            var audience = section["Audience"];
        //            var expireRaw = section["ExpireMinutes"];

        //            if (string.IsNullOrWhiteSpace(keyRaw)
        //             || string.IsNullOrWhiteSpace(issuer)
        //             || string.IsNullOrWhiteSpace(audience)
        //             || !int.TryParse(expireRaw, out var mins))
        //            {
        //                throw new InvalidOperationException(
        //                    "Invalid JWT config. Ensure Key, Issuer, Audience, ExpireMinutes are set.");
        //            }

        //            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyRaw));
        //            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        //            var roles = await _userManager.GetRolesAsync(user);

        //            //        var claims = new List<Claim> {
        //            //    new(JwtRegisteredClaimNames.Sub, user.Id),
        //            //    new(ClaimTypes.NameIdentifier, user.Id), // ✅ Required for UserManager
        //            //    new(JwtRegisteredClaimNames.Email, user.Email!)

        //            //};
        //            var claims = new List<Claim>
        //{
        //            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
        //            new Claim(ClaimTypes.NameIdentifier, user.Id),   // required by UserManager
        //            new Claim(JwtRegisteredClaimNames.Email, user.Email!),
        //            new Claim(ClaimTypes.Role, userRole)             // ✅ THIS IS MISSING
        //        };
        //            claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        //            var token = new JwtSecurityToken(
        //                issuer: issuer,
        //                audience: audience,
        //                claims: claims,
        //                expires: DateTime.UtcNow.AddMinutes(mins),
        //                signingCredentials: creds);

        //            return new JwtSecurityTokenHandler().WriteToken(token);
        //        }

        private async Task<string> GenerateJwt(ApplicationUser user)
        {
            var section = _config.GetSection("Jwt");
            var keyRaw = section["Key"];
            var issuer = section["Issuer"];
            var audience = section["Audience"];
            var expireRaw = section["ExpireMinutes"];

            if (string.IsNullOrWhiteSpace(keyRaw)
                || string.IsNullOrWhiteSpace(issuer)
                || string.IsNullOrWhiteSpace(audience)
                || !int.TryParse(expireRaw, out var mins))
            {
                throw new InvalidOperationException(
                    "Invalid JWT config. Ensure Key, Issuer, Audience, ExpireMinutes are set.");
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyRaw));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var roles = await _userManager.GetRolesAsync(user);
            var userRole = roles.FirstOrDefault() ?? throw new Exception("User has no role assigned.");

            var claims = new List<Claim>
    {
        new Claim(JwtRegisteredClaimNames.Sub, user.Id),
        new Claim(ClaimTypes.NameIdentifier, user.Id), // Required by UserManager
        new Claim(JwtRegisteredClaimNames.Email, user.Email!),
        new Claim(ClaimTypes.Role, userRole)           // ✅ Role claim for authorization
    };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(mins),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }


    }
}
