using Domain;
using Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class RecipeRepository : IRecipeRepository
    {
        private readonly RecipeDbContext _context;
        
        public RecipeRepository(RecipeDbContext context)
        {
            _context = context;
        }
        
        public async Task<Recipe?> GetByIdAsync(int id)
        {
            return await _context.Recipes
                .Include(r => r.Ingredients)
                    .ThenInclude(i => i.Ingredient)
                .Include(r => r.Steps)
                .Include(r => r.Categories)
                    .ThenInclude(c => c.Category)
                .Include(r => r.Tags)
                    .ThenInclude(t => t.Tag)
                .FirstOrDefaultAsync(r => r.Id == id);
        }
        
        public async Task<IEnumerable<Recipe>> GetAllAsync()
        {
            return await _context.Recipes
                .Include(r => r.Ingredients)
                    .ThenInclude(i => i.Ingredient)
                .Include(r => r.Steps)
                .Include(r => r.Categories)
                    .ThenInclude(c => c.Category)
                .Include(r => r.Tags)
                    .ThenInclude(t => t.Tag)
                .ToListAsync();
        }
        
        public async Task<IEnumerable<Recipe>> SearchAsync(string searchTerm)
        {
            return await _context.Recipes
                .Include(r => r.Ingredients)
                    .ThenInclude(i => i.Ingredient)
                .Include(r => r.Steps)
                .Include(r => r.Categories)
                    .ThenInclude(c => c.Category)
                .Include(r => r.Tags)
                    .ThenInclude(t => t.Tag)
                .Where(r => r.Title.Contains(searchTerm) || 
                           r.Description.Contains(searchTerm) ||
                           r.Ingredients.Any(i => i.Ingredient.Name.Contains(searchTerm)))
                .ToListAsync();
        }
        
        public async Task<IEnumerable<Recipe>> GetByIngredientsAsync(IEnumerable<string> ingredients)
        {
            var ingredientsList = ingredients.ToList();
            
            return await _context.Recipes
                .Include(r => r.Ingredients)
                    .ThenInclude(i => i.Ingredient)
                .Include(r => r.Steps)
                .Include(r => r.Categories)
                    .ThenInclude(c => c.Category)
                .Include(r => r.Tags)
                    .ThenInclude(t => t.Tag)
                .Where(r => r.Ingredients.Any(i => 
                    ingredientsList.Contains(i.Ingredient.Name)))
                .ToListAsync();
        }
        
        public async Task<int> CreateAsync(Recipe recipe)
        {
            _context.Recipes.Add(recipe);
            await _context.SaveChangesAsync();
            return recipe.Id;
        }
        
        public async Task UpdateAsync(Recipe recipe)
        {
            _context.Recipes.Update(recipe);
            await _context.SaveChangesAsync();
        }
        
        public async Task DeleteAsync(int id)
        {
            var recipe = await _context.Recipes.FindAsync(id);
            if (recipe != null)
            {
                _context.Recipes.Remove(recipe);
                await _context.SaveChangesAsync();
            }
        }
    }