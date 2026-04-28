namespace Skolaris.Models
{
    public class Inscription
    {
        public int IdInscription { get; set; }
        public int IdEleve { get; set; }
        public int IdCoursOffert { get; set; }
        public DateTime DateInscription { get; set; } = DateTime.UtcNow;

        public Eleve Eleve { get; set; } = null!;
        public CoursOffert CoursOffert { get; set; } = null!;
    }
}
