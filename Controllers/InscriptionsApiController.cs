using Microsoft.AspNetCore.Mvc;
using Skolaris.Models;
using Skolaris.Services;

namespace Skolaris.Controllers
{
    [ApiController]
    [Route("api/inscriptions")]
    public class InscriptionsApiController : ControllerBase
    {
        private readonly InscriptionService _inscriptionService;

        public InscriptionsApiController(InscriptionService inscriptionService)
        {
            _inscriptionService = inscriptionService;
        }

        // GET: api/inscriptions
        [HttpGet]
        public IActionResult GetInscriptions()
        {
            var result = _inscriptionService.GetAllInscriptions()
                .Select(i => new
                {
                    i.IdInscription,
                    i.IdEleve,
                    i.IdCoursOffert,
                    i.DateInscription
                });
            return Ok(result);
        }

        // GET: api/inscriptions/{id}
        [HttpGet("{id}")]
        public IActionResult GetInscription(int id)
        {
            var inscription = _inscriptionService.GetInscriptionById(id);
            if (inscription == null)
                return NotFound();
            return Ok(inscription);
        }

        // GET: api/inscriptions/user/{userId}
        [HttpGet("user/{userId}")]
        public IActionResult GetInscriptionsByUser(int userId)
        {
            return Ok(_inscriptionService.GetInscriptionsByUser(userId));
        }

        // GET: api/inscriptions/programme/{programmeId}
        [HttpGet("cours/{coursOffertId}")]
        public IActionResult GetInscriptionsByCoursOffert(int coursOffertId)
        {
            return Ok(_inscriptionService.GetInscriptionsByCoursOffert(coursOffertId));
        }

        // POST: api/inscriptions — ADM-07
        [HttpPost]
        public IActionResult CreateInscription([FromBody] Inscription inscription)
        {
            var result = _inscriptionService.CreateInscription(inscription);
            if (!result)
                return BadRequest("Inscription impossible (utilisateur introuvable ou déjà inscrit).");
            return Ok();
        }

        // PUT: api/inscriptions/{id}
        [HttpPut("{id}")]
        public IActionResult UpdateInscription(int id, [FromBody] Inscription inscription)
        {
            var result = _inscriptionService.UpdateInscription(id, inscription);
            if (!result)
                return NotFound();
            return Ok();
        }

        // DELETE: api/inscriptions/{id}
        [HttpDelete("{id}")]
        public IActionResult DeleteInscription(int id)
        {
            var result = _inscriptionService.DeleteInscription(id);
            if (!result)
                return NotFound();
            return Ok();
        }
    }
}
