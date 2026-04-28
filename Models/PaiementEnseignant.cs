using Skolaris.Enums;

namespace Skolaris.Models
{
    public class PaiementEnseignant
    {
        public int IdPaiementEnseignant { get; set; }
        public decimal HeuresTravaillees { get; set; }
        public decimal MontantTotal { get; set; }
        public DateTime DatePaiement { get; set; }
        public StatutPaiement Statut { get; set; } = StatutPaiement.EnAttente;
        public string? Note { get; set; }
        public int IdEnseignant { get; set; }
        public int IdSession { get; set; }
        public int IdTaux { get; set; }

        public Enseignant Enseignant { get; set; } = null!;
        public Session Session { get; set; } = null!;
        public TauxHoraireEnseignant TauxHoraire { get; set; } = null!;
    }
}
