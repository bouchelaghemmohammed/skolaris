using Microsoft.AspNetCore.Mvc;
using Skolaris.Services;

namespace Skolaris.Controllers
{
    [ApiController]
    [Route("api/notifications")]
    public class NotificationsApiController : ControllerBase
    {
        private readonly NotificationService _service;

        public NotificationsApiController(NotificationService service)
        {
            _service = service;
        }

        [HttpGet("{userId}")]
        public IActionResult GetNotifications(int userId)
        {
            return Ok(_service.GetNotifications(userId));
        }

        [HttpGet("{userId}/nonlu")]
        public IActionResult GetNonLu(int userId)
        {
            return Ok(new { Count = _service.GetNonLuCount(userId) });
        }

        [HttpPut("{id}/lue")]
        public IActionResult MarquerLue(int id)
        {
            _service.MarquerCommeLue(id);
            return Ok();
        }

        [HttpPut("{userId}/lue-tout")]
        public IActionResult MarquerToutLue(int userId)
        {
            _service.MarquerToutesCommeLues(userId);
            return Ok();
        }
    }
}
