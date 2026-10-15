
using MicroservicioPedido.Services;
using MongoDB.Bson;

namespace MicroservicioPedido;

public static class PedidoEndpoints
{
    public record CrearPedidoSolicitud(string ProductoId, int Cantidad);

    public static void MapearPedidos(this WebApplication app)
    {
        var rutas = app.MapGroup("/api/ordenes").WithTags("Pedidos");

        rutas.MapGet("", async (IPedidoService servicio) =>
        {
            var pedidos = await servicio.ObtenerTodosAsync();
            return Results.Ok(pedidos);
        });

        rutas.MapGet("/{id}", async (string id, IPedidoService servicio) =>
        {
            if (!ObjectId.TryParse(id, out _))
                return Results.BadRequest(new { mensaje = "El ID del pedido no es valido" });

            var pedido = await servicio.ObtenerPorIdAsync(id);

            return pedido is null ? Results.NotFound() : Results.Ok(pedido);
        });

        rutas.MapPost("", async (CrearPedidoSolicitud solicitud, IPedidoService servicio) =>
        {
            var resultado = await servicio.CrearAsync(solicitud.ProductoId, solicitud.Cantidad);

            return resultado.Estado switch
            {
                EstadoCreacionPedido.Creado => Results.Created($"/api/ordenes/{resultado.Pedido!.Id}", resultado.Pedido),
                EstadoCreacionPedido.DatosInvalidos => Results.BadRequest(new { mensaje = "Los datos del pedido no son validos" }),
                EstadoCreacionPedido.ProductoNoEncontrado => Results.BadRequest(new { mensaje = "El producto no existe" }),
                EstadoCreacionPedido.ProductosNoDisponible => Results.Problem("El servicio de productos no esta disponible", statusCode: 503),
                _ => Results.Problem("Error al crear el pedido", statusCode: 500)
            };
        });
    }
}
