using Microsoft.AspNetCore.Mvc;
using Skolaris.Data;
using Skolaris.Models;
using System.Linq;

namespace Skolaris.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class CoursController : ControllerBase
	{
		private readonly ApplicationDbContext _context;

		public CoursController(ApplicationDbContext context)
		{
			_context = context;
		}

		// GET: api/Cours
		[HttpGet]
		public IActionResult GetAll()
		{
			var cours = _context.Cours.ToList();
			return Ok(cours);
		}

		// POST: api/Cours
		[HttpPost]
		public IActionResult Create(Cours cours)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			_context.Cours.Add(cours);
			_context.SaveChanges();

			return CreatedAtAction(nameof(GetById), new { id = cours.IdCours }, cours);
		}

		// GET: api/Cours/{id}
		[HttpGet("{id}")]
		public IActionResult GetById(int id)
		{
			var cours = _context.Cours.Find(id);

			if (cours == null)
				return NotFound();

			return Ok(cours);
		}

		// PUT: api/Cours/{id}
		[HttpPut("{id}")]
		public IActionResult Update(int id, Cours updatedCours)
		{
			if (id != updatedCours.IdCours)
				return BadRequest();

			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			var cours = _context.Cours.Find(id);

			if (cours == null)
				return NotFound();

			cours.IdProgramme = updatedCours.IdProgramme;
			cours.IdNiveau = updatedCours.IdNiveau;
			cours.Nom = updatedCours.Nom;
			cours.Description = updatedCours.Description;
			cours.Credit = updatedCours.Credit;
			cours.Duree = updatedCours.Duree;
			cours.Actif = updatedCours.Actif;

			_context.SaveChanges();

			return Ok(cours);
		}

		// DELETE: api/Cours/{id}
		[HttpDelete("{id}")]
		public IActionResult Delete(int id)
		{
			var cours = _context.Cours.Find(id);

			if (cours == null)
				return NotFound();

			_context.Cours.Remove(cours);
			_context.SaveChanges();

			return Ok();
		}

	}
}
