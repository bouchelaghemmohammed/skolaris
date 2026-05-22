using Microsoft.AspNetCore.Mvc;
using Skolaris.Data;
using Skolaris.Models;

namespace Skolaris.Controllers
{
    [ApiController]
    [Route("api/annees")]
    public class AnneeScolaireApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AnneeScolaireApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var annees = _context.AnneesScolaires
                .Select(a => new { a.IdAnnee, a.Libelle, a.IdEcole })
                .ToList();
            return Ok(annees);
        }

        [HttpGet("ecole/{idEcole}")]
        public IActionResult GetByEcole(int idEcole)
        {
            var annees = _context.AnneesScolaires
                .Where(a => a.IdEcole == idEcole)
                .Select(a => new { a.IdAnnee, a.Libelle, a.IdEcole })
                .ToList();
            return Ok(annees);
        }

        [HttpPost]
        public IActionResult Create([FromBody] AnneeRequest request)
        {
            var annee = new AnneeScolaire
            {
                Libelle = request.Libelle,
                IdEcole = request.IdEcole
            };
            _context.AnneesScolaires.Add(annee);
            _context.SaveChanges();
            return Ok(new { annee.IdAnnee, annee.Libelle, annee.IdEcole });
        }
    }

    public class AnneeRequest
    {
        public string Libelle { get; set; } = "";
        public int IdEcole { get; set; }
    }
}
