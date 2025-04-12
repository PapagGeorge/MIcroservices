using Application.Interfaces;
using Infrastructure.Repositories;

namespace Infrastructure;

public class UnitOfWork : IUnitOfWork
{
    private readonly RecipeDbContext _context;
    private IRecipeRepository? _recipeRepository;
        
    public UnitOfWork(RecipeDbContext context)
    {
        _context = context;
    }
        
    public IRecipeRepository Recipes => 
        _recipeRepository ??= new RecipeRepository(_context);
            
    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
        
    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}