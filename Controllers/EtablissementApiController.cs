using Microsoft.AspNetCore.Mvc;
using Skolaris.Services;

namespace Skolaris.Controllers
{
    [ApiController]
    [Route("api/etablissements")]
    public class EtablissementApiController : ControllerBase
    {
        private readonly EtablissementService _service;

        public EtablissementApiController(EtablissementService service)
        {
            _service = service;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var ecoles = _service.GetAll().Select(e => new
            {
                Id = e.IdEcole,
                e.Nom,
                e.Adresse,
                e.Telephone,
                e.Email,
                e.IsActive
            });
            return Ok(ecoles);
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var e = _service.GetById(id);
            if (e == null) return NotFound();
            return Ok(new { Id = e.IdEcole, e.Nom, e.Adresse, e.Telephone, e.Email, e.IsActive });
        }

        [HttpPost]
        public IActionResult Create([FromBody] EtablissementRequest request)
        {
            var e = _service.Create(request.Nom, request.Adresse, request.Telephone, request.Email);
            return Ok(new { Id = e.IdEcole, e.Nom, e.Adresse, e.Telephone, e.Email, e.IsActive });
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] EtablissementRequest request)
        {
            var success = _service.Update(id, request.Nom, request.Adresse, request.Telephone, request.Email);
            if (!success) return NotFound();
            return Ok();
        }

        [HttpPost("{id}/toggle-active")]
        public IActionResult ToggleActive(int id)
        {
            var success = _service.ToggleActive(id);
            if (!success) return NotFound();
            return Ok();
        }
    }

    public class EtablissementRequest
    {
        public string Nom { get; set; } = "";
        public string? Adresse { get; set; }
        public string? Telephone { get; set; }
        public string? Email { get; set; }
    }
}
