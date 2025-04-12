using Microsoft.AspNetCore.Mvc;
using Application.Interfaces;
using Infrastructure.DTOs;

namespace RecipeProject.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RecipesController : ControllerBase
{
    private readonly IRecipeService _recipeService;
        
        public RecipesController(IRecipeService recipeService)
        {
            _recipeService = recipeService;
        }
        
        [HttpGet("{id}")]
        public async Task<ActionResult<RecipeDto>> GetRecipe(int id)
        {
            var recipe = await _recipeService.GetRecipeByIdAsync(id);
            
            if (recipe == null)
            {
                return NotFound();
            }
            
            return Ok(recipe);
        }
        
        [HttpGet("{id}/scale")]
        public async Task<ActionResult<RecipeDto>> GetScaledRecipe(int id, [FromQuery] int servings)
        {
            if (servings <= 0)
            {
                return BadRequest("Servings must be greater than zero");
            }
            
            var recipe = await _recipeService.GetScaledRecipeAsync(id, servings);
            
            if (recipe == null)
            {
                return NotFound();
            }
            
            return Ok(recipe);
        }
        
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<RecipeDto>>> SearchRecipes([FromQuery] string term)
        {
            var recipes = await _recipeService.SearchRecipesAsync(term);
            return Ok(recipes);
        }
        
        [HttpPost("nutrition-data")]
        public async Task<ActionResult<NutritionResponseDto>> GetRecipeNutritionData([FromBody] RecipeDto recipeDto)
        {
            if (recipeDto == null || recipeDto.Ingredients == null || recipeDto.Ingredients.Count == 0)
            {
                return BadRequest("Recipe must contain ingredients to calculate nutrition.");
            }

            try
            {
                // Send the recipe to the nutrition service and get the nutrition data back
                var nutritionResponse = await _recipeService.GetNutritionDataAsync(recipeDto);

                // Return the response as OK
                return Ok(nutritionResponse);
            }
            catch (Exception ex)
            {
                // Log the exception and return an internal server error
                // LogError(ex); (This should be logged using a logger)
                return StatusCode(500, "An error occurred while processing the request.");
            }
        }
        
        [HttpPost]
        public async Task<ActionResult<int>> CreateRecipe([FromBody] RecipeDto recipeDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            var id = await _recipeService.CreateRecipeAsync(recipeDto);
            return CreatedAtAction(nameof(GetRecipe), new { id }, id);
        }
        
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRecipe(int id, [FromBody] RecipeDto recipeDto)
        {
            if (id != recipeDto.Id)
            {
                return BadRequest();
            }
            
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            await _recipeService.UpdateRecipeAsync(recipeDto);
            return NoContent();
        }
        
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRecipe(int id)
        {
            await _recipeService.DeleteRecipeAsync(id);
            return NoContent();
        }
}