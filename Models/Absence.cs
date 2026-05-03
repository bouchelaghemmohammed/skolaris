using Skolaris.Enums;
using System.ComponentModel.DataAnnotations;

namespace Skolaris.Models
{
    public class Absence
    {
        [Key]
        public int IdAbsence { get; set; }
        public TypeAbsence Type { get; set; }
        public StatutAbsence Statut { get; set; } = StatutAbsence.EnAttente;
        public DateTime DateAbsence { get; set; } = DateTime.UtcNow;
        public int IdEleve { get; set; }
        public int IdCoursOffert { get; set; }

        public Eleve? Eleve { get; set; }
        public CoursOffert? CoursOffert { get; set; }
        public JustificationAbsence? Justification { get; set; }
    }
}
