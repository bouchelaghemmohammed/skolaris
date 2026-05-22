using Microsoft.AspNetCore.Mvc;
using Skolaris.Dto;
using Skolaris.Models;
using Skolaris.Services;

namespace Skolaris.Controllers
{
    [ApiController]
    [Route("api/absences")]
    public class AbsencesApiController : ControllerBase
    {
        private readonly AbsenceService _absenceService;

        public AbsencesApiController(AbsenceService absenceService)
        {
            _absenceService = absenceService;
        }

        // GET: api/absences
        [HttpGet]
        public IActionResult GetAbsences()
        {
            return Ok(_absenceService.GetAllAbsences());
        }

        // GET: api/absences/{id}
        [HttpGet("{id}")]
        public IActionResult GetAbsence(int id)
        {
            var absence = _absenceService.GetAbsenceById(id);
            if (absence == null)
                return NotFound();
            return Ok(absence);
        }

        // GET: api/absences/eleve/{idEleve}
        [HttpGet("eleve/{idEleve}")]
        public IActionResult GetAbsencesByEleve(int idEleve)
        {
            return Ok(_absenceService.GetAbsencesByEleve(idEleve));
        }

        // GET: api/absences/user/{userId}
        [HttpGet("user/{userId}")]
        public IActionResult GetAbsencesByUser(int userId)
        {
            return Ok(_absenceService.GetAbsencesByUser(userId));
        }

        // GET: api/absences/cours/{coursOffertId} (parallèle à Jeff)
        [HttpGet("cours/{coursOffertId}")]
        public IActionResult GetAbsencesByCoursOffert(int coursOffertId)
        {
            return Ok(_absenceService.GetAbsencesByCoursOffert(coursOffertId));
        }

        // POST: api/absences — Input présence/absence (ABS-01)
        [HttpPost]
        public IActionResult CreateAbsence([FromBody] CreateAbsenceDto dto)
        {
            var absence = new Absence
            {
                Type = dto.Type,
                Statut = Enums.StatutAbsence.EnAttente,
                DateAbsence = dto.DateAbsence,
                IdEleve = dto.IdEleve,
                IdCoursOffert = dto.IdCoursOffert
            };

            var result = _absenceService.CreateAbsence(absence);
            if (!result)
                return BadRequest("Utilisateur introuvable.");
            return Ok(new { absence.IdAbsence });
        }

        // PUT: api/absences/{id}
        [HttpPut("{id}")]
        public IActionResult UpdateAbsence(int id, [FromBody] Absence absence)
        {
            var result = _absenceService.UpdateAbsence(id, absence);
            if (!result)
                return NotFound();
            return Ok();
        }

        // PUT: api/absences/{id}/statut — changer le statut (Approuvee / Refusee)
        [HttpPut("{id}/statut")]
        public IActionResult ChangerStatut(int id, [FromBody] ChangerStatutDto dto)
        {
            var statut = dto.Statut == "Approuvee"
                ? Enums.StatutAbsence.Approuvee
                : Enums.StatutAbsence.Refusee;
            var result = _absenceService.ChangerStatut(id, statut);
            if (!result) return NotFound();
            return Ok();
        }

        // POST: api/absences/{id}/explication — texte de justification soumis par l'élève
        [HttpPost("{id}/explication")]
        public IActionResult SetExplicationEleve(int id, [FromBody] ExplicationDto dto)
        {
            var result = _absenceService.SetExplicationEleve(id, dto.Texte ?? "");
            if (!result) return NotFound();
            return Ok();
        }

        // POST: api/absences/{id}/justification — soumis par l'enseignant
        [HttpPost("{id}/justification")]
        public IActionResult SetJustification(int id, [FromBody] JustificationDto dto)
        {
            var statut = dto.Statut == "NonJustifiee"
                ? Enums.StatutJustification.NonJustifiee
                : Enums.StatutJustification.Justifiee;

            var result = _absenceService.SetJustification(id, statut, dto.Description);
            if (!result)
                return NotFound();
            return Ok();
        }

        // GET: api/absences/rapport?idEleve=&idGroupe=&idCoursOffert=
        [HttpGet("rapport")]
        public IActionResult GetRapport([FromQuery] int? idEleve, [FromQuery] int? idGroupe, [FromQuery] int? idCoursOffert)
        {
            return Ok(_absenceService.GetRapport(idEleve, idGroupe, idCoursOffert));
        }

        // DELETE: api/absences/{id}
        [HttpDelete("{id}")]
        public IActionResult DeleteAbsence(int id)
        {
            var result = _absenceService.DeleteAbsence(id);
            if (!result)
                return NotFound();
            return Ok();
        }
    }

    public class JustificationDto
    {
        public string Statut { get; set; } = "Justifiee";
        public string? Description { get; set; }
    }

    public class ExplicationDto
    {
        public string? Texte { get; set; }
    }
}
