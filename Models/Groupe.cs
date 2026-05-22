namespace Skolaris.Models
{
    public class Groupe
    {
        public int IdGroupe { get; set; }
        public string Nom { get; set; } = string.Empty;
        public int IdProgramme { get; set; }

        public Programme Programme { get; set; } = null!;
        public ICollection<CoursOffert> CoursOfferts { get; set; } = new List<CoursOffert>();
        public ICollection<Eleve> Eleves { get; set; } = new List<Eleve>();
    }
}
