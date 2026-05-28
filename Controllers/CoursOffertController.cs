using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Skolaris.Data;
using Skolaris.Models;
using System.Linq;

namespace Skolaris.Controllers
{
    [ApiController]
    [Route("api/[Controller]")]
    public class CoursOffertController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CoursOffertController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/CoursOffert
        [HttpGet]
        public IActionResult GetAll()
        {
            var coursOfferts = _context.CoursOfferts
                .Include(co => co.Cours)
                    .ThenInclude(c => c.Programme)
                .Include(co => co.Session)
                .Include(co => co.Enseignant)
                    .ThenInclude(e => e.Utilisateur)
                .Include(co => co.Groupe)
                .Select(co => new
                {
                    IdCoursOffert = co.IdCoursOffert,
                    IdCours = co.IdCours,
                    IdGroupe = co.IdGroupe,
                    IdSession = co.IdSession,
                    IdEnseignant = co.IdEnseignant,
                    IdUtilisateurEnseignant = co.Enseignant != null ? co.Enseignant.IdUtilisateur : (int?)null,
                    ModeEnseignement = co.ModeEnseignement,
                    NomCours = co.Cours.Nom,
                    NomSession = co.Session.Libelle,
                    IdProgramme = co.Cours.IdProgramme,
                    NomProgramme = co.Cours.Programme.Nom,
                    NomGroupe = co.Groupe.Nom,
                    NomEnseignant = co.Enseignant != null
                        ? co.Enseignant.Utilisateur.Prenom + " " + co.Enseignant.Utilisateur.Nom
                        : "",
                })
                .ToList();

            return Ok(coursOfferts);
        }

        // POST: api/CoursOffert
        [HttpPost]
        public IActionResult Create(CoursOffert coursOffert)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Logic here to see if the coursOffert can be created (e.g., check if the referenced Cours exists, etc.)
            // Check FK validity for IdCours
            var coursExists = _context.Cours.Any(c => c.IdCours == coursOffert.IdCours);

            if (!coursExists)
                return BadRequest("Cours n'existe pas"); // If it doesn't exist

            _context.CoursOfferts.Add(coursOffert); // Or else
            _context.SaveChanges();

            return CreatedAtAction(nameof(GetById), new { id = coursOffert.IdCoursOffert }, coursOffert);
        }

        // GET: api/CoursOffert/{id}
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var coursOffert = _context.CoursOfferts.Find(id);

            if (coursOffert == null)
                return NotFound();

            return Ok(coursOffert);
        }

        // PUT: api/CoursOffert/{id}
        [HttpPut("{id}")]
        public IActionResult Update(int id, CoursOffert updatedCoursOffert)
        {
            if (id != updatedCoursOffert.IdCoursOffert)
                return BadRequest();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var coursOffert = _context.CoursOfferts.Find(id);

            if (coursOffert == null)
                return NotFound();

            coursOffert.IdCours = updatedCoursOffert.IdCours;
            coursOffert.IdSession = updatedCoursOffert.IdSession;
            coursOffert.IdGroupe = updatedCoursOffert.IdGroupe;
            coursOffert.IdEnseignant = updatedCoursOffert.IdEnseignant;
            coursOffert.ModeEnseignement = updatedCoursOffert.ModeEnseignement;
            // coursOffert.Actif = updatedCoursOffert.Actif;

            _context.SaveChanges();

            return Ok(coursOffert);
        }

        // Logic for the last user story in my Sprint where we need to assign a CoursOffert to a Groupe and an Enseignant
        // We add a new endpoint here to assign a CoursOffert to a Groupe and an Enseignant
        [HttpPut("{id}/assign")]
        public IActionResult Assign(int id, [FromBody] AssignRequest request)
        {
            var coursOffert = _context.CoursOfferts.Find(id);
            if (coursOffert == null)
                return NotFound();

            coursOffert.IdEnseignant = request.IdEnseignant;
            coursOffert.IdGroupe = request.IdGroupe;

            _context.SaveChanges();

            return Ok(coursOffert);
        }

        // DELETE: api/CoursOffert/{id}
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var coursOffert = _context.CoursOfferts.Find(id);

            if (coursOffert == null)
                return NotFound("Cours offert introuvable.");

            bool aDesInscriptions = _context.Inscriptions.Any(i => i.IdCoursOffert == id);
            bool aDesNotes = _context.Notes.Any(n => n.IdCoursOffert == id);
            bool aDesAbsences = _context.Absences.Any(a => a.IdCoursOffert == id);
            bool aDesCreneaux = _context.EmploisDuTemps.Any(e => e.IdCoursOffert == id);
            bool aDesDetailsBulletin = _context.DetailBulletins.Any(d => d.IdCoursOffert == id);
            bool aUneGrille = _context.GrillesEvaluation.Any(g => g.IdCoursOffert == id);

            if (aDesInscriptions ||
                aDesNotes ||
                aDesAbsences ||
                aDesCreneaux ||
                aDesDetailsBulletin ||
                aUneGrille)
            {
                return Conflict("Impossible de supprimer ce cours offert : il est associé à des inscriptions, notes, absences, horaires, bulletins ou grilles d'évaluation.");
            }

            try
            {
                _context.CoursOfferts.Remove(coursOffert);
                _context.SaveChanges();

                return Ok("Cours offert supprimé avec succès.");
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException)
            {
                return Conflict("Impossible de supprimer ce cours offert : il est associé à d'autres données.");
            }
        }
    }
}
