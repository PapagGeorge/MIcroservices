using Domain;
using Infrastructure.DTOs;

namespace Application.Interfaces;

public interface IRecipeRepository
{
    Task<Recipe?> GetByIdAsync(int id);
    Task<IEnumerable<Recipe>> GetAllAsync();
    Task<IEnumerable<Recipe>> SearchAsync(string searchTerm);
    Task<IEnumerable<Recipe>> GetByIngredientsAsync(IEnumerable<string> ingredients);
    Task<int> CreateAsync(Recipe recipe);
    Task UpdateAsync(Recipe recipe);
    Task DeleteAsync(int id);
}