using NutriPlanner.Models;

namespace NutriPlanner.Services
{
    public interface IRecetteService
    {
        // === RECETTES ===
        Task<List<Recette>> GetRecettesAsync();
        Task<Recette?> GetRecetteByIdAsync(int id);
        Task AddRecetteAsync(Recette recette);
        Task UpdateRecetteAsync(Recette recette);
        Task DeleteRecetteAsync(int id);

        // === INGREDIENTS ===
        Task<List<Ingredient>> GetIngredientsAsync();
        Task<Ingredient?> GetIngredientByIdAsync(int id);
        Task AddIngredientAsync(Ingredient ingredient);
        Task UpdateIngredientAsync(Ingredient ingredient);
        Task DeleteIngredientAsync(int id);

        // === INDICATEURS ANALYTIQUES ===
        Task<int> GetTotalRecettesAsync();
        Task<double> GetCaloriesTotalesRecetteAsync(int recetteId);
        Task<double> GetCaloriesParPersonneAsync(int recetteId);
        Task<Dictionary<string, int>> GetRepartitionParCategorieAsync();
        Task<Dictionary<string, int>> GetRepartitionParTypeCuisineAsync();
    }
}