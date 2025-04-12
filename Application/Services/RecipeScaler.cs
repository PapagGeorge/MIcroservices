using Domain;

namespace Application;

public class RecipeScalerService
    {
        /// <summary>
        /// Scales ingredient amounts based on desired servings
        /// </summary>
        /// <param name="originalAmount">The original amount of the ingredient</param>
        /// <param name="originalServings">The original recipe servings</param>
        /// <param name="targetServings">The desired servings</param>
        /// <returns>The scaled amount</returns>
        public decimal ScaleAmount(decimal originalAmount, int originalServings, int targetServings)
        {
            if (originalServings <= 0 || targetServings <= 0)
            {
                throw new ArgumentException("Servings must be greater than zero");
            }
            
            decimal scaleFactor = (decimal)targetServings / originalServings;
            return Math.Round(originalAmount * scaleFactor, 2);
        }
        
        /// <summary>
        /// Creates a scaled copy of a recipe with adjusted ingredient amounts
        /// </summary>
        /// <param name="recipe">The original recipe</param>
        /// <param name="targetServings">The desired servings</param>
        /// <returns>A new recipe instance with scaled ingredient amounts</returns>
        public Recipe ScaleRecipe(Recipe recipe, int targetServings)
        {
            if (recipe == null)
            {
                throw new ArgumentNullException(nameof(recipe));
            }
            
            if (targetServings <= 0)
            {
                throw new ArgumentException("Target servings must be greater than zero", nameof(targetServings));
            }
            
            // Create a deep copy of the recipe
            var scaledRecipe = new Recipe
            {
                Id = recipe.Id,
                Title = recipe.Title,
                Author = recipe.Author,
                Description = recipe.Description,
                PrepTimeMinutes = recipe.PrepTimeMinutes,
                CookTimeMinutes = recipe.CookTimeMinutes,
                Servings = targetServings, // Set the new servings count
                ImageUrl = recipe.ImageUrl,
                CreatedAt = recipe.CreatedAt,
                UpdatedAt = recipe.UpdatedAt,
                Steps = recipe.Steps.ToList(),
                Categories = recipe.Categories.ToList(),
                Tags = recipe.Tags.ToList()
            };
            
            // Scale the ingredients
            scaledRecipe.Ingredients = recipe.Ingredients.Select(i => new RecipeIngredient
            {
                RecipeId = i.RecipeId,
                IngredientId = i.IngredientId,
                Ingredient = i.Ingredient,
                Unit = i.Unit,
                Notes = i.Notes,
                Amount = ScaleAmount(i.Amount, recipe.Servings, targetServings)
            }).ToList();
            
            return scaledRecipe;
        }
    }