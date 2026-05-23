using Microsoft.AspNetCore.Identity;
using Skolaris.Enums;
using Skolaris.Models;

namespace Skolaris.Data
{
    public static class DataSeeder
    {
        public static void Seed(ApplicationDbContext context)
        {
            if (context.Programmes.Any())
            {
                return;
            }

            var hasher = new PasswordHasher<Utilisateur>();

            // ===== UTILISATEURS =====

            // Admin
            if (!context.Utilisateurs.Any(u => u.Email == "admin@gmail.com"))
            {
                var admin = new Utilisateur
                {
                    Nom = "Admin",
                    Prenom = "Skolaris",
                    Email = "admin@gmail.com",
                    Role = Role.ADMIN,
                    IsActive = true
                };
                admin.MotDePasse = hasher.HashPassword(admin, "123456");
                context.Utilisateurs.Add(admin);
                context.SaveChanges();
            }

            // Enseignant
            if (!context.Utilisateurs.Any(u => u.Role == Role.ENSEIGNANT))
            {
                var enseignantsData = new[]
                {
                    new { Prenom = "Jean", Nom = "Tremblay", Email = "jean.tremblay@gmail.com" },
                    new { Prenom = "Marie", Nom = "Gagnon", Email = "marie.gagnon@gmail.com" },
                    new { Prenom = "Patrick", Nom = "Roy", Email = "patrick.roy@gmail.com" },
                    new { Prenom = "Sophie", Nom = "Bouchard", Email = "sophie.bouchard@gmail.com" },
                    new { Prenom = "David", Nom = "Lefebvre", Email = "david.lefebvre@gmail.com" },
                    new { Prenom = "Isabelle", Nom = "Morin", Email = "isabelle.morin@gmail.com" },
                    new { Prenom = "Alexandre", Nom = "Gauthier", Email = "alexandre.gauthier@gmail.com" },
                    new { Prenom = "Catherine", Nom = "Pelletier", Email = "catherine.pelletier@gmail.com" },
                    new { Prenom = "Nicolas", Nom = "Bergeron", Email = "nicolas.bergeron@gmail.com" },
                    new { Prenom = "Julie", Nom = "Caron", Email = "julie.caron@gmail.com" }
                };

                foreach (var e in enseignantsData)
                {
                    var utilisateur = new Utilisateur
                    {
                        Nom = e.Nom,
                        Prenom = e.Prenom,
                        Email = e.Email,
                        Role = Role.ENSEIGNANT,
                        IsActive = true
                    };

                    utilisateur.MotDePasse = hasher.HashPassword(utilisateur, "123456");

                    context.Utilisateurs.Add(utilisateur);
                    context.SaveChanges();

                    var enseignant = new Enseignant
                    {
                        IdUtilisateur = utilisateur.IdUtilisateur
                    };

                    context.Enseignants.Add(enseignant);
                    context.SaveChanges();
                }
            }

            // Élève 
            if (!context.Utilisateurs.Any(u => u.Email == "eleve@gmail.com"))
            {
                var eleve = new Utilisateur
                {
                    Nom = "Eleve",
                    Prenom = "Skolaris",
                    Email = "eleve@gmail.com",
                    Role = Role.ELEVE,
                    IsActive = true
                };
                eleve.MotDePasse = hasher.HashPassword(eleve, "123456");
                context.Utilisateurs.Add(eleve);
                context.SaveChanges();
            }

            // ===== STRUCTURE ACADÉMIQUE =====

            // École
            if (!context.Ecoles.Any())
            {
                context.Ecoles.Add(new Ecole
                {
                    Nom = "Cégep Skolaris",
                    Adresse = "123 Montreal, QC",
                    Telephone = "450-555-1234",
                    Email = "info@skolaris.qc.ca",
                    IsActive = true
                });
                context.SaveChanges();
            }

            // Année scolaire
            if (!context.AnneesScolaires.Any())
            {
                var ecole = context.Ecoles.First();
                context.AnneesScolaires.Add(new AnneeScolaire
                {
                    Libelle = "2025-2026",
                    IdEcole = ecole.IdEcole
                });
                context.SaveChanges();
            }

            // Session
            if (!context.Sessions.Any())
            {
                var annee = context.AnneesScolaires.First();
                context.Sessions.Add(new Session
                {
                    Libelle = "Hiver 2026",
                    Type = TypeSession.Hiver,
                    IdAnnee = annee.IdAnnee,
                    IsActive = true
                });
                context.SaveChanges();
            }

            // Programme
            if (!context.Programmes.Any())
            {
                context.Programmes.Add(new Programme
                {
                    Nom = "Techniques de l'informatique"
                });
                context.SaveChanges();
            }

            // Niveaux (4 par défaut)
            {
                var programme = context.Programmes.First();
                var nomsNiveaux = new[] { "Niveau 1", "Niveau 2", "Niveau 3", "Niveau 4" };
                foreach (var nom in nomsNiveaux)
                {
                    if (!context.Niveaux.Any(n => n.Nom == nom && n.IdProgramme == programme.IdProgramme))
                    {
                        context.Niveaux.Add(new Niveau { Nom = nom, IdProgramme = programme.IdProgramme });
                    }
                }
                context.SaveChanges();
            }

            // Groupe
            if (!context.Groupes.Any())
            {
                var programme = context.Programmes.First();
                context.Groupes.Add(new Groupe
                {
                    Nom = "Groupe 01",
                    IdProgramme = programme.IdProgramme
                });
                context.SaveChanges();
            }

            // Cours
            if (!context.Cours.Any())
            {
                var programme = context.Programmes.First();
                var niveau = context.Niveaux.First();
                context.Cours.Add(new Cours
                {
                    Nom = "Programmation Web Côté Serveur",
                    Code = "420-2W4-MA",
                    Description = "Développement d'applications web côté serveur",
                    IdProgramme = programme.IdProgramme,
                    IdNiveau = niveau.IdNiveau
                });
                context.SaveChanges();
            }

            // ===== ENSEIGNANT (entité liée à l'utilisateur prof) =====
            var profUser = context.Utilisateurs.FirstOrDefault(u => u.Email == "enseignant@gmail.com");
            if (profUser != null && !context.Enseignants.Any(e => e.IdUtilisateur == profUser.IdUtilisateur))
            {
                context.Enseignants.Add(new Enseignant
                {
                    IdUtilisateur = profUser.IdUtilisateur
                });
                context.SaveChanges();
            }

            // ===== ÉLÈVE (entité liée à l'utilisateur eleve) =====
            var eleveUser = context.Utilisateurs.FirstOrDefault(u => u.Email == "eleve@gmail.com");
            if (eleveUser != null && !context.Eleves.Any(e => e.IdUtilisateur == eleveUser.IdUtilisateur))
            {
                var programme = context.Programmes.First();
                var groupe = context.Groupes.First();
                var niveau = context.Niveaux.First();

                context.Eleves.Add(new Eleve
                {
                    Matricule = "2024001",
                    IdUtilisateur = eleveUser.IdUtilisateur,
                    IdProgramme = programme.IdProgramme,
                    IdGroupe = groupe.IdGroupe,
                    IdNiveau = niveau.IdNiveau
                });
                context.SaveChanges();
            }

            // ===== COURS OFFERT (lie cours + groupe + session + enseignant) =====
            if (!context.CoursOfferts.Any())
            {
                var cours = context.Cours.First();
                var groupe = context.Groupes.First();
                var session = context.Sessions.First();
                var enseignant = context.Enseignants.FirstOrDefault();

                context.CoursOfferts.Add(new CoursOffert
                {
                    IdCours = cours.IdCours,
                    IdGroupe = groupe.IdGroupe,
                    IdSession = session.IdSession,
                    IdEnseignant = enseignant?.IdEnseignant,
                    ModeEnseignement = ModeEnseignement.Présentiel
                });
                context.SaveChanges();
            }

            // ===== EMPLOI DU TEMPS (créneaux horaires) =====
            if (!context.EmploisDuTemps.Any())
            {
                var coursOffert = context.CoursOfferts.FirstOrDefault();
                if (coursOffert != null)
                {
                    context.EmploisDuTemps.AddRange(
                        new EmploiDuTemps
                        {
                            IdCoursOffert = coursOffert.IdCoursOffert,
                            JourSemaine = JourSemaine.Lundi,
                            HeureDebut = new TimeSpan(8, 30, 0),
                            HeureFin = new TimeSpan(11, 30, 0),
                            Salle = "B-201"
                        },
                        new EmploiDuTemps
                        {
                            IdCoursOffert = coursOffert.IdCoursOffert,
                            JourSemaine = JourSemaine.Mercredi,
                            HeureDebut = new TimeSpan(13, 0, 0),
                            HeureFin = new TimeSpan(16, 0, 0),
                            Salle = "B-201"
                        },
                        new EmploiDuTemps
                        {
                            IdCoursOffert = coursOffert.IdCoursOffert,
                            JourSemaine = JourSemaine.Vendredi,
                            HeureDebut = new TimeSpan(9, 0, 0),
                            HeureFin = new TimeSpan(12, 0, 0),
                            Salle = "A-105"
                        }
                    );
                    context.SaveChanges();
                }
            }
        }
    }
}
