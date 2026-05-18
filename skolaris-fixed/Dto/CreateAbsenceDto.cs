using Skolaris.Enums;

namespace Skolaris.Dto
{
    public class CreateAbsenceDto
    {
        public TypeAbsence Type { get; set; }
        public StatutAbsence Statut { get; set; }
        public DateTime DateAbsence { get; set; }
        public int IdEleve { get; set; }
        public int IdCoursOffert { get; set; }
    }
}
