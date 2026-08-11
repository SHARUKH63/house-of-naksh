using HouseOfNaksh.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HouseOfNaksh.Infrastructure.Services;

public interface IProductService
{
    Task<(IReadOnlyList<Product> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize, string? category, bool? isActive, CancellationToken ct);
    Task<Product> GetByIdAsync(int id, CancellationToken ct);
    Task<Product> CreateAsync(Product product, CancellationToken ct);
    Task<Product> UpdateAsync(int id, Action<Product> apply, CancellationToken ct);
    Task DeactivateAsync(int id, CancellationToken ct);
}

public class ProductService(
    HouseOfNakshDbContext db,
    ILogger<ProductService> logger) : IProductService
{
    public async Task<(IReadOnlyList<Product>, int)> GetPagedAsync(
        int page, int pageSize, string? category, bool? isActive, CancellationToken ct)
    {
        var query = db.Products.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(p => p.Category == category);

        if (isActive.HasValue)
            query = query.Where(p => p.IsActive == isActive.Value);

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderBy(p => p.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, total);
    }

    public async Task<Product> GetByIdAsync(int id, CancellationToken ct)
        => await db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, ct)
           ?? throw new NotFoundException(nameof(Product), id);

    public async Task<Product> CreateAsync(Product product, CancellationToken ct)
    {
        var exists = await db.Products.AnyAsync(p => p.Sku == product.Sku, ct);
        if (exists)
            throw new ConflictException($"A product with SKU '{product.Sku}' already exists");

        db.Products.Add(product);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Created product {ProductId} with SKU {Sku}", product.Id, product.Sku);
        return product;
    }

    public async Task<Product> UpdateAsync(int id, Action<Product> apply, CancellationToken ct)
    {
        var product = await db.Products.FirstOrDefaultAsync(p => p.Id == id, ct)
                      ?? throw new NotFoundException(nameof(Product), id);

        apply(product);
        product.UpdatedAtUtc = DateTime.UtcNow;

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConflictException(
                "The product was modified by another request. Reload and try again.");
        }

        logger.LogInformation("Updated product {ProductId}", id);
        return product;
    }

    public async Task DeactivateAsync(int id, CancellationToken ct)
    {
        var product = await db.Products.FirstOrDefaultAsync(p => p.Id == id, ct)
                      ?? throw new NotFoundException(nameof(Product), id);

        product.IsActive = false;
        product.UpdatedAtUtc = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Deactivated product {ProductId}", id);
    }
}
