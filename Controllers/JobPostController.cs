using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OluBackendApp.Data;
using OluBackendApp.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace OluBackendApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "OfficeOwner")]
    public class JobPostController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public JobPostController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ───────────────────────────────
        // POST: api/JobPost
        [HttpPost]
        public async Task<IActionResult> CreateJob([FromBody] JobPost jobPost)
        {
            var userId = _userManager.GetUserId(User); // string ID

            var ownerProfile = await _context.OfficeOwnerProfiles
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (ownerProfile == null)
                return Unauthorized("OfficeOwner profile not found.");

            jobPost.CreatedAt = DateTime.UtcNow;
            jobPost.OfficeOwnerProfileId = ownerProfile.Id;

            _context.JobPosts.Add(jobPost);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetJobById), new { id = jobPost.Id }, jobPost);
        }

        // ───────────────────────────────
        // GET: api/JobPost
        [HttpGet]
        public async Task<IActionResult> GetMyJobs()
        {
            var userId = _userManager.GetUserId(User);

            var ownerProfile = await _context.OfficeOwnerProfiles
                .FirstOrDefaultAsync(p => p.UserId == userId);

            if (ownerProfile == null)
                return Unauthorized("OfficeOwner profile not found.");

            var jobs = await _context.JobPosts
                .Where(j => j.OfficeOwnerProfileId == ownerProfile.Id)
                .ToListAsync();

            return Ok(jobs);
        }

        // ───────────────────────────────
        // GET: api/JobPost/{id}
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetJobById(int id)
        {
            var userId = _userManager.GetUserId(User);

            var job = await _context.JobPosts
                .Include(j => j.OfficeOwnerProfile)
                .FirstOrDefaultAsync(j => j.Id == id && j.OfficeOwnerProfile.UserId == userId);

            if (job == null)
                return NotFound("Job not found or not owned by you.");

            return Ok(job);
        }

        // ───────────────────────────────
        // PUT: api/JobPost/{id}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateJob(int id, [FromBody] JobPost updatedJob)
        {
            var userId = _userManager.GetUserId(User);

            var job = await _context.JobPosts
                .Include(j => j.OfficeOwnerProfile)
                .FirstOrDefaultAsync(j => j.Id == id && j.OfficeOwnerProfile.UserId == userId);

            if (job == null)
                return NotFound("Job not found or not owned by you.");

            job.Title = updatedJob.Title;
            job.Description = updatedJob.Description;
            job.Budget = updatedJob.Budget;
            job.Location = updatedJob.Location;

            await _context.SaveChangesAsync();
            return Ok(job);
        }

        // ───────────────────────────────
        // DELETE: api/JobPost/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteJob(int id)
        {
            var userId = _userManager.GetUserId(User);

            var job = await _context.JobPosts
                .Include(j => j.OfficeOwnerProfile)
                .FirstOrDefaultAsync(j => j.Id == id && j.OfficeOwnerProfile.UserId == userId);

            if (job == null)
                return NotFound("Job not found or not owned by you.");

            _context.JobPosts.Remove(job);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
