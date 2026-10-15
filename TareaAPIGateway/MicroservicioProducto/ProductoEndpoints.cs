using MicroservicioProducto.Entities;
using MicroservicioProducto.Services;

namespace MicroservicioProducto
{
    public static class ProductoEndpoints
    {
        public static void MapProductoEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/api/producto");

            group.MapPost("/", CreateAsync);
            group.MapGet("/", GetAllAsync);
            group.MapGet("/{id}", GetByIdAsync);
        }

        private static async Task<IResult> CreateAsync(
            ProductoRequest request,
            IProductoService service)
        {
            try
            {
                var productoCreado = await service.CreateAsync(request);
                return Results.Created($"/api/producto/{productoCreado.IDProducto}", productoCreado);
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                return Results.Problem(
                    detail: ex.Message,
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "Error interno al crear el producto");
            }
        }

        private static async Task<IResult> GetAllAsync(IProductoService service)
        {
            try
            {
                var productos = await service.GetAllAsync();
                return Results.Ok(productos);
            }
            catch (Exception ex)
            {
                return Results.Problem(
                    detail: ex.Message,
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "Error al obtener la lista de productos");
            }
        }

        private static async Task<IResult> GetByIdAsync(
            string id,
            IProductoService service)
        {
            try
            {
                var producto = await service.GetByIdAsync(id);

                if (producto is null)
                    return Results.NotFound(new { Error = $"Producto con ID '{id}' no encontrado" });

                return Results.Ok(producto);
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                return Results.Problem(
                    detail: ex.Message,
                    statusCode: StatusCodes.Status500InternalServerError,
                    title: "Error al obtener el producto");
            }
        }
    }
}