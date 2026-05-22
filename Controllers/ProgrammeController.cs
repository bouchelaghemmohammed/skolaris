using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Skolaris.Data;
using Skolaris.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;


namespace Skolaris.Controllers
{
	[ApiController] // If request data is invalid, the API automatically returns an HTTP 400 Bad Request with error details
	[Route("api/[controller]")]
	public class ProgrammeController: ControllerBase
	{
		private readonly ApplicationDbContext _context;

		public ProgrammeController(ApplicationDbContext context)
		{
			_context = context;
		}

		// GET: api/Programme
		[HttpGet]
		public IActionResult GetAll() // Returns a list of all the Programmes in db
		{
			var programmes = _context.Programmes.ToList(); // Table: Programmes
			return Ok(programmes);// Http 200
		}

		// POST: api/Programme
		[HttpPost]
		public IActionResult Create(Programme programme) // Instance of Programme is created
		{
			if (!ModelState.IsValid) // Did the incoming request successfully pass all validation rules?
				return BadRequest(ModelState);

			_context.Programmes.Add(programme); // Adding programme to the Programmes table in db
			_context.SaveChanges();

			return CreatedAtAction(nameof(GetById), new { id = programme.IdProgramme }, programme); // Http 201 with the location of the newly created resource
		}

		// GET: api/Programme/{id}
		[HttpGet("{id}")]
		public IActionResult GetById(int id)
		{
			var programme = _context.Programmes.Find(id);

			if (programme == null)
				return NotFound(); // Http 404

			return Ok(programme);
		}

		// PUT: api/Programme/{id}
		[HttpPut("{id}")]
		public IActionResult Update(int id, Programme updatedProgramme)
		{
			if (id != updatedProgramme.IdProgramme)
				return BadRequest();

			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			var programme = _context.Programmes.Find(id);

			if (programme == null)
				return NotFound();

			programme.Nom = updatedProgramme.Nom; // Updating Nom field of the programme

            //programme.Description = updated.Description;

            _context.SaveChanges();

			return Ok(programme);
		}


		// DELETE: api/Programme/{id}
		[HttpDelete ("{id}")]
		public IActionResult Delete(int id)
		{
			var programme = _context.Programmes.Find(id);

			if (programme == null)
				return NotFound();

			_context.Programmes.Remove(programme);
			_context.SaveChanges();

			return Ok();
		}
	}
}

