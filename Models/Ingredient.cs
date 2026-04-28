using System.ComponentModel.DataAnnotations;

namespace NutriPlanner.Models
{
    public class Ingredient
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Le nom doit faire entre 2 et 100 caractères.")]
        public string Nom { get; set; }

        [Range(0, 10000, ErrorMessage = "Les calories doivent être entre 0 et 10 000 kcal/unité.")]
        public double CaloriesParUnite { get; set; }

        [StringLength(50)]
        public string Unite { get; set; } = "g";

        //====== Entity Framework Core relationships ======
        public ICollection<RecetteIngredient> RecetteIngredients { get; set; } = new List<RecetteIngredient>();
    }
}