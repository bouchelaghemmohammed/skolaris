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
        private readonly EmailService _emailService;

        public BulletinService(ApplicationDbContext context, NoteService noteService, EmailService emailService)
        {
            _context = context;
            _noteService = noteService;
            _emailService = emailService;
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

        // NOT-08 : Génère le bulletin de tous les élèves d'un groupe pour une session.
        // Réutilise GenererBulletin pour chaque élève. Retourne un résumé par élève.
        public BulletinLotResult GenererBulletinsLot(int idGroupe, int idSession)
        {
            var groupe = _context.Groupes.FirstOrDefault(g => g.IdGroupe == idGroupe);
            if (groupe == null)
                return BulletinLotResult.Fail("Groupe introuvable.");

            var session = _context.Sessions.FirstOrDefault(s => s.IdSession == idSession);
            if (session == null)
                return BulletinLotResult.Fail("Session introuvable.");

            var eleves = _context.Eleves
                .Include(e => e.Utilisateur)
                .Where(e => e.IdGroupe == idGroupe)
                .ToList();

            if (eleves.Count == 0)
                return BulletinLotResult.Fail("Aucun élève dans ce groupe.");

            var entries = new List<BulletinLotEntry>();
            foreach (var eleve in eleves)
            {
                var result = GenererBulletin(eleve.IdEleve, idSession);
                entries.Add(new BulletinLotEntry
                {
                    IdEleve = eleve.IdEleve,
                    NomComplet = $"{eleve.Utilisateur.Prenom} {eleve.Utilisateur.Nom}",
                    Success = result.Success,
                    Message = result.Success ? "Bulletin généré." : result.ErrorMessage,
                    IdBulletin = result.Bulletin?.IdBulletin,
                    MoyenneGenerale = result.Bulletin?.MoyenneGeneral,
                    Mention = result.Bulletin?.Mention.ToString()
                });
            }

            return new BulletinLotResult
            {
                Success = true,
                IdGroupe = idGroupe,
                IdSession = idSession,
                NombreEleves = eleves.Count,
                NombreReussites = entries.Count(e => e.Success),
                Entries = entries
            };
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

        // NOT-09 : Envoie le bulletin par courriel (HTML) à l'élève.
        public async Task<EnvoiBulletinResult> EnvoyerBulletinParCourriel(int idBulletin)
        {
            var bulletin = _context.Bulletins
                .Include(b => b.Eleve).ThenInclude(e => e.Utilisateur)
                .Include(b => b.Session).ThenInclude(s => s.AnneeScolaire)
                .Include(b => b.DetailBulletins).ThenInclude(d => d.Note)
                .Include(b => b.DetailBulletins).ThenInclude(d => d.CoursOffert).ThenInclude(co => co.Cours)
                .FirstOrDefault(b => b.IdBulletin == idBulletin);

            if (bulletin == null)
                return new EnvoiBulletinResult { Success = false, ErrorMessage = "Bulletin introuvable." };

            var email = bulletin.Eleve.Utilisateur.Email;
            var nomComplet = $"{bulletin.Eleve.Utilisateur.Prenom} {bulletin.Eleve.Utilisateur.Nom}";
            var subject = $"Votre bulletin — {bulletin.Session.Libelle} {bulletin.Session.AnneeScolaire.Libelle}";
            var html = BuildBulletinHtml(bulletin, nomComplet);

            var sent = await _emailService.SendEmailAsync(email, nomComplet, subject, html);
            if (!sent)
                return new EnvoiBulletinResult { Success = false, ErrorMessage = $"L'envoi a échoué (vérifier la configuration SMTP). Le bulletin existe pour {email}." };

            return new EnvoiBulletinResult { Success = true, EmailDestinataire = email };
        }

        // Aperçu HTML du bulletin (utilisé par /api/bulletins/{id}/preview pour visualisation navigateur)
        public string? GetBulletinHtml(int idBulletin)
        {
            var bulletin = _context.Bulletins
                .Include(b => b.Eleve).ThenInclude(e => e.Utilisateur)
                .Include(b => b.Session).ThenInclude(s => s.AnneeScolaire)
                .Include(b => b.DetailBulletins).ThenInclude(d => d.Note)
                .Include(b => b.DetailBulletins).ThenInclude(d => d.CoursOffert).ThenInclude(co => co.Cours)
                .FirstOrDefault(b => b.IdBulletin == idBulletin);

            if (bulletin == null) return null;

            var nomComplet = $"{bulletin.Eleve.Utilisateur.Prenom} {bulletin.Eleve.Utilisateur.Nom}";
            return BuildBulletinHtml(bulletin, nomComplet);
        }

        private string BuildBulletinHtml(Bulletin bulletin, string nomComplet)
        {
            // Regrouper les détails par cours pour afficher proprement
            var parCours = bulletin.DetailBulletins
                .GroupBy(d => new { d.IdCoursOffert, CoursNom = d.CoursOffert.Cours.Nom, CoursCode = d.CoursOffert.Cours.Code })
                .Select(g => new
                {
                    g.Key.CoursNom,
                    g.Key.CoursCode,
                    Notes = g.Select(d => d.Note).ToList()
                })
                .ToList();

            var coursRows = string.Join("", parCours.Select(c =>
            {
                var notesRows = string.Join("", c.Notes.Select(n =>
                    $"<tr><td style='padding:4px 8px; color:#666;'>{n.Type} — {(string.IsNullOrEmpty(n.Description) ? "—" : System.Net.WebUtility.HtmlEncode(n.Description))}</td><td style='padding:4px 8px; text-align:right;'>{n.Ponderation:F2}%</td><td style='padding:4px 8px; text-align:right; font-weight:bold;'>{n.Valeur:F2}</td></tr>"));
                var moyCours = _noteService.CalculerNoteFinale(bulletin.IdEleve, c.Notes.First().IdCoursOffert);
                return $@"
                    <h4 style='color:#1565c0; margin-top:24px; margin-bottom:8px;'>{System.Net.WebUtility.HtmlEncode(c.CoursCode ?? "")} — {System.Net.WebUtility.HtmlEncode(c.CoursNom)}</h4>
                    <table style='width:100%; border-collapse:collapse; font-size:14px;'>
                        <thead><tr style='background:#e3f2fd; color:#1565c0;'><th style='padding:6px 8px; text-align:left;'>Évaluation</th><th style='padding:6px 8px; text-align:right;'>Pondération</th><th style='padding:6px 8px; text-align:right;'>Note</th></tr></thead>
                        <tbody>{notesRows}</tbody>
                        <tfoot><tr style='background:#fafafa;'><td colspan='2' style='padding:6px 8px; text-align:right; font-weight:bold;'>Moyenne du cours</td><td style='padding:6px 8px; text-align:right; font-weight:bold; color:#1976d2;'>{(moyCours.HasValue ? moyCours.Value.ToString("F2") : "—")}</td></tr></tfoot>
                    </table>";
            }));

            return $@"<!DOCTYPE html>
<html>
<head><meta charset='utf-8'></head>
<body style='font-family:Arial,sans-serif; background:#f0f7ff; margin:0; padding:20px;'>
  <div style='max-width:700px; margin:0 auto; background:white; border-radius:12px; padding:35px; box-shadow:0 4px 20px rgba(21,101,192,0.15);'>
    <div style='text-align:center; margin-bottom:25px;'>
      <div style='display:inline-block; background:#1976d2; color:white; border-radius:10px; width:48px; height:48px; line-height:48px; font-size:24px; font-weight:bold;'>S</div>
      <h2 style='color:#1565c0; margin-top:12px;'>Bulletin scolaire — Skolaris</h2>
    </div>
    <h3 style='color:#1976d2;'>Bonjour {System.Net.WebUtility.HtmlEncode(nomComplet)},</h3>
    <p style='color:#444;'>Voici votre bulletin officiel pour la session <strong>{System.Net.WebUtility.HtmlEncode(bulletin.Session.Libelle)} {System.Net.WebUtility.HtmlEncode(bulletin.Session.AnneeScolaire.Libelle)}</strong>.</p>
    <div style='background:#e3f2fd; border-radius:8px; padding:16px; margin:24px 0; text-align:center;'>
      <div style='color:#777; font-size:13px;'>Moyenne générale</div>
      <div style='color:#1565c0; font-size:42px; font-weight:bold;'>{bulletin.MoyenneGeneral:F2}</div>
      <div style='color:#1976d2; font-size:18px; font-weight:bold;'>{bulletin.Mention}</div>
    </div>
    {coursRows}
    <hr style='border:none; border-top:1px solid #e3f2fd; margin:30px 0;'>
    <p style='color:#999; font-size:12px; text-align:center;'>© Skolaris — Plateforme scolaire — Bulletin #{bulletin.IdBulletin}</p>
  </div>
</body>
</html>";
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

    public class BulletinLotResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public int IdGroupe { get; set; }
        public int IdSession { get; set; }
        public int NombreEleves { get; set; }
        public int NombreReussites { get; set; }
        public List<BulletinLotEntry> Entries { get; set; } = new();

        public static BulletinLotResult Fail(string error) => new() { Success = false, ErrorMessage = error };
    }

    public class BulletinLotEntry
    {
        public int IdEleve { get; set; }
        public string NomComplet { get; set; } = "";
        public bool Success { get; set; }
        public string? Message { get; set; }
        public int? IdBulletin { get; set; }
        public decimal? MoyenneGenerale { get; set; }
        public string? Mention { get; set; }
    }

    public class EnvoiBulletinResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public string? EmailDestinataire { get; set; }
    }
}
