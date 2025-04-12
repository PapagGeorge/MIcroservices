namespace Application.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IRecipeRepository Recipes { get; }
    Task<int> SaveChangesAsync();
}