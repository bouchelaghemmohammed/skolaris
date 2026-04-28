using Microsoft.AspNetCore.Mvc;
using Skolaris.Services;

namespace Skolaris.Controllers
{
    [ApiController]
    [Route("api/dossier")]
    public class DossierAcademiqueApiController : ControllerBase
    {
        private readonly DossierAcademiqueService _service;

        public DossierAcademiqueApiController(DossierAcademiqueService service)
        {
            _service = service;
        }

        /// <summary>
        /// Récupère le dossier académique par IdUtilisateur (userId dans localStorage).
        /// </summary>
        [HttpGet("{idUtilisateur}")]
        public IActionResult GetDossier(int idUtilisateur)
        {
            var dossier = _service.GetDossierByUtilisateur(idUtilisateur);
            if (dossier == null)
                return NotFound("Aucun dossier trouvé pour cet utilisateur.");
            return Ok(dossier);
        }
    }
}
