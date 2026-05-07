using Microsoft.AspNetCore.Mvc;
using Skolaris.Services;

namespace Skolaris.Controllers
{
    [ApiController]
    [Route("api/grilles")]
    public class GrillesEvaluationApiController : ControllerBase
    {
        private readonly GrilleEvaluationService _service;

        public GrillesEvaluationApiController(GrilleEvaluationService service)
        {
            _service = service;
        }

        // GET: api/grilles/cours/{idCoursOffert}
        [HttpGet("cours/{idCoursOffert}")]
        public IActionResult GetByCoursOffert(int idCoursOffert)
        {
            var grille = _service.GetByCoursOffert(idCoursOffert);
            if (grille == null) return NotFound();
            return Ok(grille);
        }

        // GET: api/grilles/{id}
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var grille = _service.GetById(id);
            if (grille == null) return NotFound();
            return Ok(grille);
        }

        // POST: api/grilles — crée la grille pour un cours offert
        [HttpPost]
        public IActionResult Create([FromBody] CreateGrilleRequest req)
        {
            var result = _service.CreateGrille(req.IdCoursOffert, req.Description);
            if (!result.Success)
                return BadRequest(result.ErrorMessage);
            return Ok(result.Grille);
        }

        // DELETE: api/grilles/{id}
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var ok = _service.DeleteGrille(id);
            if (!ok) return NotFound();
            return Ok();
        }

        // POST: api/grilles/{idGrille}/categories — ajoute une catégorie
        [HttpPost("{idGrille}/categories")]
        public IActionResult AddCategorie(int idGrille, [FromBody] CategorieRequest req)
        {
            var result = _service.AddCategorie(idGrille, req.Nom, req.Ponderation);
            if (!result.Success)
                return BadRequest(result.ErrorMessage);
            return Ok(result.Categorie);
        }

        // PUT: api/grilles/categories/{idCategorie}
        [HttpPut("categories/{idCategorie}")]
        public IActionResult UpdateCategorie(int idCategorie, [FromBody] CategorieRequest req)
        {
            var result = _service.UpdateCategorie(idCategorie, req.Nom, req.Ponderation);
            if (!result.Success)
                return BadRequest(result.ErrorMessage);
            return Ok();
        }

        // DELETE: api/grilles/categories/{idCategorie}
        [HttpDelete("categories/{idCategorie}")]
        public IActionResult DeleteCategorie(int idCategorie)
        {
            var ok = _service.DeleteCategorie(idCategorie);
            if (!ok) return NotFound();
            return Ok();
        }
    }

    public class CreateGrilleRequest
    {
        public int IdCoursOffert { get; set; }
        public string? Description { get; set; }
    }

    public class CategorieRequest
    {
        public string Nom { get; set; } = "";
        public decimal Ponderation { get; set; }
    }
}
