using Microsoft.EntityFrameworkCore;
using Skolaris.Data;

namespace Skolaris.Services
{
    public class DossierAcademiqueService
    {
        private readonly ApplicationDbContext _context;

        public DossierAcademiqueService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Récupère le dossier académique par IdUtilisateur (stocké dans localStorage comme userId).
        /// </summary>
        public DossierAcademiqueViewModel? GetDossierByUtilisateur(int idUtilisateur)
        {
            var eleve = _context.Eleves
                .Include(e => e.Utilisateur)
                .FirstOrDefault(e => e.IdUtilisateur == idUtilisateur);

            if (eleve == null) return null;

            return BuildDossier(eleve.IdEleve, $"{eleve.Utilisateur.Prenom} {eleve.Utilisateur.Nom}");
        }

        private DossierAcademiqueViewModel BuildDossier(int idEleve, string nom)
        {
            var inscriptions = _context.Inscriptions
                .Include(i => i.CoursOffert)
                    .ThenInclude(co => co.Cours)
                .Include(i => i.CoursOffert)
                    .ThenInclude(co => co.Session)
                        .ThenInclude(s => s.AnneeScolaire)
                            .ThenInclude(a => a.Ecole)
                .Include(i => i.CoursOffert)
                    .ThenInclude(co => co.Enseignant)
                        .ThenInclude(e => e!.Utilisateur)
                .Where(i => i.IdEleve == idEleve)
                .OrderByDescending(i => i.CoursOffert.Session.AnneeScolaire.Libelle)
                .ThenBy(i => i.CoursOffert.Session.Libelle)
                .ToList();

            // Récupérer les notes de l'élève
            var notes = _context.Notes
                .Where(n => n.IdEleve == idEleve)
                .ToList();

            var items = inscriptions.Select(i =>
            {
                var note = notes
                    .Where(n => n.IdCoursOffert == i.IdCoursOffert)
                    .OrderByDescending(n => n.Valeur)
                    .FirstOrDefault();

                return new InscriptionViewModel
                {
                    CoursNom = i.CoursOffert.Cours.Nom,
                    CoursCode = i.CoursOffert.Cours.Code ?? "",
                    SessionNom = i.CoursOffert.Session.Type.ToString(),
                    SessionLibelle = i.CoursOffert.Session.Libelle,
                    AnneeLibelle = i.CoursOffert.Session.AnneeScolaire.Libelle,
                    EtablissementNom = i.CoursOffert.Session.AnneeScolaire.Ecole.Nom,
                    EnseignantNom = i.CoursOffert.Enseignant != null
                        ? $"{i.CoursOffert.Enseignant.Utilisateur.Prenom} {i.CoursOffert.Enseignant.Utilisateur.Nom}"
                        : "—",
                    Note = note != null ? note.Valeur.ToString("F1") : "—",
                    ModeEnseignement = i.CoursOffert.ModeEnseignement.ToString(),
                    DateInscription = i.DateInscription
                };
            }).ToList();

            return new DossierAcademiqueViewModel
            {
                EleveId = idEleve,
                EleveNom = nom,
                Inscriptions = items,
                TotalCours = items.Count,
                CoursAvecNote = items.Count(x => x.Note != "—"),
                CoursActifs = items.Count(x => x.Note == "—")
            };
        }
    }

    public class DossierAcademiqueViewModel
    {
        public int EleveId { get; set; }
        public string EleveNom { get; set; } = "";
        public List<InscriptionViewModel> Inscriptions { get; set; } = new();
        public int TotalCours { get; set; }
        public int CoursAvecNote { get; set; }
        public int CoursActifs { get; set; }
    }

    public class InscriptionViewModel
    {
        public string CoursNom { get; set; } = "";
        public string CoursCode { get; set; } = "";
        public string SessionNom { get; set; } = "";
        public string SessionLibelle { get; set; } = "";
        public string AnneeLibelle { get; set; } = "";
        public string EtablissementNom { get; set; } = "";
        public string EnseignantNom { get; set; } = "";
        public string Note { get; set; } = "";
        public string ModeEnseignement { get; set; } = "";
        public DateTime DateInscription { get; set; }
    }
}
