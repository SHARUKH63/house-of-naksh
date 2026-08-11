using HouseOfNaksh.Domain;

namespace HouseOfNaksh.Api.Contracts;

public static class ProductMappings
{
    public static ProductResponse ToResponse(this Product p) =>
        new(p.Id, p.Sku, p.Name, p.Description, p.Price, p.StockQuantity, p.Category, p.IsActive, p.CreatedAtUtc);

    public static Product ToEntity(this CreateProductRequest r) =>
        new()
        {
            Sku = r.Sku,
            Name = r.Name,
            Description = r.Description,
            Price = r.Price,
            StockQuantity = r.StockQuantity,
            Category = r.Category,
            IsActive = true
        };
}
