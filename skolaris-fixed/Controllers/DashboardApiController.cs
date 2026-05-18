using Microsoft.AspNetCore.Mvc;
using Skolaris.Services;

namespace Skolaris.Controllers
{
    [ApiController]
    [Route("api/dashboard")]
    public class DashboardApiController : ControllerBase
    {
        private readonly DashboardService _dashboardService;

        public DashboardApiController(DashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet("admin-stats")]
        public IActionResult GetAdminStats()
        {
            return Ok(_dashboardService.GetAdminStats());
        }
    }
}
