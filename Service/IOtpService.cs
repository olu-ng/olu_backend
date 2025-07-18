//using OluBackendApp.Data;
//using OluBackendApp.Models;
//using OluBackendApp.Services;
//using System.Security.Cryptography;
//using Microsoft.EntityFrameworkCore;
//using System.Text;

//namespace OluBackendApp.Services
//{
//    public interface IOtpService
//    {
//        Task<string> GenerateAsync(string userId, OtpPurpose purpose, string? deviceFingerprint = null);
//        Task<bool> ValidateAsync(string userId, OtpPurpose purpose, string code, string? deviceFingerprint = null);
//    }

//    public class OtpService : IOtpService
//    {
//        private readonly ApplicationDbContext _db;
//        private readonly IEmailService _email;
//        public OtpService(ApplicationDbContext db, IEmailService email) { _db = db; _email = email; }

//        public async Task<string> GenerateAsync(string userId, OtpPurpose purpose, string? deviceFingerprint = null)
//        {
//            // 6-digit code
//            var code = RandomNumberGenerator.GetInt32(100000, 999999).ToString();
//            var record = new OtpRecord
//            {
//                UserId = userId,
//                Code = code,
//                Purpose = purpose,
//                ExpiresAt = DateTime.UtcNow.AddMinutes(10),
//                DeviceFingerprint = deviceFingerprint
//            };
//            _db.OtpRecords.Add(record);
//            await _db.SaveChangesAsync();

//            // send email
//            await _email.SendAsync(
//                to: (await _db.Users.FindAsync(userId))!.Email!,
//                subject: $"Your OTP for {purpose}",
//                html: $"<p>Your code is <strong>{code}</strong>. It expires in 10 minutes.</p>");

//            return code;
//        }

//        public async Task<bool> ValidateAsync(string userId, OtpPurpose purpose, string code, string? deviceFingerprint = null)
//        {
//            var rec = await _db.OtpRecords
//                .Where(o => o.UserId == userId && o.Purpose == purpose && !o.Used)
//                .OrderByDescending(o => o.Id)
//                .FirstOrDefaultAsync();

//            if (rec == null || rec.Code != code || rec.ExpiresAt < DateTime.UtcNow)
//                return false;

//            if (purpose == OtpPurpose.NewDevice && rec.DeviceFingerprint != deviceFingerprint)
//                return false;

//            rec.Used = true;
//            await _db.SaveChangesAsync();
//            return true;
//        }
//    }
//}





using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OluBackendApp.Data;
using OluBackendApp.Models;

namespace OluBackendApp.Services
{
    public interface IOtpService
    {
        Task<string> GenerateAsync(string userId, OtpPurpose purpose, string? deviceFingerprint = null);
        Task<bool> ValidateAsync(string userId, OtpPurpose purpose, string code, string? deviceFingerprint = null);
    }

    public class OtpService : IOtpService
    {
        private readonly ApplicationDbContext _db;
        private readonly IEmailService _email;

        // In-memory logs for demo (swap to Redis for prod)
        private static readonly ConcurrentDictionary<string, DateTime[]> _genLog = new();
        private static readonly ConcurrentDictionary<string, (int Fails, DateTime? LockoutEnd)> _failLog = new();

        public OtpService(ApplicationDbContext db, IEmailService email)
        {
            _db = db;
            _email = email;
        }

        public async Task<string> GenerateAsync(string userId, OtpPurpose purpose, string? deviceFingerprint = null)
        {
            var key = $"{userId}:{purpose}";
            var now = DateTime.UtcNow;

            // — Rate‑limit: max 3 per hour
            _genLog.AddOrUpdate(key,
                _ => new[] { now },
                (_, arr) =>
                {
                    var recent = arr.Where(t => t > now.AddHours(-1)).Append(now).ToArray();
                    if (recent.Length > 3)
                        throw new InvalidOperationException("Too many OTP requests. Try again later.");
                    return recent;
                });

            // create OTP
            var code = RandomNumberGenerator.GetInt32(100000, 999999).ToString();
            var rec = new OtpRecord
            {
                UserId = userId,
                Code = code,
                Purpose = purpose,
                DeviceFingerprint = deviceFingerprint,
                ExpiresAt = now.AddMinutes(10),
                Used = false
            };
            _db.OtpRecords.Add(rec);
            await _db.SaveChangesAsync();

            // send via email
            var user = await _db.Users.FindAsync(userId);
            if (user != null)
            {
                await _email.SendAsync(
                    user.Email!,
                    $"Your OTP for {purpose}",
                    $"<p>Your code is <strong>{code}</strong>. Expires in 10 minutes.</p>");
            }

            return code;
        }

        public async Task<bool> ValidateAsync(string userId, OtpPurpose purpose, string code, string? deviceFingerprint = null)
        {
            var key = $"{userId}:{purpose}";
            var now = DateTime.UtcNow;

            // — Brute‑force: lock out after 5 fails
            if (_failLog.TryGetValue(key, out var info) && info.LockoutEnd > now)
                throw new InvalidOperationException("Too many failed attempts. Try again later.");

            // find latest valid record
            var rec = await _db.OtpRecords
                .Where(o => o.UserId == userId && o.Purpose == purpose && !o.Used)
                .OrderByDescending(o => o.CreatedAt)
                .FirstOrDefaultAsync();

            var success = rec != null
                && rec.Code == code
                && rec.ExpiresAt >= now
                && (purpose != OtpPurpose.NewDevice || rec.DeviceFingerprint == deviceFingerprint);

            if (!success)
            {
                // increment fail count
                var fails = info.Fails + 1;
                var lockoutEnd = fails >= 5 ? now.AddMinutes(15) : info.LockoutEnd;
                _failLog[key] = (fails, lockoutEnd);
                return false;
            }

            // mark used & clear failures
            rec.Used = true;
            await _db.SaveChangesAsync();
            _failLog.TryRemove(key, out _);
            return true;
        }
    }
}
