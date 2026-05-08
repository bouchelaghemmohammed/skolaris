using Microsoft.AspNetCore.Mvc;
using Skolaris.Data;

namespace Skolaris.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NiveauController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public NiveauController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Niveau
        [HttpGet]
        public IActionResult GetAll()
        {
            var niveaux = _context.Niveaux
                .Select(n => new { n.IdNiveau, n.Nom, n.IdProgramme })
                .OrderBy(n => n.Nom)
                .ToList();
            return Ok(niveaux);
        }

        // GET: api/Niveau/programme/{idProgramme}
        [HttpGet("programme/{idProgramme}")]
        public IActionResult GetByProgramme(int idProgramme)
        {
            var niveaux = _context.Niveaux
                .Where(n => n.IdProgramme == idProgramme)
                .Select(n => new { n.IdNiveau, n.Nom, n.IdProgramme })
                .OrderBy(n => n.Nom)
                .ToList();
            return Ok(niveaux);
        }
    }
}
