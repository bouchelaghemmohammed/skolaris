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
    }
}
