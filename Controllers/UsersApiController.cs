using Microsoft.AspNetCore.Mvc;
using Skolaris.Services;

namespace Skolaris.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UsersApiController : ControllerBase
    {
        private readonly UserService _userService;

        public UsersApiController(UserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public IActionResult GetUsers()
        {
            return Ok(_userService.GetAllUsers());
        }

        [HttpPost]
        public IActionResult CreateUser([FromBody] CreateUserRequest request)
        {
            var (success, error) = _userService.CreateUser(
                request.Prenom, request.Nom, request.Email, request.Role, request.MotDePasse);
            if (!success)
                return BadRequest(error);
            return Ok();
        }

        [HttpPost("{id}/toggle-active")]
        public IActionResult ToggleActive(int id)
        {
            var result = _userService.ToggleActive(id, 0);

            if (!result)
                return BadRequest("Action impossible.");

            return Ok();
        }

        [HttpPost("{id}/change-role")]
        public IActionResult ChangeRole(int id, [FromBody] ChangeRoleRequest request)
        {
            var result = _userService.ChangeRole(id, request.Role);

            if (!result)
                return BadRequest("R�le invalide.");

            return Ok();
        }
    }

    public class ChangeRoleRequest
    {
        public string Role { get; set; } = "";
    }

    public class CreateUserRequest
    {
        public string Prenom { get; set; } = "";
        public string Nom { get; set; } = "";
        public string Email { get; set; } = "";
        public string Role { get; set; } = "ELEVE";
        public string MotDePasse { get; set; } = "";
    }
}
