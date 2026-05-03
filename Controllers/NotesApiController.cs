using Microsoft.AspNetCore.Mvc;
using Skolaris.Models;
using Skolaris.Services;

namespace Skolaris.Controllers
{
    [ApiController]
    [Route("api/notes")]
    public class NotesApiController : ControllerBase
    {
        private readonly NoteService _noteService;

        public NotesApiController(NoteService noteService)
        {
            _noteService = noteService;
        }

        // GET: api/notes
        [HttpGet]
        public IActionResult GetNotes()
        {
            return Ok(_noteService.GetAllNotes());
        }

        // GET: api/notes/{id}
        [HttpGet("{id}")]
        public IActionResult GetNote(int id)
        {
            var note = _noteService.GetNoteById(id);
            if (note == null)
                return NotFound();
            return Ok(note);
        }

        // GET: api/notes/eleve/{idEleve} — NOT-11
        [HttpGet("eleve/{idEleve}")]
        public IActionResult GetNotesByEleve(int idEleve)
        {
            return Ok(_noteService.GetNotesByEleve(idEleve));
        }

        // GET: api/notes/utilisateur/{idUtilisateur} — NOT-11 (vue enrichie élève)
        [HttpGet("utilisateur/{idUtilisateur}")]
        public IActionResult GetNotesByUtilisateur(int idUtilisateur)
        {
            return Ok(_noteService.GetNotesEnrichiesByUtilisateur(idUtilisateur));
        }

        // GET: api/notes/cours/{idCoursOffert}
        [HttpGet("cours/{idCoursOffert}")]
        public IActionResult GetNotesByCoursOffert(int idCoursOffert)
        {
            return Ok(_noteService.GetNotesByCoursOffert(idCoursOffert));
        }

        // GET: api/notes/eleve/{idEleve}/cours/{idCoursOffert}
        [HttpGet("eleve/{idEleve}/cours/{idCoursOffert}")]
        public IActionResult GetNotesByEleveAndCoursOffert(int idEleve, int idCoursOffert)
        {
            return Ok(_noteService.GetNotesByEleveAndCoursOffert(idEleve, idCoursOffert));
        }

        // GET: api/notes/eleve/{idEleve}/cours/{idCoursOffert}/finale — NOT-04
        [HttpGet("eleve/{idEleve}/cours/{idCoursOffert}/finale")]
        public IActionResult GetNoteFinale(int idEleve, int idCoursOffert)
        {
            var finale = _noteService.CalculerNoteFinale(idEleve, idCoursOffert);
            if (finale == null)
                return NotFound("Aucune note pour cet élève dans ce cours.");
            return Ok(new { idEleve, idCoursOffert, noteFinale = finale });
        }

        // POST: api/notes — NOT-02
        [HttpPost]
        public IActionResult CreateNote([FromBody] Note note)
        {
            var result = _noteService.CreateNote(note);
            if (!result)
                return BadRequest("Élève ou cours introuvable, ou note/pondération hors plage (0-100).");
            return Ok(note);
        }

        // PUT: api/notes/{id} — NOT-03
        [HttpPut("{id}")]
        public IActionResult UpdateNote(int id, [FromBody] Note note)
        {
            var result = _noteService.UpdateNote(id, note);
            if (!result)
                return NotFound("Note introuvable ou valeurs invalides.");
            return Ok();
        }

        // DELETE: api/notes/{id}
        [HttpDelete("{id}")]
        public IActionResult DeleteNote(int id)
        {
            var result = _noteService.DeleteNote(id);
            if (!result)
                return NotFound();
            return Ok();
        }
    }
}
