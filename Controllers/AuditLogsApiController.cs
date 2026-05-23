using Microsoft.AspNetCore.Mvc;

using Skolaris.Services;

namespace Skolaris.Controllers
{
    [ApiController]
    [Route("api/audit-logs")]
    public class AuditLogsApiController : ControllerBase
    {
        // Service des logs
        private readonly AuditLogService _auditLogService;

        // Constructeur
        public AuditLogsApiController(AuditLogService auditLogService)
        {
            _auditLogService = auditLogService;
        }

        // GET : api/audit-logs
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                // Récupérer tous les logs
                var logs = await _auditLogService.GetAllAsync();

                // Retourner les logs
                return Ok(logs);
            }
            catch (Exception ex)
            {
                // Retour erreur serveur
                return StatusCode(500, ex.Message);
            }
        }

        // DELETE : api/audit-logs/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _auditLogService.DeleteLogAsync(id);

            return NoContent();
        }

        // DELETE : api/audit-logs/clear
        [HttpDelete("clear")]
        public async Task<IActionResult> Clear()
        {
            await _auditLogService.ClearLogsAsync();

            return NoContent();
        }
    }
}