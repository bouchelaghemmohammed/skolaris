namespace Skolaris.Models
{
    public class Enseignant
    {
        public int IdEnseignant { get; set; }
        public int IdUtilisateur { get; set; }

        public Utilisateur Utilisateur { get; set; } = null!;
        public ICollection<CoursOffert> CoursOfferts { get; set; } = new List<CoursOffert>();
        public ICollection<TauxHoraireEnseignant> TauxHoraires { get; set; } = new List<TauxHoraireEnseignant>();
        public ICollection<PaiementEnseignant> Paiements { get; set; } = new List<PaiementEnseignant>();
    }
}
