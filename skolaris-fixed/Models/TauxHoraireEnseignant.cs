namespace Skolaris.Models
{
    public class TauxHoraireEnseignant
    {
        public int IdTaux { get; set; }
        public decimal TauxHoraire { get; set; }
        public DateTime DateEffet { get; set; }
        public int IdEnseignant { get; set; }

        public Enseignant Enseignant { get; set; } = null!;
        public ICollection<PaiementEnseignant> Paiements { get; set; } = new List<PaiementEnseignant>();
    }
}
