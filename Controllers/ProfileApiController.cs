using Microsoft.AspNetCore.Mvc;
using Skolaris.Services;

namespace Skolaris.Controllers
{
    [ApiController]
    [Route("api/profile")]
    public class ProfileApiController : ControllerBase
    {
        private readonly UserService _userService;

        public ProfileApiController(UserService userService)
        {
            _userService = userService;
        }

        // PUT: api/profile/{id} — ADM-11
        [HttpPut("{id}")]
        public IActionResult UpdateProfile(int id, [FromBody] UpdateProfileRequest request)
        {
            var result = _userService.UpdateProfile(id, request.Prenom, request.Nom, request.Email);
            if (!result)
                return BadRequest("Mise à jour impossible (utilisateur introuvable ou email invalide).");
            return Ok();
        }
    }

    public class UpdateProfileRequest
    {
        public string Prenom { get; set; } = "";
        public string Nom { get; set; } = "";
        public string Email { get; set; } = "";
    }
}
