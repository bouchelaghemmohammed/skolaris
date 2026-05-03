using Microsoft.AspNetCore.Mvc;
using Skolaris.Services;

namespace Skolaris.Controllers
{
    [ApiController]
    [Route("api/bulletins")]
    public class BulletinsApiController : ControllerBase
    {
        private readonly BulletinService _bulletinService;

        public BulletinsApiController(BulletinService bulletinService)
        {
            _bulletinService = bulletinService;
        }

        // GET: api/bulletins
        [HttpGet]
        public IActionResult GetBulletins()
        {
            return Ok(_bulletinService.GetAllBulletins());
        }

        // GET: api/bulletins/{id}
        [HttpGet("{id}")]
        public IActionResult GetBulletin(int id)
        {
            var bulletin = _bulletinService.GetBulletinById(id);
            if (bulletin == null)
                return NotFound();
            return Ok(bulletin);
        }

        // GET: api/bulletins/eleve/{idEleve} — NOT-11
        [HttpGet("eleve/{idEleve}")]
        public IActionResult GetBulletinsByEleve(int idEleve)
        {
            return Ok(_bulletinService.GetBulletinsByEleve(idEleve));
        }

        // GET: api/bulletins/utilisateur/{idUtilisateur} — NOT-11 (vue enrichie élève)
        [HttpGet("utilisateur/{idUtilisateur}")]
        public IActionResult GetBulletinsByUtilisateur(int idUtilisateur)
        {
            return Ok(_bulletinService.GetBulletinsEnrichisByUtilisateur(idUtilisateur));
        }

        // GET: api/bulletins/eleve/{idEleve}/session/{idSession}
        [HttpGet("eleve/{idEleve}/session/{idSession}")]
        public IActionResult GetBulletinByEleveAndSession(int idEleve, int idSession)
        {
            var bulletin = _bulletinService.GetBulletinByEleveAndSession(idEleve, idSession);
            if (bulletin == null)
                return NotFound();
            return Ok(bulletin);
        }

        // POST: api/bulletins/generer/{idEleve}/{idSession} — NOT-07
        [HttpPost("generer/{idEleve}/{idSession}")]
        public IActionResult GenererBulletin(int idEleve, int idSession)
        {
            var result = _bulletinService.GenererBulletin(idEleve, idSession);
            if (!result.Success)
                return BadRequest(result.ErrorMessage);
            return Ok(result.Bulletin);
        }

        // DELETE: api/bulletins/{id}
        [HttpDelete("{id}")]
        public IActionResult DeleteBulletin(int id)
        {
            var result = _bulletinService.DeleteBulletin(id);
            if (!result)
                return NotFound();
            return Ok();
        }
    }
}
