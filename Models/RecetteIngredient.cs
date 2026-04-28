using System.ComponentModel.DataAnnotations;

namespace NutriPlanner.Models
{
    public class RecetteIngredient
    {
        [Key]
        public int Id { get; set; }

        [Range(0.01, 100000, ErrorMessage = "La quantité doit être positive.")]
        public double Quantite { get; set; }

        //====== Entity Framework Core relationships ======
        public int RecetteId { get; set; }
        public Recette Recette { get; set; }

        public int IngredientId { get; set; }
        public Ingredient Ingredient { get; set; }
    }
}