using Microsoft.AspNetCore.Mvc;
using Skolaris.Enums;
using Skolaris.Services;

namespace Skolaris.Controllers
{
    [ApiController]
    [Route("api/sessions")]
    public class SessionAcademiqueApiController : ControllerBase
    {
        private readonly SessionAcademiqueService _service;

        public SessionAcademiqueApiController(SessionAcademiqueService service)
        {
            _service = service;
        }

        [HttpGet]
        public IActionResult GetAll() => Ok(_service.GetAll());

        [HttpGet("annee/{idAnnee}")]
        public IActionResult GetByAnnee(int idAnnee) => Ok(_service.GetByAnnee(idAnnee));

        [HttpPost]
        public IActionResult Create([FromBody] SessionCreateRequest request)
        {
            if (!Enum.TryParse<TypeSession>(request.Type, true, out var type))
                return BadRequest("Type de session invalide (Automne, Hiver, Ete).");

            var session = _service.Create(request.Libelle, type, request.IdAnnee);
            return Ok(new
            {
                session.IdSession,
                session.Libelle,
                Type = session.Type.ToString(),
                session.IsActive,
                session.IdAnnee
            });
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] SessionUpdateRequest request)
        {
            if (!Enum.TryParse<TypeSession>(request.Type, true, out var type))
                return BadRequest("Type de session invalide.");

            var success = _service.Update(id, request.Libelle, type, request.IsActive);
            if (!success) return NotFound();
            return Ok();
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var success = _service.Delete(id);
            if (!success) return NotFound();
            return Ok();
        }
    }

    public class SessionCreateRequest
    {
        public string Libelle { get; set; } = "";
        public string Type { get; set; } = "";  // "Automne", "Hiver", "Ete"
        public int IdAnnee { get; set; }
    }

    public class SessionUpdateRequest
    {
        public string Libelle { get; set; } = "";
        public string Type { get; set; } = "";
        public bool IsActive { get; set; }
    }
}
