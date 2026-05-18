using Skolaris.Enums;

namespace Skolaris.Models
{
    public class PaiementEleve
    {
        public int IdPaiementEleve { get; set; }
        public decimal Montant { get; set; }
        public DateTime DatePaiement { get; set; }
        public string? ModePaiement { get; set; }
        public StatutPaiement Statut { get; set; } = StatutPaiement.EnAttente;
        public string? Note { get; set; }
        public int IdEleve { get; set; }
        public int IdSession { get; set; }

        public Eleve Eleve { get; set; } = null!;
        public Session Session { get; set; } = null!;
    }
}
