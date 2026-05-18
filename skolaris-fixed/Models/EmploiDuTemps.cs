using Skolaris.Enums;

namespace Skolaris.Models
{
    public class EmploiDuTemps
    {
        public int IdEmploi { get; set; }
        public JourSemaine JourSemaine { get; set; }
        public TimeSpan HeureDebut { get; set; }
        public TimeSpan HeureFin { get; set; }
        public string? Salle { get; set; }
        public int IdCoursOffert { get; set; }

        public CoursOffert CoursOffert { get; set; } = null!;
    }
}
