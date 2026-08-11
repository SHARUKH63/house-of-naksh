using HouseOfNaksh.Api.Contracts;
using HouseOfNaksh.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace HouseOfNaksh.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ProductsController(IProductService products) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<PagedResponse<ProductResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponse<ProductResponse>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? category = null,
        [FromQuery] bool? isActive = null,
        CancellationToken ct = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var (items, total) = await products.GetPagedAsync(page, pageSize, category, isActive, ct);

        return Ok(new PagedResponse<ProductResponse>(
            items.Select(p => p.ToResponse()).ToList(),
            page, pageSize, total));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType<ProductResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductResponse>> GetById(int id, CancellationToken ct)
        => Ok((await products.GetByIdAsync(id, ct)).ToResponse());

    [HttpPost]
    [ProducesResponseType<ProductResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProductResponse>> Create(
        CreateProductRequest request, CancellationToken ct)
    {
        var created = await products.CreateAsync(request.ToEntity(), ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created.ToResponse());
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType<ProductResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProductResponse>> Update(
        int id, UpdateProductRequest request, CancellationToken ct)
    {
        var updated = await products.UpdateAsync(id, p =>
        {
            p.Name = request.Name;
            p.Description = request.Description;
            p.Price = request.Price;
            p.Category = request.Category;
            p.IsActive = request.IsActive;
        }, ct);

        return Ok(updated.ToResponse());
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(int id, CancellationToken ct)
    {
        await products.DeactivateAsync(id, ct);
        return NoContent();
    }
}
