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
        public NoteOperationResult CreateNote(Note note)
        {
            var eleve = _context.Eleves.FirstOrDefault(e => e.IdEleve == note.IdEleve);
            var coursOffert = _context.CoursOfferts.FirstOrDefault(co => co.IdCoursOffert == note.IdCoursOffert);

            if (eleve == null || coursOffert == null)
                return NoteOperationResult.Fail("Élève ou cours introuvable.");

            if (note.Valeur < 0 || note.Valeur > 100)
                return NoteOperationResult.Fail("La note doit être entre 0 et 100.");

            if (note.Ponderation < 0 || note.Ponderation > 100)
                return NoteOperationResult.Fail("La pondération doit être entre 0 et 100.");

            // NOT-10 : verrouillage de la saisie après date limite
            var verrou = VerifierVerrouillage(coursOffert.IdSession);
            if (verrou != null)
                return NoteOperationResult.Fail(verrou);

            if (note.DateEvaluation == default)
                note.DateEvaluation = DateTime.UtcNow;

            _context.Notes.Add(note);
            _context.SaveChanges();
            return NoteOperationResult.Ok();
        }

        // NOT-03 : Modifier une note
        public NoteOperationResult UpdateNote(int id, Note updated)
        {
            var note = _context.Notes.FirstOrDefault(n => n.IdNote == id);
            if (note == null)
                return NoteOperationResult.Fail("Note introuvable.");

            if (updated.Valeur < 0 || updated.Valeur > 100)
                return NoteOperationResult.Fail("La note doit être entre 0 et 100.");

            if (updated.Ponderation < 0 || updated.Ponderation > 100)
                return NoteOperationResult.Fail("La pondération doit être entre 0 et 100.");

            // NOT-10 : verrouillage de la saisie après date limite
            var coursOffert = _context.CoursOfferts.FirstOrDefault(co => co.IdCoursOffert == note.IdCoursOffert);
            if (coursOffert != null)
            {
                var verrou = VerifierVerrouillage(coursOffert.IdSession);
                if (verrou != null)
                    return NoteOperationResult.Fail(verrou);
            }

            note.Valeur = updated.Valeur;
            note.Type = updated.Type;
            note.Description = updated.Description;
            note.Ponderation = updated.Ponderation;
            note.DateEvaluation = updated.DateEvaluation == default ? note.DateEvaluation : updated.DateEvaluation;
            note.Commentaire = updated.Commentaire;

            _context.SaveChanges();
            return NoteOperationResult.Ok();
        }

        // NOT-10 : retourne null si la session permet la saisie, sinon un message d'erreur
        private string? VerifierVerrouillage(int idSession)
        {
            var session = _context.Sessions.FirstOrDefault(s => s.IdSession == idSession);
            if (session == null) return null;
            if (session.DateLimiteSaisieNotes.HasValue && DateTime.UtcNow > session.DateLimiteSaisieNotes.Value)
                return $"La saisie des notes est verrouillée depuis le {session.DateLimiteSaisieNotes.Value:yyyy-MM-dd}.";
            return null;
        }

        // NOT-10 : pose ou retire la date limite de saisie pour une session
        public bool SetDateLimiteSaisie(int idSession, DateTime? dateLimite)
        {
            var session = _context.Sessions.FirstOrDefault(s => s.IdSession == idSession);
            if (session == null) return false;
            session.DateLimiteSaisieNotes = dateLimite;
            _context.SaveChanges();
            return true;
        }

        public bool DeleteNote(int id)
        {
            var note = _context.Notes.FirstOrDefault(n => n.IdNote == id);
            if (note == null)
                return false;

            // Une note peut être référencée par des DetailBulletin (NOT-07).
            // On les supprime d'abord — le bulletin garde sa moyenne historique mais peut être régénéré.
            var details = _context.DetailBulletins.Where(d => d.IdNote == id).ToList();
            if (details.Count > 0)
                _context.DetailBulletins.RemoveRange(details);

            _context.Notes.Remove(note);
            _context.SaveChanges();
            return true;
        }

        // NOT-04 : Calcul automatique de la note finale (moyenne pondérée).
        // Si une grille d'évaluation (NOT-01) existe pour le cours offert et que des notes
        // ont une catégorie assignée, on calcule la moyenne par catégorie puis la moyenne
        // pondérée des catégories. Sinon, on utilise la pondération directe sur chaque note.
        public decimal? CalculerNoteFinale(int idEleve, int idCoursOffert)
        {
            var notes = _context.Notes
                .Where(n => n.IdEleve == idEleve && n.IdCoursOffert == idCoursOffert)
                .ToList();

            if (notes.Count == 0)
                return null;

            // NOT-01 : si la grille existe pour ce cours et que des notes sont rattachées à des catégories
            var grille = _context.GrillesEvaluation
                .Include(g => g.Categories)
                .FirstOrDefault(g => g.IdCoursOffert == idCoursOffert);

            if (grille != null && notes.Any(n => n.IdCategorie.HasValue))
            {
                decimal sommePonderee = 0;
                decimal sommePonderations = 0;
                foreach (var cat in grille.Categories)
                {
                    var notesCat = notes.Where(n => n.IdCategorie == cat.IdCategorie).ToList();
                    if (notesCat.Count == 0) continue;
                    var moyCat = notesCat.Average(n => n.Valeur);
                    sommePonderee += moyCat * cat.Ponderation;
                    sommePonderations += cat.Ponderation;
                }
                if (sommePonderations > 0)
                    return Math.Round(sommePonderee / sommePonderations, 2);
                // pas de catégorie active → on retombe sur la logique par note
            }

            var sommePondsNotes = notes.Sum(n => n.Ponderation);

            if (sommePondsNotes <= 0)
                return Math.Round(notes.Average(n => n.Valeur), 2);

            var sommePondereeNotes = notes.Sum(n => n.Valeur * n.Ponderation);
            return Math.Round(sommePondereeNotes / sommePondsNotes, 2);
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
                    DateEvaluation = n.DateEvaluation,
                    Commentaire = n.Commentaire
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
        public string? Commentaire { get; set; }
    }

    public class NoteOperationResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public static NoteOperationResult Ok() => new() { Success = true };
        public static NoteOperationResult Fail(string error) => new() { Success = false, ErrorMessage = error };
    }
}
