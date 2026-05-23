using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Skolaris.Data;

namespace Skolaris.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EnseignantController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public EnseignantController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Enseignant
        [HttpGet]
        public IActionResult GetAll()
        {
            var enseignants = _context.Enseignants
                .Include(e => e.Utilisateur)
                .Select(e => new
                {
                    IdEnseignant = e.IdEnseignant,
                    prenom = e.Utilisateur.Prenom,
                    Nom = e.Utilisateur.Nom
                })
                .ToList();

            return Ok(enseignants);
        }
    }
}
