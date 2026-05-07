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

        // NOT-06 : Statistiques de classe pour un cours offert donné.
        // Calcule moyenne, médiane, min, max, distribution par mention et écart-type
        // basés sur la note finale (CalculerNoteFinale) de chaque élève inscrit.
        public StatistiquesClasseViewModel? CalculerStatistiquesClasse(int idCoursOffert)
        {
            var coursOffert = _context.CoursOfferts
                .Include(co => co.Cours)
                .Include(co => co.Groupe)
                .Include(co => co.Session)
                .FirstOrDefault(co => co.IdCoursOffert == idCoursOffert);
            if (coursOffert == null) return null;

            var inscriptionsEleves = _context.Inscriptions
                .Include(i => i.Eleve).ThenInclude(e => e.Utilisateur)
                .Where(i => i.IdCoursOffert == idCoursOffert)
                .ToList();

            var resultats = new List<EleveResultatViewModel>();
            foreach (var i in inscriptionsEleves)
            {
                var moy = CalculerNoteFinale(i.IdEleve, idCoursOffert);
                resultats.Add(new EleveResultatViewModel
                {
                    IdEleve = i.IdEleve,
                    NomComplet = $"{i.Eleve.Utilisateur.Prenom} {i.Eleve.Utilisateur.Nom}",
                    Matricule = i.Eleve.Matricule,
                    NoteFinale = moy
                });
            }

            var notesValides = resultats.Where(r => r.NoteFinale.HasValue).Select(r => r.NoteFinale!.Value).OrderBy(v => v).ToList();

            var stats = new StatistiquesClasseViewModel
            {
                IdCoursOffert = idCoursOffert,
                CoursNom = coursOffert.Cours.Nom,
                CoursCode = coursOffert.Cours.Code ?? "",
                GroupeNom = coursOffert.Groupe.Nom,
                SessionLibelle = coursOffert.Session.Libelle,
                NombreInscrits = resultats.Count,
                NombreNotes = notesValides.Count,
                Eleves = resultats.OrderByDescending(r => r.NoteFinale ?? -1).ToList()
            };

            if (notesValides.Count > 0)
            {
                stats.Moyenne = Math.Round(notesValides.Average(), 2);
                stats.Mediane = CalculerMediane(notesValides);
                stats.Minimum = notesValides.First();
                stats.Maximum = notesValides.Last();
                stats.EcartType = CalculerEcartType(notesValides, stats.Moyenne.Value);

                stats.Distribution = new DistributionMention
                {
                    Excellent = notesValides.Count(v => v >= 90),
                    TresBien = notesValides.Count(v => v >= 80 && v < 90),
                    Bien = notesValides.Count(v => v >= 70 && v < 80),
                    Passable = notesValides.Count(v => v >= 60 && v < 70),
                    Echec = notesValides.Count(v => v < 60)
                };
            }
            else
            {
                stats.Distribution = new DistributionMention();
            }

            return stats;
        }

        private static decimal CalculerMediane(List<decimal> sorted)
        {
            int n = sorted.Count;
            if (n == 0) return 0;
            return n % 2 == 1 ? sorted[n / 2] : Math.Round((sorted[(n / 2) - 1] + sorted[n / 2]) / 2m, 2);
        }

        private static decimal CalculerEcartType(List<decimal> values, decimal moyenne)
        {
            if (values.Count <= 1) return 0;
            var sommeCarres = values.Sum(v => (double)((v - moyenne) * (v - moyenne)));
            return Math.Round((decimal)Math.Sqrt(sommeCarres / values.Count), 2);
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

    // NOT-06
    public class StatistiquesClasseViewModel
    {
        public int IdCoursOffert { get; set; }
        public string CoursNom { get; set; } = "";
        public string CoursCode { get; set; } = "";
        public string GroupeNom { get; set; } = "";
        public string SessionLibelle { get; set; } = "";
        public int NombreInscrits { get; set; }
        public int NombreNotes { get; set; }
        public decimal? Moyenne { get; set; }
        public decimal? Mediane { get; set; }
        public decimal? Minimum { get; set; }
        public decimal? Maximum { get; set; }
        public decimal? EcartType { get; set; }
        public DistributionMention Distribution { get; set; } = new();
        public List<EleveResultatViewModel> Eleves { get; set; } = new();
    }

    public class DistributionMention
    {
        public int Excellent { get; set; }
        public int TresBien { get; set; }
        public int Bien { get; set; }
        public int Passable { get; set; }
        public int Echec { get; set; }
    }

    public class EleveResultatViewModel
    {
        public int IdEleve { get; set; }
        public string NomComplet { get; set; } = "";
        public string Matricule { get; set; } = "";
        public decimal? NoteFinale { get; set; }
    }
}
