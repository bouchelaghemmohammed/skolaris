using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Skolaris.Data;

namespace Skolaris.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CoursOffertsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CoursOffertsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/CoursOfferts
        // Utilisé par le frontend pour remplir les dropdowns (EmploiDuTempsAdmin)
        [HttpGet]
        public IActionResult GetAll()
        {
            var offerts = _context.CoursOfferts
                .Include(co => co.Cours)
                .Include(co => co.Groupe)
                .Include(co => co.Enseignant)
                    .ThenInclude(e => e.Utilisateur)
                .Select(co => new
                {
                    id            = co.IdCoursOffert,
                    coursNom      = co.Cours.Nom,
                    groupeNom     = co.Groupe.Nom,
                    enseignantNom = co.Enseignant != null
                        ? co.Enseignant.Utilisateur.Prenom + " " + co.Enseignant.Utilisateur.Nom
                        : "—"
                })
                .ToList();

            return Ok(offerts);
        }
    }
}
