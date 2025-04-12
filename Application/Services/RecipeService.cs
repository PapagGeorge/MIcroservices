using Application.Interfaces;
using Infrastructure.DTOs;
using Domain;


namespace Application;

public class RecipeService : IRecipeService
    {
        private readonly Interfaces.IUnitOfWork _unitOfWork;
        private readonly RecipeScalerService _recipeScaler;
        private readonly IRabbitMqRpcClient<string, NutritionResponseDto> _rpcClient;
        private const string NutritionQueue = "nutrition_query_queue";
        
        public RecipeService(
            Interfaces.IUnitOfWork unitOfWork,
            RecipeScalerService recipeScaler,
            IRabbitMqRpcClient<string, NutritionResponseDto> rpcClient)
        {
            _unitOfWork = unitOfWork;
            _recipeScaler = recipeScaler;
            _rpcClient = rpcClient;
        }
        
        public async Task<RecipeDto?> GetRecipeByIdAsync(int id)
        {
            var recipe = await _unitOfWork.Recipes.GetByIdAsync(id);
            return recipe != null ? MapToDto(recipe) : null;
        }
        
        public async Task<RecipeDto?> GetScaledRecipeAsync(int id, int servings)
        {
            var recipe = await _unitOfWork.Recipes.GetByIdAsync(id);
            if (recipe == null) return null;
            
            // Use the domain service to scale the recipe
            var scaledRecipe = _recipeScaler.ScaleRecipe(recipe, servings);
            return MapToDto(scaledRecipe);
        }
        
        public async Task<IEnumerable<RecipeDto>> SearchRecipesAsync(string searchTerm)
        {
            var recipes = await _unitOfWork.Recipes.SearchAsync(searchTerm);
            return recipes.Select(MapToDto);
        }
        
        public async Task<int> CreateRecipeAsync(RecipeDto recipeDto)
        {
            var recipe = MapToEntity(recipeDto);
            recipe.CreatedAt = DateTime.UtcNow;
            recipe.UpdatedAt = DateTime.UtcNow;
            
            var id = await _unitOfWork.Recipes.CreateAsync(recipe);
            await _unitOfWork.SaveChangesAsync();
            return id;
        }
        
        public async Task<NutritionResponseDto> GetNutritionDataAsync(RecipeDto recipe)
        {
            var ingredientsQuery = string.Join(", ", recipe.Ingredients.Select(i =>
                $"{i.Name} {i.Unit} {i.Amount}"));

            // Send the request and await the response
            var result = await _rpcClient.SendRequestAsync("nutrition_queue", ingredientsQuery);


            return result;
        }
        
        public async Task UpdateRecipeAsync(RecipeDto recipeDto)
        {
            var recipe = MapToEntity(recipeDto);
            recipe.UpdatedAt = DateTime.UtcNow;
            
            await _unitOfWork.Recipes.UpdateAsync(recipe);
            await _unitOfWork.SaveChangesAsync();
        }
        
        public async Task DeleteRecipeAsync(int id)
        {
            await _unitOfWork.Recipes.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
        }
        
        // Helper methods for mapping between entities and DTOs
        private RecipeDto MapToDto(Recipe recipe)
        {
            return new RecipeDto
            {
                Id = recipe.Id,
                Title = recipe.Title,
                Author = recipe.Author,
                Description = recipe.Description,
                PrepTimeMinutes = recipe.PrepTimeMinutes,
                CookTimeMinutes = recipe.CookTimeMinutes,
                Servings = recipe.Servings,
                ImageUrl = recipe.ImageUrl,
                CreatedAt = recipe.CreatedAt,
                UpdatedAt = recipe.UpdatedAt,
                
                Ingredients = recipe.Ingredients.Select(i => new IngredientDto
                {
                    Id = i.Ingredient.Id,
                    Name = i.Ingredient.Name,
                    Amount = i.Amount,
                    Unit = i.Unit,
                    Notes = i.Notes
                }).ToList(),
                
                Steps = recipe.Steps
                    .OrderBy(s => s.OrderNumber)
                    .Select(s => new StepDto
                    {
                        OrderNumber = s.OrderNumber,
                        Instruction = s.Instruction
                    }).ToList(),
                
                Categories = recipe.Categories.Select(c => c.Category.Name).ToList(),
                Tags = recipe.Tags.Select(t => t.Tag.Name).ToList()
            };
        }
        
        private Recipe MapToEntity(RecipeDto dto)
        {
            // This is a simplified version - in a real app you would handle
            // ingredients, steps, categories and tags more carefully
            return new Recipe
            {
                Id = dto.Id,
                Title = dto.Title,
                Author = dto.Author,
                Description = dto.Description,
                PrepTimeMinutes = dto.PrepTimeMinutes,
                CookTimeMinutes = dto.CookTimeMinutes,
                Servings = dto.Servings,
                ImageUrl = dto.ImageUrl,
                CreatedAt = dto.CreatedAt,
                UpdatedAt = dto.UpdatedAt
                // Relationships would be handled here
            };
        }
    }