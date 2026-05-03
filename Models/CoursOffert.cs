using Skolaris.Enums;

namespace Skolaris.Models
{
    public class CoursOffert
    {
        public int IdCoursOffert { get; set; }
        public int IdCours { get; set; }
        public int IdGroupe { get; set; }
        public int IdSession { get; set; }
        public int? IdEnseignant { get; set; }
        public ModeEnseignement ModeEnseignement { get; set; } = ModeEnseignement.Présentiel;

        public Cours Cours { get; set; } = null!;
        public Groupe Groupe { get; set; } = null!;
        public Session Session { get; set; } = null!;
        public Enseignant? Enseignant { get; set; }
        public ICollection<Inscription> Inscriptions { get; set; } = new List<Inscription>();
        public ICollection<Note> Notes { get; set; } = new List<Note>();
        public ICollection<Absence> Absences { get; set; } = new List<Absence>();
        public ICollection<EmploiDuTemps> EmploisDuTemps { get; set; } = new List<EmploiDuTemps>();
        public ICollection<DetailBulletin> DetailBulletins { get; set; } = new List<DetailBulletin>();
    }
}
