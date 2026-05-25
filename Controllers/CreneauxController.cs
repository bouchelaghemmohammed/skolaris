using Microsoft.AspNetCore.Mvc;
using Skolaris.Services;

namespace Skolaris.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CreneauxController : ControllerBase
    {
        private readonly EmploiDuTempsService _service;

        public CreneauxController(EmploiDuTempsService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _service.GetAllAsync();
            return Ok(data);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var data = await _service.GetByIdAsync(id);

            if (data == null)
                return NotFound();

            return Ok(data);
        }

        [HttpPost]
        public async Task<IActionResult> Create(EmploiDuTempsCreateDto dto)
        {
            var result = await _service.CreateAsync(dto);

            if (!result.Success)
                return BadRequest(result.Message);

            return Ok(result.Message);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, EmploiDuTempsCreateDto dto)
        {
            var result = await _service.UpdateAsync(id, dto);

            if (!result.Success)
                return BadRequest(result.Message);

            return Ok(result.Message);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _service.DeleteAsync(id);

            if (!deleted)
                return NotFound();

            return Ok("Créneau supprimé.");
        }

        [HttpPatch("{id}/publier")]
        public async Task<IActionResult> Publier(int id)
        {
            var result = await _service.PublierAsync(id);

            if (!result)
                return NotFound();

            return Ok("Créneau publié.");
        }

        [HttpPatch("{id}/depublier")]
        public async Task<IActionResult> Depublier(int id)
        {
            var result = await _service.DepublierAsync(id);

            if (!result)
                return NotFound();

            return Ok("Créneau dépublié.");
        }

        [HttpPatch("publier-tout")]
        public async Task<IActionResult> PublierTout()
        {
            await _service.PublierToutAsync();

            return Ok("Tous les créneaux ont été publiés.");
        }

        [HttpGet("eleve/{idUtilisateur}")]
        public async Task<IActionResult> GetHoraireEleve(int idUtilisateur)
        {
            var data = await _service.GetHoraireEleveAsync(idUtilisateur);

            return Ok(data);
        }

        [HttpGet("enseignant/{idUtilisateur}")]
        public async Task<IActionResult> GetHoraireEnseignant(int idUtilisateur)
        {
            var data = await _service.GetHoraireEnseignantAsync(idUtilisateur);

            return Ok(data);
        }
    }
}