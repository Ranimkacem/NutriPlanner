using System.ComponentModel.DataAnnotations;

namespace NutriPlanner.Models
{
    public class Recette
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(150, MinimumLength = 3, ErrorMessage = "Le titre doit faire entre 3 et 150 caractères.")]
        public string Titre { get; set; }

        [StringLength(1000)]
        public string Description { get; set; } = "";

        [Range(1, 100, ErrorMessage = "Le nombre de personnes doit être entre 1 et 100.")]
        public int NombrePersonnes { get; set; } = 4;

        [Required]
        public string Categorie { get; set; } = "Déjeuner";

        [Required]
        public string TypeCuisine { get; set; } = "Tunisienne";

        public DateTime DateCreation { get; set; } = DateTime.Now;

        //====== Entity Framework Core relationships ======
        public ICollection<RecetteIngredient> RecetteIngredients { get; set; } = new List<RecetteIngredient>();
    }
}