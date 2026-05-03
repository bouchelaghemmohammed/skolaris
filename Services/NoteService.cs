using Microsoft.EntityFrameworkCore;
using Skolaris.Data;
using Skolaris.Models;

namespace Skolaris.Services
{
    public class NoteService
    {
        private readonly ApplicationDbContext _context;

        public NoteService(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Note> GetAllNotes()
        {
            return _context.Notes.ToList();
        }

        public Note? GetNoteById(int id)
        {
            return _context.Notes.FirstOrDefault(n => n.IdNote == id);
        }

        public List<Note> GetNotesByEleve(int idEleve)
        {
            return _context.Notes
                .Where(n => n.IdEleve == idEleve)
                .OrderByDescending(n => n.DateEvaluation)
                .ToList();
        }

        public List<Note> GetNotesByCoursOffert(int idCoursOffert)
        {
            return _context.Notes
                .Where(n => n.IdCoursOffert == idCoursOffert)
                .OrderByDescending(n => n.DateEvaluation)
                .ToList();
        }

        public List<Note> GetNotesByEleveAndCoursOffert(int idEleve, int idCoursOffert)
        {
            return _context.Notes
                .Where(n => n.IdEleve == idEleve && n.IdCoursOffert == idCoursOffert)
                .OrderBy(n => n.DateEvaluation)
                .ToList();
        }

        // NOT-02 : Saisir une note
        public bool CreateNote(Note note)
        {
            var eleve = _context.Eleves.FirstOrDefault(e => e.IdEleve == note.IdEleve);
            var coursOffert = _context.CoursOfferts.FirstOrDefault(co => co.IdCoursOffert == note.IdCoursOffert);

            if (eleve == null || coursOffert == null)
                return false;

            if (note.Valeur < 0 || note.Valeur > 100)
                return false;

            if (note.Ponderation < 0 || note.Ponderation > 100)
                return false;

            if (note.DateEvaluation == default)
                note.DateEvaluation = DateTime.UtcNow;

            _context.Notes.Add(note);
            _context.SaveChanges();
            return true;
        }

        // NOT-03 : Modifier une note
        public bool UpdateNote(int id, Note updated)
        {
            var note = _context.Notes.FirstOrDefault(n => n.IdNote == id);
            if (note == null)
                return false;

            if (updated.Valeur < 0 || updated.Valeur > 100)
                return false;

            if (updated.Ponderation < 0 || updated.Ponderation > 100)
                return false;

            note.Valeur = updated.Valeur;
            note.Type = updated.Type;
            note.Description = updated.Description;
            note.Ponderation = updated.Ponderation;
            note.DateEvaluation = updated.DateEvaluation == default ? note.DateEvaluation : updated.DateEvaluation;

            _context.SaveChanges();
            return true;
        }

        public bool DeleteNote(int id)
        {
            var note = _context.Notes.FirstOrDefault(n => n.IdNote == id);
            if (note == null)
                return false;

            _context.Notes.Remove(note);
            _context.SaveChanges();
            return true;
        }

        // NOT-04 : Calcul automatique de la note finale (moyenne pondérée).
        // Si la somme des pondérations est 0, on retombe sur une moyenne simple.
        public decimal? CalculerNoteFinale(int idEleve, int idCoursOffert)
        {
            var notes = _context.Notes
                .Where(n => n.IdEleve == idEleve && n.IdCoursOffert == idCoursOffert)
                .ToList();

            if (notes.Count == 0)
                return null;

            var sommePonderations = notes.Sum(n => n.Ponderation);

            if (sommePonderations <= 0)
                return Math.Round(notes.Average(n => n.Valeur), 2);

            var sommePonderee = notes.Sum(n => n.Valeur * n.Ponderation);
            return Math.Round(sommePonderee / sommePonderations, 2);
        }

        // NOT-11 : Notes enrichies (avec libellés) pour un utilisateur donné.
        public List<NoteEleveViewModel> GetNotesEnrichiesByUtilisateur(int idUtilisateur)
        {
            var eleve = _context.Eleves.FirstOrDefault(e => e.IdUtilisateur == idUtilisateur);
            if (eleve == null) return new List<NoteEleveViewModel>();

            return _context.Notes
                .Include(n => n.CoursOffert).ThenInclude(co => co.Cours)
                .Include(n => n.CoursOffert).ThenInclude(co => co.Session)
                .Where(n => n.IdEleve == eleve.IdEleve)
                .OrderByDescending(n => n.DateEvaluation)
                .Select(n => new NoteEleveViewModel
                {
                    IdNote = n.IdNote,
                    CoursNom = n.CoursOffert.Cours.Nom,
                    CoursCode = n.CoursOffert.Cours.Code ?? "",
                    SessionLibelle = n.CoursOffert.Session.Libelle,
                    Type = n.Type.ToString(),
                    Description = n.Description,
                    Valeur = n.Valeur,
                    Ponderation = n.Ponderation,
                    DateEvaluation = n.DateEvaluation
                })
                .ToList();
        }
    }

    public class NoteEleveViewModel
    {
        public int IdNote { get; set; }
        public string CoursNom { get; set; } = "";
        public string CoursCode { get; set; } = "";
        public string SessionLibelle { get; set; } = "";
        public string Type { get; set; } = "";
        public string? Description { get; set; }
        public decimal Valeur { get; set; }
        public decimal Ponderation { get; set; }
        public DateTime DateEvaluation { get; set; }
    }
}
