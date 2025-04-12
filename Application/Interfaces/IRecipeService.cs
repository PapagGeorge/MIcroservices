using Infrastructure.DTOs;

namespace Application.Interfaces;

public interface IRecipeService
{
    Task<RecipeDto?> GetRecipeByIdAsync(int id);
    Task<RecipeDto?> GetScaledRecipeAsync(int id, int servings);
    Task<IEnumerable<RecipeDto>> SearchRecipesAsync(string searchTerm);
    Task<int> CreateRecipeAsync(RecipeDto recipeDto);
    Task UpdateRecipeAsync(RecipeDto recipeDto);
    Task DeleteRecipeAsync(int id);
    Task<NutritionResponseDto> GetNutritionDataAsync(RecipeDto recipe);
}