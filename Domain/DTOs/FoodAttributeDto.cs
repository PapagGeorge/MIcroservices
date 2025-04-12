namespace Infrastructure.DTOs;

public class FoodAttributeDto
{
    public string FoodName { get; set; } = string.Empty;
    public double Calories { get; set; }
    public double Carbohydrates { get; set; }
    public double Protein { get; set; }
    public double TotalFat { get; set; }
    public string ServingDescription { get; set; } = string.Empty;
}