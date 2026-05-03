using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Skolaris.Models
{
    public class Inscription
    {
        [Key]
        public int IdInscription { get; set; }

        [ForeignKey("Eleve")]
        public int IdEleve { get; set; }

        [ForeignKey("CoursOffert")]
        public int IdCoursOffert { get; set; }

        public DateTime DateInscription { get; set; } = DateTime.UtcNow;

        [ValidateNever]
        public Eleve Eleve { get; set; } = null!;

        [ValidateNever]
        public CoursOffert CoursOffert { get; set; } = null!;
    }
}