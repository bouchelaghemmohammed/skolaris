using Skolaris.Models;

using Microsoft.AspNetCore.Mvc;
using Skolaris.Data;
using Skolaris.Models;

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

        // GET: api/Niveau/{id}
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var niveau = _context.Niveaux
                .Where(n => n.IdNiveau == id)
                .Select(n => new { n.IdNiveau, n.Nom, n.IdProgramme })
                .FirstOrDefault();

            if (niveau == null)
                return NotFound();

            return Ok(niveau);
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

        // POST: api/Niveau
        [HttpPost]
        public IActionResult Create(Niveau niveau)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _context.Niveaux.Add(niveau);
            _context.SaveChanges();

            return Ok(niveau);
        }


        // PUT: api/Niveau/{id}
        [HttpPut("{id}")]
        public IActionResult Update(int id, Niveau updatedNiveau)
        {
            if (id != updatedNiveau.IdNiveau)
                return BadRequest();

            var niveau = _context.Niveaux.Find(id);

            if (niveau == null)
                return NotFound();

            niveau.Nom = updatedNiveau.Nom;
            niveau.IdProgramme = updatedNiveau.IdProgramme;

            _context.SaveChanges();

            return Ok(niveau);
        }

        // Delete: api/Niveau/{id}
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {

            var niveau = _context.Niveaux.Find(id);

            if (niveau == null)
            {
                return NotFound();
            }

            _context.Niveaux.Remove(niveau);

            _context.SaveChanges();

            return Ok();
        }
    }
}
