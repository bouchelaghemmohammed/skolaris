using Microsoft.AspNetCore.Identity;
using Skolaris.Enums;
using Skolaris.Models;

namespace Skolaris.Data
{
    public static class DataSeeder
    {
        public static void Seed(ApplicationDbContext context)
        {
            // Créer l'admin par défaut si inexistant
            if (!context.Utilisateurs.Any(u => u.Email == "admin@gmail.com"))
            {
                var hasher = new PasswordHasher<Utilisateur>();
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
        }
    }
}
