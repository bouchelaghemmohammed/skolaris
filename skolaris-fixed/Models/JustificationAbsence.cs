using Skolaris.Enums;

namespace Skolaris.Models
{
    public class JustificationAbsence
    {
        public int IdJustification { get; set; }
        public string? Description { get; set; }
        public StatutJustification Statut { get; set; } = StatutJustification.Justifiee;
        public int IdAbsence { get; set; }

        public Absence Absence { get; set; } = null!;
    }
}
