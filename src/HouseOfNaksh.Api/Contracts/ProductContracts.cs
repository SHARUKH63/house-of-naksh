namespace HouseOfNaksh.Api.Contracts;

public record ProductResponse(
    int Id,
    string Sku,
    string Name,
    string? Description,
    decimal Price,
    int StockQuantity,
    string? Category,
    bool IsActive,
    DateTime CreatedAtUtc);

public record CreateProductRequest(
    string Sku,
    string Name,
    string? Description,
    decimal Price,
    int StockQuantity,
    string? Category);

public record UpdateProductRequest(
    string Name,
    string? Description,
    decimal Price,
    string? Category,
    bool IsActive);

public record PagedResponse<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalCount)
{
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasNext => Page < TotalPages;
}
