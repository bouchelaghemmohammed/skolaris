using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Skolaris.Data;
using Skolaris.Enums;
using Skolaris.Models;

namespace Skolaris.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CreneauxController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CreneauxController(ApplicationDbContext context
        {
            _context = context;
        }

        // GET: api/Creneaux
        [HttpGet]
        public IActionResult GetAll()
        {
            
            var creneaux = _context.EmploisDuTemps
                .Include(e => e.CoursOffert).ThenInclude(co => co.Cours)
                .Include(e => e.CoursOffert).ThenInclude(co => co.Groupe)
                .Include(e => e.CoursOffert).ThenInclude(co => co.Enseignant)
                    .ThenInclude(ens => ens!.Utilisateur)
                .AsEnumerable()
                .Select(e => new
                {
                    id            = e.IdEmploi,
                    coursOffertId = e.IdCoursOffert,
                    coursNom      = e.CoursOffert.Cours.Nom,
                    groupeNom     = e.CoursOffert.Groupe.Nom,
                    groupeId      = e.CoursOffert.IdGroupe,
                    enseignantNom = e.CoursOffert.Enseignant != null
                        ? e.CoursOffert.Enseignant.Utilisateur.Prenom + " " + e.CoursOffert.Enseignant.Utilisateur.Nom
                        : "—",
                    jourSemaine   = (int)e.JourSemaine,   
                    heureDebut    = e.HeureDebut.ToString(@"hh\:mm"),
                    heureFin      = e.HeureFin.ToString(@"hh\:mm"),
                    salle         = e.Salle
                })
                .OrderBy(e => e.jourSemaine)
                .ToList();

            return Ok(creneaux);
        }

        // GET: api/Creneaux/eleve/{id}   (id = IdUtilisateur)
        [HttpGet("eleve/{idUtilisateur}")]
        public IActionResult GetByEleve(int idUtilisateur)
        {
            var eleve = _context.Eleves.FirstOrDefault(e => e.IdUtilisateur == idUtilisateur);
            if (eleve == null) return NotFound("Élève introuvable.");

            var creneaux = _context.EmploisDuTemps
                .Include(e => e.CoursOffert).ThenInclude(co => co.Cours)
                .Include(e => e.CoursOffert).ThenInclude(co => co.Groupe)
                .Include(e => e.CoursOffert).ThenInclude(co => co.Enseignant)
                    .ThenInclude(ens => ens!.Utilisateur)
                .Where(e => e.CoursOffert.IdGroupe == eleve.IdGroupe)
                .AsEnumerable()
                .Select(e => new
                {
                    id            = e.IdEmploi,
                    coursNom      = e.CoursOffert.Cours.Nom,
                    enseignantNom = e.CoursOffert.Enseignant != null
                        ? e.CoursOffert.Enseignant.Utilisateur.Prenom + " " + e.CoursOffert.Enseignant.Utilisateur.Nom
                        : "—",
                    jourSemaine   = (int)e.JourSemaine,
                    heureDebut    = e.HeureDebut.ToString(@"hh\:mm"),
                    heureFin      = e.HeureFin.ToString(@"hh\:mm"),
                    salle         = e.Salle
                })
                .OrderBy(e => e.jourSemaine)
                .ThenBy(e => e.heureDebut)
                .ToList();

            return Ok(creneaux);
        }

        // GET: api/Creneaux/enseignant/{id}   (id = IdUtilisateur)
        [HttpGet("enseignant/{idUtilisateur}")]
        public IActionResult GetByEnseignant(int idUtilisateur)
        {
            var enseignant = _context.Enseignants.FirstOrDefault(e => e.IdUtilisateur == idUtilisateur);
            if (enseignant == null) return NotFound("Enseignant introuvable.");

            var creneaux = _context.EmploisDuTemps
                .Include(e => e.CoursOffert).ThenInclude(co => co.Cours)
                .Include(e => e.CoursOffert).ThenInclude(co => co.Groupe)
                .Where(e => e.CoursOffert.IdEnseignant == enseignant.IdEnseignant)
                .AsEnumerable()
                .Select(e => new
                {
                    id          = e.IdEmploi,
                    coursNom    = e.CoursOffert.Cours.Nom,
                    groupeNom   = e.CoursOffert.Groupe.Nom,
                    jourSemaine = (int)e.JourSemaine,
                    heureDebut  = e.HeureDebut.ToString(@"hh\:mm"),
                    heureFin    = e.HeureFin.ToString(@"hh\:mm"),
                    salle       = e.Salle
                })
                .OrderBy(e => e.jourSemaine)
                .ThenBy(e => e.heureDebut)
                .ToList();

            return Ok(creneaux);
        }

        // GET: api/Creneaux/{id}
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var creneau = _context.EmploisDuTemps.Find(id);
            if (creneau == null) return NotFound();
            return Ok(creneau);
        }

        // POST: api/Creneaux
        [HttpPost]
        public IActionResult Create(EmploiDuTempsCreateDto dto)
        {
            if (dto.CoursOffertId == 0) return BadRequest("Cours offert requis.");
            var coursOffert = _context.CoursOfferts.Find(dto.CoursOffertId);
            if (coursOffert == null) return BadRequest("Cours offert introuvable.");

            if (!TimeSpan.TryParse(dto.HeureDebut, out var debut))
                return BadRequest("Format heure début invalide.");
            if (!TimeSpan.TryParse(dto.HeureFin, out var fin))
                return BadRequest("Format heure fin invalide.");

            var creneau = new EmploiDuTemps
            {
                IdCoursOffert = dto.CoursOffertId,
                JourSemaine   = (JourSemaine)dto.JourSemaine,
                HeureDebut    = debut,
                HeureFin      = fin,
                Salle         = dto.Salle
            };

            _context.EmploisDuTemps.Add(creneau);
            _context.SaveChanges();
            return CreatedAtAction(nameof(GetById), new { id = creneau.IdEmploi }, creneau);
        }

        // PUT: api/Creneaux/{id}
        [HttpPut("{id}")]
        public IActionResult Update(int id, EmploiDuTempsCreateDto dto)
        {
            var creneau = _context.EmploisDuTemps.Find(id);
            if (creneau == null) return NotFound();

            if (!TimeSpan.TryParse(dto.HeureDebut, out var debut))
                return BadRequest("Format heure début invalide.");
            if (!TimeSpan.TryParse(dto.HeureFin, out var fin))
                return BadRequest("Format heure fin invalide.");

            creneau.IdCoursOffert = dto.CoursOffertId;
            creneau.JourSemaine   = (JourSemaine)dto.JourSemaine;
            creneau.HeureDebut    = debut;
            creneau.HeureFin      = fin;
            creneau.Salle         = dto.Salle;

            _context.SaveChanges();
            return Ok(creneau);
        }

        // DELETE: api/Creneaux/{id}
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var creneau = _context.EmploisDuTemps.Find(id);
            if (creneau == null) return NotFound();
            _context.EmploisDuTemps.Remove(creneau);
            _context.SaveChanges();
            return Ok();
        }
    }

    public class EmploiDuTempsCreateDto
    {
        public int     CoursOffertId { get; set; }
        public int     JourSemaine   { get; set; }
        public string  HeureDebut    { get; set; } = "";
        public string  HeureFin      { get; set; } = "";
        public string? Salle         { get; set; }
    }
}
