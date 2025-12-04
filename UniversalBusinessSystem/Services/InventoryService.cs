using Microsoft.EntityFrameworkCore;
using UniversalBusinessSystem.Core.Entities;
using UniversalBusinessSystem.Core.Services;
using UniversalBusinessSystem.Data;

namespace UniversalBusinessSystem.Services;

public interface IInventoryService
{
    Task<IReadOnlyList<Product>> GetProductsAsync(string? search = null, Guid? categoryId = null, Guid? unitId = null);
    Task<IReadOnlyList<Category>> GetCategoriesAsync();
    Task<IReadOnlyList<Unit>> GetUnitsAsync();
    Task<Product> AddProductAsync(Product product);
    Task<Category> AddCategoryAsync(Category category);
    Task DeleteProductAsync(Guid productId);
    Task DeleteCategoryAsync(Guid categoryId);
}

public class InventoryService : IInventoryService
{
    private readonly UniversalBusinessSystemDbContext _context;
    private readonly IAuthenticationService _authService;

    public InventoryService(UniversalBusinessSystemDbContext context, IAuthenticationService authService)
    {
        _context = context;
        _authService = authService;
    }

    public async Task<IReadOnlyList<Product>> GetProductsAsync(string? search = null, Guid? categoryId = null, Guid? unitId = null)
    {
        var organizationId = await GetOrganizationIdAsync().ConfigureAwait(false);

        var query = _context.Products
            .Include(p => p.Category)
            .Include(p => p.Unit)
            .Where(p => p.OrganizationId == organizationId);

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(p =>
                p.Name.Contains(search) ||
                (p.Sku != null && p.Sku.Contains(search)) ||
                (p.Barcode != null && p.Barcode.Contains(search)));
        }

        if (categoryId.HasValue && categoryId != Guid.Empty)
        {
            query = query.Where(p => p.CategoryId == categoryId.Value);
        }

        if (unitId.HasValue && unitId != Guid.Empty)
        {
            query = query.Where(p => p.UnitId == unitId.Value);
        }

        return await query
            .OrderBy(p => p.Name)
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<Category>> GetCategoriesAsync()
    {
        var organizationId = await GetOrganizationIdAsync().ConfigureAwait(false);

        return await _context.Categories
            .Where(c => c.OrganizationId == organizationId)
            .OrderBy(c => c.Name)
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<Unit>> GetUnitsAsync()
    {
        var organizationId = await GetOrganizationIdAsync().ConfigureAwait(false);

        return await _context.OrganizationUnits
            .Where(ou => ou.OrganizationId == organizationId)
            .Select(ou => ou.Unit)
            .OrderBy(u => u.Name)
            .ToListAsync()
            .ConfigureAwait(false);
    }

    public async Task<Product> AddProductAsync(Product product)
    {
        var organizationId = await GetOrganizationIdAsync().ConfigureAwait(false);
        product.OrganizationId = organizationId;
        product.Id = Guid.NewGuid();
        product.CreatedAt = DateTime.UtcNow;
        product.UpdatedAt = null;

        await _context.Products.AddAsync(product).ConfigureAwait(false);
        await _context.SaveChangesAsync().ConfigureAwait(false);

        return product;
    }

    public async Task<Category> AddCategoryAsync(Category category)
    {
        var organizationId = await GetOrganizationIdAsync().ConfigureAwait(false);
        category.OrganizationId = organizationId;
        category.Id = Guid.NewGuid();
        category.CreatedAt = DateTime.UtcNow;
        category.UpdatedAt = null;

        await _context.Categories.AddAsync(category).ConfigureAwait(false);
        await _context.SaveChangesAsync().ConfigureAwait(false);

        return category;
    }

    public async Task DeleteProductAsync(Guid productId)
    {
        var organizationId = await GetOrganizationIdAsync().ConfigureAwait(false);

        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == productId && p.OrganizationId == organizationId)
            .ConfigureAwait(false);

        if (product == null)
        {
            throw new InvalidOperationException("Product not found or not accessible.");
        }

        _context.Products.Remove(product);
        await _context.SaveChangesAsync().ConfigureAwait(false);
    }

    public async Task DeleteCategoryAsync(Guid categoryId)
    {
        var organizationId = await GetOrganizationIdAsync().ConfigureAwait(false);

        var category = await _context.Categories
            .Include(c => c.Products)
            .FirstOrDefaultAsync(c => c.Id == categoryId && c.OrganizationId == organizationId)
            .ConfigureAwait(false);

        if (category == null)
        {
            throw new InvalidOperationException("Category not found or not accessible.");
        }

        if (category.Products.Any())
        {
            throw new InvalidOperationException("Cannot delete a category that still has products.");
        }

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync().ConfigureAwait(false);
    }

    private async Task<Guid> GetOrganizationIdAsync()
    {
        var user = await _authService.GetCurrentUserAsync().ConfigureAwait(false);
        if (user?.Organization == null)
        {
            throw new InvalidOperationException("No organization context available. Please sign in again.");
        }

        return user.Organization.Id;
    }
}
