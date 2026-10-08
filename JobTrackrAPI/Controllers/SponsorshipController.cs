using JobTrackrAPI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace JobTrackrAPI.Controllers
{
    [Route("api/sponsorship")]
    [ApiController]
    public class SponsorshipController : ControllerBase
    {
        private readonly SponsorshipService _sponsorshipService;

        public SponsorshipController(SponsorshipService sponsorshipService)
        {
            _sponsorshipService = sponsorshipService;
        }

        [HttpGet("{employerName}")]
        public async Task<IActionResult> GetInsight(string employerName)
        {
            var insight = await _sponsorshipService.GetSponsorshipInsightAsync(employerName);
            return Ok(new { employerName, insight });
        }   
    }
}
