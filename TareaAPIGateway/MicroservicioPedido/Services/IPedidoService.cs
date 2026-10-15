
using MicroservicioPedido.Entities;

namespace MicroservicioPedido.Services;

public interface IPedidoService
{
    Task<List<Pedido>> ObtenerTodosAsync();
    Task<Pedido?> ObtenerPorIdAsync(string id);
    Task<ResultadoCreacionPedido> CrearAsync(string productoId, int cantidad);
}

public enum EstadoCreacionPedido
{
    Creado,
    DatosInvalidos,
    ProductoNoEncontrado,
    ProductosNoDisponible
}

public sealed record ResultadoCreacionPedido(
    EstadoCreacionPedido Estado,
    Pedido? Pedido = null);
