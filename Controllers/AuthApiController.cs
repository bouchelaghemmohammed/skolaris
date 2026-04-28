using Microsoft.AspNetCore.Mvc;
using Skolaris.Services;

namespace Skolaris.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthApiController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthApiController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            var user = _authService.Login(request.Email, request.Password);

            if (user == null)
                return Unauthorized();

            return Ok(new
            {
                Id = user.IdUtilisateur,
                Nom = $"{user.Prenom} {user.Nom}",
                user.Email,
                Role = user.Role.ToString()
            });
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email))
                return BadRequest("Email requis.");

            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var result = await _authService.ForgotPasswordAsync(request.Email, baseUrl);

            return Ok(new
            {
                result.Success,
                result.EmailSent,
                result.DevToken
            });
        }

        [HttpPost("reset-password")]
        public IActionResult ResetPassword([FromBody] ResetPasswordRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Token) || string.IsNullOrWhiteSpace(request.NewPassword))
                return BadRequest("Token et nouveau mot de passe requis.");

            var success = _authService.ResetPassword(request.Token, request.NewPassword);

            if (!success)
                return BadRequest("Token invalide ou expiré.");

            return Ok(new { Success = true });
        }
    }

    public class LoginRequest
    {
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
    }

    public class ForgotPasswordRequest
    {
        public string Email { get; set; } = "";
    }

    public class ResetPasswordRequest
    {
        public string Token { get; set; } = "";
        public string NewPassword { get; set; } = "";
    }
}
