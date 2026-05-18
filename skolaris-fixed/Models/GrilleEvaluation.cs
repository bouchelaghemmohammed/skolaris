namespace Skolaris.Models
{
    public class GrilleEvaluation
    {
        public int IdGrille { get; set; }
        public int IdCoursOffert { get; set; }
        public string? Description { get; set; }
        public DateTime DateCreation { get; set; } = DateTime.UtcNow;

        public CoursOffert CoursOffert { get; set; } = null!;
        public ICollection<CategorieEvaluation> Categories { get; set; } = new List<CategorieEvaluation>();
    }
}
