using Microsoft.AspNetCore.Mvc;
using Skolaris.Data;
using Microsoft.EntityFrameworkCore;

namespace Skolaris.Controllers
{
    [ApiController]
    [Route("api/eleves")]
    public class ElevesApiController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ElevesApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/eleves
        [HttpGet]
        public IActionResult GetEleves()
        {
            var eleves = _context.Eleves
                .Include(e => e.Utilisateur)
                .Select(e => new
                {
                    IdEleve = e.IdEleve,
                    Matricule = e.Matricule,
                    Nom = e.Utilisateur.Nom,
                    Prenom = e.Utilisateur.Prenom,
                    Email = e.Utilisateur.Email
                })
                .ToList();

            return Ok(eleves);
        }

        // GET: api/eleves/cours/{idCoursOffert}
        [HttpGet("cours/{idCoursOffert}")]
        public IActionResult GetElevesByCoursOffert(int idCoursOffert)
        {
            var eleves = _context.Inscriptions
                .Where(i => i.IdCoursOffert == idCoursOffert)
                .Include(i => i.Eleve)
                    .ThenInclude(e => e.Utilisateur)
                .Select(i => new
                {
                    IdEleve = i.Eleve.IdEleve,
                    Matricule = i.Eleve.Matricule,
                    Nom = i.Eleve.Utilisateur.Nom,
                    Prenom = i.Eleve.Utilisateur.Prenom
                })
                .ToList();

            return Ok(eleves);
        }
    }
}
