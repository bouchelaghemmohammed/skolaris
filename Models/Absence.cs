using Skolaris.Enums;

namespace Skolaris.Models
{
    public class Absence
    {
        public int IdAbsence { get; set; }
        public TypeAbsence Type { get; set; }
        public StatutAbsence Statut { get; set; } = StatutAbsence.EnAttente;
        public DateTime DateAbsence { get; set; } = DateTime.UtcNow;
        public int IdEleve { get; set; }
        public int IdCoursOffert { get; set; }

        public Eleve Eleve { get; set; } = null!;
        public CoursOffert CoursOffert { get; set; } = null!;
        public JustificationAbsence? Justification { get; set; }
    }
}
