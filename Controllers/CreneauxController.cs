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

        // GET: api/Creneaux
        // Vue Admin — tous les créneaux
        [HttpGet]
        public IActionResult GetAll()
        {
            var creneaux = _context.EmploisDuTemps
                .Include(e => e.CoursOffert)
                    .ThenInclude(co => co.Cours)
                .Include(e => e.CoursOffert)
                    .ThenInclude(co => co.Groupe)
                .Include(e => e.CoursOffert)
                    .ThenInclude(co => co.Enseignant)
                        .ThenInclude(ens => ens.Utilisateur)
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
                .ToList();

            return Ok(creneaux);
        }

        // GET: api/Creneaux/eleve/{id}
        // HOR-06 — Vue Élève : créneaux filtrés par groupe de l'élève
        [HttpGet("eleve/{idEleve}")]
        public IActionResult GetByEleve(int idEleve)
        {
            var eleve = _context.Eleves.Find(idEleve);
            if (eleve == null) return NotFound("Élève introuvable.");

            var creneaux = _context.EmploisDuTemps
                .Include(e => e.CoursOffert)
                    .ThenInclude(co => co.Cours)
                .Include(e => e.CoursOffert)
                    .ThenInclude(co => co.Groupe)
                .Include(e => e.CoursOffert)
                    .ThenInclude(co => co.Enseignant)
                        .ThenInclude(ens => ens.Utilisateur)
                .Where(e => e.CoursOffert.IdGroupe == eleve.IdGroupe)
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

        // GET: api/Creneaux/enseignant/{id}
        // HOR-08 — Vue Enseignant : créneaux filtrés par enseignant
        [HttpGet("enseignant/{idEnseignant}")]
        public IActionResult GetByEnseignant(int idEnseignant)
        {
            var creneaux = _context.EmploisDuTemps
                .Include(e => e.CoursOffert)
                    .ThenInclude(co => co.Cours)
                .Include(e => e.CoursOffert)
                    .ThenInclude(co => co.Groupe)
                .Where(e => e.CoursOffert.IdEnseignant == idEnseignant)
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
        // HOR-04 — Admin crée un créneau
        [HttpPost]
        public IActionResult Create(EmploiDuTempsCreateDto dto)
        {
            var coursOffert = _context.CoursOfferts.Find(dto.CoursOffertId);
            if (coursOffert == null) return BadRequest("Cours offert introuvable.");

            var creneau = new EmploiDuTemps
            {
                IdCoursOffert = dto.CoursOffertId,
                JourSemaine   = (JourSemaine)dto.JourSemaine,
                HeureDebut    = TimeSpan.Parse(dto.HeureDebut),
                HeureFin      = TimeSpan.Parse(dto.HeureFin),
                Salle         = dto.Salle
            };

            _context.EmploisDuTemps.Add(creneau);
            _context.SaveChanges();

            return CreatedAtAction(nameof(GetById), new { id = creneau.IdEmploi }, creneau);
        }

        // PUT: api/Creneaux/{id}
        // HOR-04 — Admin modifie un créneau
        [HttpPut("{id}")]
        public IActionResult Update(int id, EmploiDuTempsCreateDto dto)
        {
            var creneau = _context.EmploisDuTemps.Find(id);
            if (creneau == null) return NotFound();

            creneau.IdCoursOffert = dto.CoursOffertId;
            creneau.JourSemaine   = (JourSemaine)dto.JourSemaine;
            creneau.HeureDebut    = TimeSpan.Parse(dto.HeureDebut);
            creneau.HeureFin      = TimeSpan.Parse(dto.HeureFin);
            creneau.Salle         = dto.Salle;

            _context.SaveChanges();
            return Ok(creneau);
        }

        // DELETE: api/Creneaux/{id}
        // HOR-04 — Admin supprime un créneau
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

    // ── DTO ─────────────────────────────────────────────────────────────────
    public class EmploiDuTempsCreateDto
    {
        public int     CoursOffertId { get; set; }
        public int     JourSemaine   { get; set; }
        public string  HeureDebut    { get; set; } = "";
        public string  HeureFin      { get; set; } = "";
        public string? Salle         { get; set; }
    }
}
