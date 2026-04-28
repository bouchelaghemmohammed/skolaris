namespace Skolaris.Models
{
    public class Eleve
    {
        public int IdEleve { get; set; }
        public string Matricule { get; set; } = string.Empty;
        public int IdUtilisateur { get; set; }
        public int IdProgramme { get; set; }
        public int IdGroupe { get; set; }
        public int IdNiveau { get; set; }

        public Utilisateur Utilisateur { get; set; } = null!;
        public Programme Programme { get; set; } = null!;
        public Groupe Groupe { get; set; } = null!;
        public Niveau Niveau { get; set; } = null!;
        public ICollection<Inscription> Inscriptions { get; set; } = new List<Inscription>();
        public ICollection<Note> Notes { get; set; } = new List<Note>();
        public ICollection<Bulletin> Bulletins { get; set; } = new List<Bulletin>();
        public ICollection<Absence> Absences { get; set; } = new List<Absence>();
        public ICollection<PaiementEleve> Paiements { get; set; } = new List<PaiementEleve>();
    }
}
