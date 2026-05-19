using Skolaris.Models;

using Microsoft.AspNetCore.Mvc;
using Skolaris.Data;

namespace Skolaris.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GroupesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public GroupesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Groupes (GetAll)
        // Utilisé par le frontend pour le filtre par groupe (EmploiDuTempsAdmin)
        [HttpGet]
        public IActionResult GetAll()
        {
            var groupes = _context.Groupes
                .Select(g => new { IdGroupe = g.IdGroupe, Nom = g.Nom, IdProgramme = g.IdProgramme }) // Modified: added IdProgramme
                .ToList();

            return Ok(groupes);
        }

        // GET: api/Groupes/:id (GetById)
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var groupe = _context.Groupes
                .Where(g => g.IdGroupe == id)
                .Select(g => new { id = g.IdGroupe, nom = g.Nom, g.IdProgramme })
                .FirstOrDefault();

            if (groupe == null)
                return NotFound();

            return Ok(groupe);
        }

        // POST: api/Groupes (Post)
        [HttpPost]
        public IActionResult Create(Groupe groupe)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            _context.Groupes.Add(groupe);
            _context.SaveChanges();
            return Ok();
        }

        // PUT: api/Groupes/:id
        [HttpPut("{id}")]
        public IActionResult Update(int id, Groupe updatedGroupe)
        {
            if (id != updatedGroupe.IdGroupe) // If id entered by user is different from the one in db
                return BadRequest(ModelState);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var groupe = _context.Groupes.Find(id);

            if (groupe == null)
            {
                return NotFound();
            }

            groupe.Nom = updatedGroupe.Nom;
            groupe.IdProgramme = updatedGroupe.IdProgramme;

            _context.SaveChanges();
            return Ok(groupe);

        }

        // Delete: api/Groupes/:id
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var groupe = _context.Groupes.Find(id);

            if (groupe == null)
                return NotFound();

            _context.Groupes.Remove(groupe);
            _context.SaveChanges();

            return Ok();
        }

    }

}

// Implementing GET BY ID, POST, PUT, DELETE
