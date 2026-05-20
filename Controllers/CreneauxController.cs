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

        public CreneauxController(ApplicationDbContext context)
        {
            _context = context;
        }

        private object MapCreneau(EmploiDuTemps e) => new
        {
            id            = e.IdEmploi,
            coursOffertId = e.IdCoursOffert,
            coursNom      = e.CoursOffert.Cours.Nom,
            groupeNom     = e.CoursOffert.Groupe.Nom,
            groupeId      = e.CoursOffert.IdGroupe,
            enseignantId  = e.CoursOffert.Enseignant != null ? e.CoursOffert.Enseignant.IdEnseignant : (int?)null,
            enseignantNom = e.CoursOffert.Enseignant != null
                ? e.CoursOffert.Enseignant.Utilisateur.Prenom + " " + e.CoursOffert.Enseignant.Utilisateur.Nom
                : "—",
            jourSemaine   = (int)e.JourSemaine,
            heureDebut    = e.HeureDebut.ToString("hh\\:mm"),
            heureFin      = e.HeureFin.ToString("hh\\:mm"),
            salle         = e.Salle,
            isPublie      = e.IsPublie
        };

        private IQueryable<EmploiDuTemps> WithIncludes() =>
            _context.EmploisDuTemps
                .Include(e => e.CoursOffert).ThenInclude(co => co.Cours)
                .Include(e => e.CoursOffert).ThenInclude(co => co.Groupe)
                .Include(e => e.CoursOffert).ThenInclude(co => co.Enseignant)
                    .ThenInclude(ens => ens!.Utilisateur);

        // GET: api/Creneaux
        [HttpGet]
        public IActionResult GetAll()
        {
            var list = WithIncludes().AsEnumerable()
                .Select(MapCreneau)
                .OrderBy(e => ((dynamic)e).jourSemaine)
                .ToList();
            return Ok(list);
        }

        // GET: api/Creneaux/publie  — only published (for eleve/enseignant)
        [HttpGet("publie")]
        public IActionResult GetPublie()
        {
            var list = WithIncludes()
                .Where(e => e.IsPublie)
                .AsEnumerable()
                .Select(MapCreneau)
                .OrderBy(e => ((dynamic)e).jourSemaine)
                .ToList();
            return Ok(list);
        }

        // GET: api/Creneaux/eleve/{id}
        [HttpGet("eleve/{idUtilisateur}")]
        public IActionResult GetByEleve(int idUtilisateur)
        {
            var eleve = _context.Eleves.FirstOrDefault(e => e.IdUtilisateur == idUtilisateur);
            if (eleve == null) return NotFound("Eleve introuvable.");

            var list = WithIncludes()
                .Where(e => e.CoursOffert.IdGroupe == eleve.IdGroupe && e.IsPublie)
                .AsEnumerable()
                .Select(e => new {
                    id            = e.IdEmploi,
                    coursNom      = e.CoursOffert.Cours.Nom,
                    enseignantNom = e.CoursOffert.Enseignant != null
                        ? e.CoursOffert.Enseignant.Utilisateur.Prenom + " " + e.CoursOffert.Enseignant.Utilisateur.Nom
                        : "—",
                    jourSemaine   = (int)e.JourSemaine,
                    heureDebut    = e.HeureDebut.ToString("hh\\:mm"),
                    heureFin      = e.HeureFin.ToString("hh\\:mm"),
                    salle         = e.Salle
                })
                .OrderBy(e => e.jourSemaine).ThenBy(e => e.heureDebut)
                .ToList();

            return Ok(list);
        }

        // GET: api/Creneaux/enseignant/{id}
        [HttpGet("enseignant/{idUtilisateur}")]
        public IActionResult GetByEnseignant(int idUtilisateur)
        {
            var enseignant = _context.Enseignants.FirstOrDefault(e => e.IdUtilisateur == idUtilisateur);
            if (enseignant == null) return NotFound("Enseignant introuvable.");

            var list = WithIncludes()
                .Where(e => e.CoursOffert.IdEnseignant == enseignant.IdEnseignant && e.IsPublie)
                .AsEnumerable()
                .Select(e => new {
                    id          = e.IdEmploi,
                    coursNom    = e.CoursOffert.Cours.Nom,
                    groupeNom   = e.CoursOffert.Groupe.Nom,
                    jourSemaine = (int)e.JourSemaine,
                    heureDebut  = e.HeureDebut.ToString("hh\\:mm"),
                    heureFin    = e.HeureFin.ToString("hh\\:mm"),
                    salle       = e.Salle
                })
                .OrderBy(e => e.jourSemaine).ThenBy(e => e.heureDebut)
                .ToList();

            return Ok(list);
        }

        // GET: api/Creneaux/{id}
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var c = _context.EmploisDuTemps.Find(id);
            if (c == null) return NotFound();
            return Ok(c);
        }

        // POST: api/Creneaux — with conflict detection
        [HttpPost]
        public IActionResult Create(EmploiDuTempsCreateDto dto)
        {
            if (dto.CoursOffertId == 0) return BadRequest("Cours offert requis.");
            var coursOffert = _context.CoursOfferts
                .Include(co => co.Groupe)
                .FirstOrDefault(co => co.IdCoursOffert == dto.CoursOffertId);
            if (coursOffert == null) return BadRequest("Cours offert introuvable.");

            if (!TimeSpan.TryParse(dto.HeureDebut, out var debut))
                return BadRequest("Format heure debut invalide.");
            if (!TimeSpan.TryParse(dto.HeureFin, out var fin))
                return BadRequest("Format heure fin invalide.");

            var jour = (JourSemaine)dto.JourSemaine;

            var conflicts = DetectConflicts(0, jour, debut, fin, dto.Salle, coursOffert.IdEnseignant, coursOffert.IdGroupe);
            if (conflicts.Any())
                return Conflict(new { message = "Conflit detecte", conflicts });

            var creneau = new EmploiDuTemps
            {
                IdCoursOffert = dto.CoursOffertId,
                JourSemaine   = jour,
                HeureDebut    = debut,
                HeureFin      = fin,
                Salle         = dto.Salle,
                IsPublie      = false
            };

            _context.EmploisDuTemps.Add(creneau);
            _context.SaveChanges();
            return CreatedAtAction(nameof(GetById), new { id = creneau.IdEmploi }, creneau);
        }

        // PUT: api/Creneaux/{id} — with conflict detection
        [HttpPut("{id}")]
        public IActionResult Update(int id, EmploiDuTempsCreateDto dto)
        {
            var creneau = _context.EmploisDuTemps.Find(id);
            if (creneau == null) return NotFound();

            var coursOffert = _context.CoursOfferts
                .Include(co => co.Groupe)
                .FirstOrDefault(co => co.IdCoursOffert == dto.CoursOffertId);
            if (coursOffert == null) return BadRequest("Cours offert introuvable.");

            if (!TimeSpan.TryParse(dto.HeureDebut, out var debut))
                return BadRequest("Format heure debut invalide.");
            if (!TimeSpan.TryParse(dto.HeureFin, out var fin))
                return BadRequest("Format heure fin invalide.");

            var jour = (JourSemaine)dto.JourSemaine;

            var conflicts = DetectConflicts(id, jour, debut, fin, dto.Salle, coursOffert.IdEnseignant, coursOffert.IdGroupe);
            if (conflicts.Any())
                return Conflict(new { message = "Conflit detecte", conflicts });

            creneau.IdCoursOffert = dto.CoursOffertId;
            creneau.JourSemaine   = jour;
            creneau.HeureDebut    = debut;
            creneau.HeureFin      = fin;
            creneau.Salle         = dto.Salle;

            _context.SaveChanges();
            return Ok(creneau);
        }

        // PATCH: api/Creneaux/{id}/publier
        [HttpPatch("{id}/publier")]
        public IActionResult Publier(int id)
        {
            var creneau = _context.EmploisDuTemps.Find(id);
            if (creneau == null) return NotFound();
            creneau.IsPublie = true;
            _context.SaveChanges();
            return Ok(new { message = "Creneau publie." });
        }

        // PATCH: api/Creneaux/{id}/depublier
        [HttpPatch("{id}/depublier")]
        public IActionResult Depublier(int id)
        {
            var creneau = _context.EmploisDuTemps.Find(id);
            if (creneau == null) return NotFound();
            creneau.IsPublie = false;
            _context.SaveChanges();
            return Ok(new { message = "Creneau depublie." });
        }

        // PATCH: api/Creneaux/publier-tout — publish all at once
        [HttpPatch("publier-tout")]
        public IActionResult PublierTout()
        {
            var tous = _context.EmploisDuTemps.ToList();
            tous.ForEach(e => e.IsPublie = true);
            _context.SaveChanges();
            return Ok(new { message = tous.Count + " creneaux publies." });
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

        // ── Conflict detection helper ─────────────────────────────────────────
        private List<string> DetectConflicts(int excludeId, JourSemaine jour, TimeSpan debut, TimeSpan fin, string? salle, int? idEnseignant, int idGroupe)
        {
            var conflicts = new List<string>();

            var existing = _context.EmploisDuTemps
                .Include(e => e.CoursOffert)
                .Where(e => e.IdEmploi != excludeId && e.JourSemaine == jour)
                .AsEnumerable()
                .Where(e => debut < e.HeureFin && fin > e.HeureDebut)
                .ToList();

            // Salle conflict
            if (!string.IsNullOrWhiteSpace(salle))
            {
                var salleConflict = existing.FirstOrDefault(e =>
                    !string.IsNullOrWhiteSpace(e.Salle) &&
                    e.Salle!.Trim().ToLower() == salle.Trim().ToLower());
                if (salleConflict != null)
                    conflicts.Add("La salle " + salle + " est deja occupee a ce creneau.");
            }

            // Enseignant conflict
            if (idEnseignant.HasValue)
            {
                var ensConflict = existing.FirstOrDefault(e => e.CoursOffert.IdEnseignant == idEnseignant);
                if (ensConflict != null)
                    conflicts.Add("L'enseignant a deja un cours a ce creneau.");
            }

            // Groupe conflict
            var groupeConflict = existing.FirstOrDefault(e => e.CoursOffert.IdGroupe == idGroupe);
            if (groupeConflict != null)
                conflicts.Add("Le groupe a deja un cours a ce creneau.");

            return conflicts;
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
