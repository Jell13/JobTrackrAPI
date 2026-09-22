using JobTrackrAPI.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace JobTrackrAPI.Controllers
{
    [Route("api/applications")]
    [ApiController]
    [Authorize]
    public class ApplicationsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ApplicationsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllApplications()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if(userIdClaim == null)
            {
                return Unauthorized();
            }

            int userId;
            try
            {
                userId = int.Parse(userIdClaim);
            }
            catch (FormatException)
            {
                return Unauthorized();
            }
            var applications = await _context.Applications.Where(s => s.UserId == userId).ToListAsync();

            return Ok(applications);
        
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetApplicationByAppId(int id)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            int userId;
            try
            {
                userId = int.Parse(userIdClaim);
            }
            catch (FormatException)
            {
                return Unauthorized();
            }
            var application = await _context.Applications.Where(s => s.Id == id).Where(s => s.UserId == userId).FirstOrDefaultAsync();
            
            if (application == null)
            {
                return NotFound();
            }

            return Ok(application);
        }

        public record CreateApplicationRequest(string Company, string Role, string Status, string Description, DateTime AppliedDate);

        [HttpPost]
        public async Task<IActionResult> CreateApplication(CreateApplicationRequest application)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            int userId;
            try
            {
                userId = int.Parse(userIdClaim);
            }
            catch (FormatException)
            {
                return Unauthorized();
            }

            var newApplication = new Models.Application{

                Company = application.Company,
                Role = application.Role,
                Status = application.Status,
                Description = application.Description,
                AppliedDate = DateTime.SpecifyKind(application.AppliedDate, DateTimeKind.Utc),
                UserId = userId
            };

            _context.Applications.Add(newApplication);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetApplicationByAppId), new { id = newApplication.Id }, newApplication);
        }

        public record UpdateStatusRequest(string Status);
           
        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateApplicationStatus(int id, UpdateStatusRequest status)
        {
            var userIdClaims = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if(userIdClaims == null)
            {
                return Unauthorized();
            }
            int userId;
            try
            {
                userId = int.Parse(userIdClaims);
            }
            catch (FormatException)
            {
                return Unauthorized();
            }

            var application = await _context.Applications.Where(s => s.Id == id).Where(s => s.UserId == userId).FirstOrDefaultAsync();
            if(application == null)
            {
                return NotFound();
            }

            application.Status = status.Status;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteApplication(int id)
        {
            var userIdClaims = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if(userIdClaims == null)
            {
                return Unauthorized();
            }
            int userId;
            try
            {
                userId = int.Parse(userIdClaims);
            }
            catch (FormatException)
            {
                return Unauthorized();
            }
            var application = await _context.Applications.Where(s => s.Id == id).Where(s => s.UserId == userId).FirstOrDefaultAsync();
            if(application == null)
            {
                return NotFound();
            }

            _context.Applications.Remove(application);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
