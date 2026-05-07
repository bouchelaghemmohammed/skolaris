using Microsoft.EntityFrameworkCore;
using Skolaris.Data;
using Skolaris.Models;

namespace Skolaris.Services
{
    public class GrilleEvaluationService
    {
        private readonly ApplicationDbContext _context;

        public GrilleEvaluationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public GrilleEvaluation? GetByCoursOffert(int idCoursOffert)
        {
            return _context.GrillesEvaluation
                .Include(g => g.Categories)
                .FirstOrDefault(g => g.IdCoursOffert == idCoursOffert);
        }

        public GrilleEvaluation? GetById(int id)
        {
            return _context.GrillesEvaluation
                .Include(g => g.Categories)
                .FirstOrDefault(g => g.IdGrille == id);
        }

        // NOT-01 : crée la grille pour un cours offert (une seule par cours offert).
        public GrilleResult CreateGrille(int idCoursOffert, string? description)
        {
            var coursOffert = _context.CoursOfferts.FirstOrDefault(co => co.IdCoursOffert == idCoursOffert);
            if (coursOffert == null)
                return GrilleResult.Fail("Cours offert introuvable.");

            if (_context.GrillesEvaluation.Any(g => g.IdCoursOffert == idCoursOffert))
                return GrilleResult.Fail("Une grille existe déjà pour ce cours offert.");

            var grille = new GrilleEvaluation
            {
                IdCoursOffert = idCoursOffert,
                Description = description,
                DateCreation = DateTime.UtcNow
            };
            _context.GrillesEvaluation.Add(grille);
            _context.SaveChanges();
            return GrilleResult.Ok(grille);
        }

        public bool DeleteGrille(int idGrille)
        {
            var grille = _context.GrillesEvaluation
                .Include(g => g.Categories)
                .FirstOrDefault(g => g.IdGrille == idGrille);
            if (grille == null) return false;

            // Détacher les notes liées aux catégories de cette grille
            var categorieIds = grille.Categories.Select(c => c.IdCategorie).ToList();
            var notesLiees = _context.Notes.Where(n => n.IdCategorie != null && categorieIds.Contains(n.IdCategorie.Value)).ToList();
            foreach (var n in notesLiees) n.IdCategorie = null;

            _context.CategoriesEvaluation.RemoveRange(grille.Categories);
            _context.GrillesEvaluation.Remove(grille);
            _context.SaveChanges();
            return true;
        }

        public CategorieResult AddCategorie(int idGrille, string nom, decimal ponderation)
        {
            var grille = _context.GrillesEvaluation.FirstOrDefault(g => g.IdGrille == idGrille);
            if (grille == null)
                return CategorieResult.Fail("Grille introuvable.");

            if (string.IsNullOrWhiteSpace(nom))
                return CategorieResult.Fail("Nom de catégorie obligatoire.");

            if (ponderation < 0 || ponderation > 100)
                return CategorieResult.Fail("La pondération doit être entre 0 et 100.");

            var categorie = new CategorieEvaluation
            {
                IdGrille = idGrille,
                Nom = nom.Trim(),
                Ponderation = ponderation
            };
            _context.CategoriesEvaluation.Add(categorie);
            _context.SaveChanges();
            return CategorieResult.Ok(categorie);
        }

        public CategorieResult UpdateCategorie(int idCategorie, string nom, decimal ponderation)
        {
            var categorie = _context.CategoriesEvaluation.FirstOrDefault(c => c.IdCategorie == idCategorie);
            if (categorie == null)
                return CategorieResult.Fail("Catégorie introuvable.");

            if (string.IsNullOrWhiteSpace(nom))
                return CategorieResult.Fail("Nom de catégorie obligatoire.");

            if (ponderation < 0 || ponderation > 100)
                return CategorieResult.Fail("La pondération doit être entre 0 et 100.");

            categorie.Nom = nom.Trim();
            categorie.Ponderation = ponderation;
            _context.SaveChanges();
            return CategorieResult.Ok(categorie);
        }

        public bool DeleteCategorie(int idCategorie)
        {
            var categorie = _context.CategoriesEvaluation.FirstOrDefault(c => c.IdCategorie == idCategorie);
            if (categorie == null) return false;

            // Détacher les notes liées à cette catégorie
            var notesLiees = _context.Notes.Where(n => n.IdCategorie == idCategorie).ToList();
            foreach (var n in notesLiees) n.IdCategorie = null;

            _context.CategoriesEvaluation.Remove(categorie);
            _context.SaveChanges();
            return true;
        }
    }

    public class GrilleResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public GrilleEvaluation? Grille { get; set; }
        public static GrilleResult Ok(GrilleEvaluation g) => new() { Success = true, Grille = g };
        public static GrilleResult Fail(string error) => new() { Success = false, ErrorMessage = error };
    }

    public class CategorieResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public CategorieEvaluation? Categorie { get; set; }
        public static CategorieResult Ok(CategorieEvaluation c) => new() { Success = true, Categorie = c };
        public static CategorieResult Fail(string error) => new() { Success = false, ErrorMessage = error };
    }
}
