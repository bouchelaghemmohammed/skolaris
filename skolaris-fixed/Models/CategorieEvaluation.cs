namespace Skolaris.Models
{
    public class CategorieEvaluation
    {
        public int IdCategorie { get; set; }
        public int IdGrille { get; set; }
        public string Nom { get; set; } = string.Empty;
        public decimal Ponderation { get; set; }

        public GrilleEvaluation Grille { get; set; } = null!;
        public ICollection<Note> Notes { get; set; } = new List<Note>();
    }
}
