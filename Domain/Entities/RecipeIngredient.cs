namespace Domain;

public class RecipeIngredient
{
    public int RecipeId { get; set; }
    public Recipe Recipe { get; set; } = null!;
        
    public int IngredientId { get; set; }
    public Ingredient Ingredient { get; set; } = null!;
        
    public decimal Amount { get; set; }
    public string Unit { get; set; } = string.Empty;
    public string? Notes { get; set; }
}