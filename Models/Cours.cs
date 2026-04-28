using Skolaris.Enums;

namespace Skolaris.Models
{
    public class Cours
    {
        public int IdCours { get; set; }
        public string Nom { get; set; } = string.Empty;
        public string? Code { get; set; }
        public string? Description { get; set; }
        public int IdProgramme { get; set; }
        public int IdNiveau { get; set; }

        public Programme Programme { get; set; } = null!;
        public Niveau Niveau { get; set; } = null!;
        public ICollection<CoursOffert> CoursOfferts { get; set; } = new List<CoursOffert>();
    }
}
