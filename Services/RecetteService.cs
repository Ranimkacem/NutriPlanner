using Microsoft.EntityFrameworkCore;
using NutriPlanner.Data;
using NutriPlanner.Models;

namespace NutriPlanner.Services
{
    public class RecetteService : IRecetteService
    {
        private readonly AppDbContext _dbContext;

        public RecetteService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // =========================================================
        // RECETTES
        // =========================================================

        public async Task<List<Recette>> GetRecettesAsync()
        {
            return await _dbContext.Recettes
                .Include(r => r.RecetteIngredients)
                    .ThenInclude(ri => ri.Ingredient)
                .OrderBy(r => r.Titre)
                .ToListAsync();
        }

        public async Task<Recette?> GetRecetteByIdAsync(int id)
        {
            return await _dbContext.Recettes
                .Include(r => r.RecetteIngredients)
                    .ThenInclude(ri => ri.Ingredient)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task AddRecetteAsync(Recette recette)
        {
            recette.DateCreation = DateTime.Now;
            _dbContext.Recettes.Add(recette);
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateRecetteAsync(Recette recette)
        {
            _dbContext.Recettes.Update(recette);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteRecetteAsync(int id)
        {
            var recette = await _dbContext.Recettes.FindAsync(id);
            if (recette != null)
            {
                _dbContext.Recettes.Remove(recette);
                await _dbContext.SaveChangesAsync();
            }
        }

        // =========================================================
        // INGREDIENTS
        // =========================================================

        public async Task<List<Ingredient>> GetIngredientsAsync()
        {
            return await _dbContext.Ingredients
                .OrderBy(i => i.Nom)
                .ToListAsync();
        }

        public async Task<Ingredient?> GetIngredientByIdAsync(int id)
        {
            return await _dbContext.Ingredients.FindAsync(id);
        }

        public async Task AddIngredientAsync(Ingredient ingredient)
        {
            _dbContext.Ingredients.Add(ingredient);
            await _dbContext.SaveChangesAsync();
        }

        public async Task UpdateIngredientAsync(Ingredient ingredient)
        {
            _dbContext.Ingredients.Update(ingredient);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteIngredientAsync(int id)
        {
            var ingredient = await _dbContext.Ingredients.FindAsync(id);
            if (ingredient != null)
            {
                _dbContext.Ingredients.Remove(ingredient);
                await _dbContext.SaveChangesAsync();
            }
        }

        // =========================================================
        // INDICATEURS ANALYTIQUES
        // =========================================================

        public async Task<int> GetTotalRecettesAsync()
        {
            return await _dbContext.Recettes.CountAsync();
        }

        public async Task<double> GetCaloriesTotalesRecetteAsync(int recetteId)
        {
            var lignes = await _dbContext.RecetteIngredients
                .Include(ri => ri.Ingredient)
                .Where(ri => ri.RecetteId == recetteId)
                .ToListAsync();

            return lignes.Sum(ri => ri.Quantite * ri.Ingredient.CaloriesParUnite);
        }

        public async Task<double> GetCaloriesParPersonneAsync(int recetteId)
        {
            var recette = await _dbContext.Recettes.FindAsync(recetteId);
            if (recette == null || recette.NombrePersonnes == 0) return 0;

            double total = await GetCaloriesTotalesRecetteAsync(recetteId);
            return total / recette.NombrePersonnes;
        }

        public async Task<Dictionary<string, int>> GetRepartitionParCategorieAsync()
        {
            return await _dbContext.Recettes
                .GroupBy(r => r.Categorie)
                .ToDictionaryAsync(g => g.Key, g => g.Count());
        }

        public async Task<Dictionary<string, int>> GetRepartitionParTypeCuisineAsync()
        {
            return await _dbContext.Recettes
                .GroupBy(r => r.TypeCuisine)
                .ToDictionaryAsync(g => g.Key, g => g.Count());
        }
    }
}