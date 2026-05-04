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

        // GET: api/Groupes
        // Utilisé par le frontend pour le filtre par groupe (EmploiDuTempsAdmin)
        [HttpGet]
        public IActionResult GetAll()
        {
            var groupes = _context.Groupes
                .Select(g => new { id = g.IdGroupe, nom = g.Nom })
                .ToList();

            return Ok(groupes);
        }
    }
}
