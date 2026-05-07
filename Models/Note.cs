using Skolaris.Enums;

namespace Skolaris.Models
{
    public class Note
    {
        public int IdNote { get; set; }
        public decimal Valeur { get; set; }
        public TypeNote Type { get; set; }
        public string? Description { get; set; }
        public decimal Ponderation { get; set; }
        public DateTime DateEvaluation { get; set; } = DateTime.UtcNow;
        public string? Commentaire { get; set; }
        public int IdEleve { get; set; }
        public int IdCoursOffert { get; set; }
        public int? IdCategorie { get; set; }

        public Eleve Eleve { get; set; } = null!;
        public CoursOffert CoursOffert { get; set; } = null!;
        public CategorieEvaluation? Categorie { get; set; }
        public ICollection<DetailBulletin> DetailBulletins { get; set; } = new List<DetailBulletin>();
    }
}
