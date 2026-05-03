using Microsoft.EntityFrameworkCore;
using Skolaris.Data;
using Skolaris.Enums;
using Skolaris.Models;

namespace Skolaris.Services
{
    public class BulletinService
    {
        private readonly ApplicationDbContext _context;
        private readonly NoteService _noteService;

        public BulletinService(ApplicationDbContext context, NoteService noteService)
        {
            _context = context;
            _noteService = noteService;
        }

        public List<Bulletin> GetAllBulletins()
        {
            return _context.Bulletins.ToList();
        }

        public Bulletin? GetBulletinById(int id)
        {
            return _context.Bulletins
                .Include(b => b.Eleve).ThenInclude(e => e.Utilisateur)
                .Include(b => b.Session).ThenInclude(s => s.AnneeScolaire)
                .Include(b => b.DetailBulletins).ThenInclude(d => d.Note)
                .Include(b => b.DetailBulletins).ThenInclude(d => d.CoursOffert).ThenInclude(co => co.Cours)
                .FirstOrDefault(b => b.IdBulletin == id);
        }

        public List<Bulletin> GetBulletinsByEleve(int idEleve)
        {
            return _context.Bulletins
                .Include(b => b.Session).ThenInclude(s => s.AnneeScolaire)
                .Where(b => b.IdEleve == idEleve)
                .OrderByDescending(b => b.Session.AnneeScolaire.Libelle)
                .ThenBy(b => b.Session.Libelle)
                .ToList();
        }

        public Bulletin? GetBulletinByEleveAndSession(int idEleve, int idSession)
        {
            return _context.Bulletins
                .Include(b => b.DetailBulletins).ThenInclude(d => d.Note)
                .Include(b => b.DetailBulletins).ThenInclude(d => d.CoursOffert).ThenInclude(co => co.Cours)
                .FirstOrDefault(b => b.IdEleve == idEleve && b.IdSession == idSession);
        }

        // NOT-07 : Génère (ou régénère) le bulletin d'un élève pour une session.
        // Calcule la moyenne pondérée par cours, la moyenne générale et la mention.
        public BulletinResult GenererBulletin(int idEleve, int idSession)
        {
            var eleve = _context.Eleves
                .Include(e => e.Utilisateur)
                .FirstOrDefault(e => e.IdEleve == idEleve);
            if (eleve == null)
                return BulletinResult.Fail("Élève introuvable.");

            var session = _context.Sessions.FirstOrDefault(s => s.IdSession == idSession);
            if (session == null)
                return BulletinResult.Fail("Session introuvable.");

            var coursOfferts = _context.CoursOfferts
                .Where(co => co.IdSession == idSession)
                .Where(co => _context.Inscriptions.Any(i => i.IdEleve == idEleve && i.IdCoursOffert == co.IdCoursOffert))
                .ToList();

            if (coursOfferts.Count == 0)
                return BulletinResult.Fail("Aucune inscription pour cet élève dans cette session.");

            // Construire ou réutiliser le bulletin
            var bulletin = _context.Bulletins
                .Include(b => b.DetailBulletins)
                .FirstOrDefault(b => b.IdEleve == idEleve && b.IdSession == idSession);

            if (bulletin != null)
            {
                _context.DetailBulletins.RemoveRange(bulletin.DetailBulletins);
            }
            else
            {
                bulletin = new Bulletin
                {
                    IdEleve = idEleve,
                    IdSession = idSession,
                    DetailBulletins = new List<DetailBulletin>()
                };
                _context.Bulletins.Add(bulletin);
            }

            decimal sommeMoyennes = 0;
            int nbCours = 0;
            var details = new List<DetailBulletin>();

            foreach (var co in coursOfferts)
            {
                var notesDuCours = _context.Notes
                    .Where(n => n.IdEleve == idEleve && n.IdCoursOffert == co.IdCoursOffert)
                    .ToList();

                if (notesDuCours.Count == 0)
                    continue;

                var moyenne = _noteService.CalculerNoteFinale(idEleve, co.IdCoursOffert);
                if (moyenne == null)
                    continue;

                sommeMoyennes += moyenne.Value;
                nbCours++;

                // Une ligne de détail par note de chaque cours
                foreach (var note in notesDuCours)
                {
                    details.Add(new DetailBulletin
                    {
                        IdNote = note.IdNote,
                        IdCoursOffert = co.IdCoursOffert,
                        Bulletin = bulletin
                    });
                }
            }

            if (nbCours == 0)
                return BulletinResult.Fail("Aucune note disponible pour générer le bulletin.");

            bulletin.MoyenneGeneral = Math.Round(sommeMoyennes / nbCours, 2);
            bulletin.Mention = DeterminerMention(bulletin.MoyenneGeneral);

            foreach (var d in details)
                bulletin.DetailBulletins.Add(d);

            _context.SaveChanges();

            return BulletinResult.Ok(bulletin);
        }

        // NOT-11 : Bulletins enrichis pour un utilisateur donné.
        public List<BulletinEleveViewModel> GetBulletinsEnrichisByUtilisateur(int idUtilisateur)
        {
            var eleve = _context.Eleves.FirstOrDefault(e => e.IdUtilisateur == idUtilisateur);
            if (eleve == null) return new List<BulletinEleveViewModel>();

            return _context.Bulletins
                .Include(b => b.Session).ThenInclude(s => s.AnneeScolaire)
                .Where(b => b.IdEleve == eleve.IdEleve)
                .OrderByDescending(b => b.Session.AnneeScolaire.Libelle)
                .ThenBy(b => b.Session.Libelle)
                .Select(b => new BulletinEleveViewModel
                {
                    IdBulletin = b.IdBulletin,
                    SessionLibelle = b.Session.Libelle,
                    SessionType = b.Session.Type.ToString(),
                    AnneeLibelle = b.Session.AnneeScolaire.Libelle,
                    MoyenneGenerale = b.MoyenneGeneral,
                    Mention = b.Mention.ToString()
                })
                .ToList();
        }

        public bool DeleteBulletin(int id)
        {
            var bulletin = _context.Bulletins
                .Include(b => b.DetailBulletins)
                .FirstOrDefault(b => b.IdBulletin == id);

            if (bulletin == null)
                return false;

            _context.DetailBulletins.RemoveRange(bulletin.DetailBulletins);
            _context.Bulletins.Remove(bulletin);
            _context.SaveChanges();
            return true;
        }

        private static Mention DeterminerMention(decimal moyenne)
        {
            if (moyenne >= 90) return Mention.Excellent;
            if (moyenne >= 80) return Mention.TresBien;
            if (moyenne >= 70) return Mention.Bien;
            if (moyenne >= 60) return Mention.Passable;
            return Mention.Echec;
        }
    }

    public class BulletinResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public Bulletin? Bulletin { get; set; }

        public static BulletinResult Ok(Bulletin bulletin) => new() { Success = true, Bulletin = bulletin };
        public static BulletinResult Fail(string error) => new() { Success = false, ErrorMessage = error };
    }

    public class BulletinEleveViewModel
    {
        public int IdBulletin { get; set; }
        public string SessionLibelle { get; set; } = "";
        public string SessionType { get; set; } = "";
        public string AnneeLibelle { get; set; } = "";
        public decimal MoyenneGenerale { get; set; }
        public string Mention { get; set; } = "";
    }
}
