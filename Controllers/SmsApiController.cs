using Microsoft.AspNetCore.Mvc;
using Skolaris.Services;

namespace Skolaris.Controllers
{
    [ApiController]
    [Route("api/sms")]
    public class SmsApiController : ControllerBase
    {
        private readonly SmsService _smsService;

        public SmsApiController(SmsService smsService)
        {
            _smsService = smsService;
        }

        // POST api/sms/envoyer
        // Body: { "telephones": ["+33612345678", "+1514..."], "message": "Urgence école..." }
        [HttpPost("envoyer")]
        public async Task<IActionResult> Envoyer([FromBody] EnvoiSmsRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Message))
                return BadRequest("Le message ne peut pas être vide.");

            if (request.Telephones == null || request.Telephones.Count == 0)
                return BadRequest("Aucun numéro de téléphone fourni.");

            if (request.Message.Length > 640)
                return BadRequest("Message trop long (maximum 640 caractères).");

            var results = await _smsService.SendBulkSmsAsync(request.Telephones, request.Message);

            return Ok(results);
        }
    }

    public class EnvoiSmsRequest
    {
        public List<string> Telephones { get; set; } = new();
        public string Message { get; set; } = "";
    }
}
