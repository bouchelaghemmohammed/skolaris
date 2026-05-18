using Microsoft.AspNetCore.Mvc;
using Skolaris.Enums;
using Skolaris.Services;

namespace Skolaris.Controllers
{
    [ApiController]
    [Route("api/messagerie")]
    public class MessagerieApiController : ControllerBase
    {
        private readonly MessagerieService _service;
        private readonly IWebHostEnvironment _env;

        public MessagerieApiController(MessagerieService service, IWebHostEnvironment env)
        {
            _service = service;
            _env = env;
        }

        // GET api/messagerie/conversations/{userId}
        [HttpGet("conversations/{userId:int}")]
        public IActionResult GetConversations(int userId)
        {
            return Ok(_service.GetConversations(userId));
        }

        // GET api/messagerie/conversations/{conversationId}/messages/{userId}
        [HttpGet("conversations/{conversationId:int}/messages/{userId:int}")]
        public IActionResult GetMessages(int conversationId, int userId)
        {
            _service.MarquerTousLus(conversationId, userId);
            return Ok(_service.GetMessages(conversationId, userId));
        }

        // GET api/messagerie/nonlu/{userId}
        [HttpGet("nonlu/{userId:int}")]
        public IActionResult GetNonLuCount(int userId)
        {
            return Ok(new { Count = _service.GetNonLuCount(userId) });
        }

        // GET api/messagerie/users/{currentUserId}
        [HttpGet("users/{currentUserId:int}")]
        public IActionResult GetUsers(int currentUserId)
        {
            return Ok(_service.GetUtilisateursActifs(currentUserId));
        }

        // GET api/messagerie/groupes
        [HttpGet("groupes")]
        public IActionResult GetGroupes()
        {
            return Ok(_service.GetGroupes());
        }

        // POST api/messagerie/conversations
        [HttpPost("conversations")]
        public IActionResult CreateConversation([FromBody] CreateConversationRequest req)
        {
            if (req.ParticipantIds == null || !req.ParticipantIds.Any())
                return BadRequest("Au moins un destinataire requis.");

            var type = req.Type == "Groupe" ? TypeConversation.Groupe : TypeConversation.Individuelle;
            var id = _service.CreateConversation(req.CreatorId, req.Sujet ?? "", type, req.ParticipantIds);
            if (id == -1) return Unauthorized("Session expirée, veuillez vous reconnecter.");
            return Ok(new { IdConversation = id });
        }

        // POST api/messagerie/messages
        [HttpPost("messages")]
        public IActionResult SendMessage([FromBody] SendMessageRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Contenu) && string.IsNullOrWhiteSpace(req.PieceJointeNom))
                return BadRequest("Contenu ou pièce jointe requis.");

            var id = _service.SendMessage(req.ConversationId, req.SenderId, req.Contenu ?? "", req.PieceJointePath, req.PieceJointeNom);
            if (id == -1) return Forbid();
            return Ok(new { IdMessage = id });
        }

        // POST api/messagerie/upload
        [HttpPost("upload")]
        [Microsoft.AspNetCore.Mvc.RequestFormLimits(MultipartBodyLengthLimit = 10_485_760)]
        [Microsoft.AspNetCore.Mvc.DisableRequestSizeLimit]
        public async Task<IActionResult> Upload([FromForm] IFormFile file, [FromForm] int userId)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Fichier requis.");

            if (file.Length > 10 * 1024 * 1024)
                return BadRequest("Taille maximale 10 Mo.");

            var allowedExt = new[] { ".pdf", ".png", ".jpg", ".jpeg", ".gif", ".webp" };
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExt.Contains(ext))
                return BadRequest("Type de fichier non autorisé.");

            // ContentRootPath est toujours défini (contrairement à WebRootPath qui peut être null)
            var wwwRoot = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
            var uploadDir = Path.Combine(wwwRoot, "uploads", "messagerie");
            Directory.CreateDirectory(uploadDir);

            var safeName = $"{Guid.NewGuid()}{ext}";
            var fullPath = Path.Combine(uploadDir, safeName);

            await using var stream = new FileStream(fullPath, FileMode.Create);
            await file.CopyToAsync(stream);

            return Ok(new { Path = $"/uploads/messagerie/{safeName}", Nom = file.FileName });
        }

        // PUT api/messagerie/messages/{messageId}/lu/{userId}
        [HttpPut("messages/{messageId:int}/lu/{userId:int}")]
        public IActionResult MarquerLu(int messageId, int userId)
        {
            _service.MarquerLu(messageId, userId);
            return Ok();
        }

        // PUT api/messagerie/messages/{messageId}/nonlu/{userId}
        [HttpPut("messages/{messageId:int}/nonlu/{userId:int}")]
        public IActionResult MarquerNonLu(int messageId, int userId)
        {
            _service.MarquerNonLu(messageId, userId);
            return Ok();
        }

        // PUT api/messagerie/conversations/{conversationId}/lus/{userId}
        [HttpPut("conversations/{conversationId:int}/lus/{userId:int}")]
        public IActionResult MarquerTousLus(int conversationId, int userId)
        {
            _service.MarquerTousLus(conversationId, userId);
            return Ok();
        }

        // POST api/messagerie/messages/{messageId}/signaler
        [HttpPost("messages/{messageId:int}/signaler")]
        public IActionResult SignalerMessage(int messageId)
        {
            var ok = _service.SignalerMessage(messageId);
            if (!ok) return NotFound();
            return Ok();
        }

        // GET api/messagerie/messages/signales
        [HttpGet("messages/signales")]
        public IActionResult GetMessagesSignales()
        {
            return Ok(_service.GetMessagesSignales());
        }

        // DELETE api/messagerie/messages/{messageId}
        [HttpDelete("messages/{messageId:int}")]
        public IActionResult SupprimerMessage(int messageId)
        {
            var ok = _service.SupprimerMessage(messageId);
            if (!ok) return NotFound();
            return Ok();
        }

        // PUT api/messagerie/messages/{messageId}/lever-signalement
        [HttpPut("messages/{messageId:int}/lever-signalement")]
        public IActionResult LeverSignalement(int messageId)
        {
            var ok = _service.LeverSignalement(messageId);
            if (!ok) return NotFound();
            return Ok();
        }

        // POST api/messagerie/annonce
        [HttpPost("annonce")]
        public IActionResult EnvoyerAnnonce([FromBody] AnnonceRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Sujet) || string.IsNullOrWhiteSpace(req.Contenu))
                return BadRequest("Sujet et contenu requis.");

            var id = _service.EnvoyerAnnonce(req.AdminId, req.Sujet, req.Contenu, req.Cible);
            return Ok(new { IdConversation = id });
        }

        // POST api/messagerie/groupe-cours
        [HttpPost("groupe-cours")]
        public IActionResult EnvoyerAuGroupe([FromBody] MessageGroupeRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Sujet) || string.IsNullOrWhiteSpace(req.Contenu))
                return BadRequest("Sujet et contenu requis.");

            var id = _service.EnvoyerAuGroupe(req.EnseignantId, req.GroupeId, req.Sujet, req.Contenu);
            if (id == -1) return BadRequest("Aucun élève dans ce groupe.");
            return Ok(new { IdConversation = id });
        }
    }

    public class CreateConversationRequest
    {
        public int CreatorId { get; set; }
        public string? Sujet { get; set; }
        public string Type { get; set; } = "Individuelle";
        public List<int> ParticipantIds { get; set; } = new();
    }

    public class SendMessageRequest
    {
        public int ConversationId { get; set; }
        public int SenderId { get; set; }
        public string? Contenu { get; set; }
        public string? PieceJointePath { get; set; }
        public string? PieceJointeNom { get; set; }
    }

    public class AnnonceRequest
    {
        public int AdminId { get; set; }
        public string Sujet { get; set; } = "";
        public string Contenu { get; set; } = "";
        /// <summary>TOUS, ENSEIGNANTS ou ELEVES</summary>
        public string Cible { get; set; } = "TOUS";
    }

    public class MessageGroupeRequest
    {
        public int EnseignantId { get; set; }
        public int GroupeId { get; set; }
        public string Sujet { get; set; } = "";
        public string Contenu { get; set; } = "";
    }
}
