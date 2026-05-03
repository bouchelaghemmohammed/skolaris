using Microsoft.AspNetCore.Identity;
using Skolaris.Enums;
using Skolaris.Models;

namespace Skolaris.Data
{
    public static class DataSeeder
    {
        public static void Seed(ApplicationDbContext context)
        {
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
            if (!context.Utilisateurs.Any(u => u.Email == "prof@gmail.com"))
            {
                var enseignant = new Utilisateur
                {
                    Nom = "Dion",
                    Prenom = "Céline",
                    Email = "prof@gmail.com",
                    Role = Role.ENSEIGNANT,
                    IsActive = true
                };
                enseignant.MotDePasse = hasher.HashPassword(enseignant, "123456");
                context.Utilisateurs.Add(enseignant);
                context.SaveChanges();
            }

            // Élève (correction: vérifie le bon email — bug dans le code original)
            if (!context.Utilisateurs.Any(u => u.Email == "eleve@gmail.com"))
            {
                var eleve = new Utilisateur
                {
                    Nom = "Tremblay",
                    Prenom = "Marie",
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
                    Adresse = "123 rue de l'Éducation, Mascouche, QC",
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

            // Niveau
            if (!context.Niveaux.Any())
            {
                var programme = context.Programmes.First();
                context.Niveaux.Add(new Niveau
                {
                    Nom = "Session 6",
                    IdProgramme = programme.IdProgramme
                });
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
            var profUser = context.Utilisateurs.FirstOrDefault(u => u.Email == "prof@gmail.com");
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
        }
    }
}