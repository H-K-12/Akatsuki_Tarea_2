
using MicroservicioPedido.Entities;

namespace MicroservicioPedido.Repository;

public interface IPedidoRepository
{
    Task<List<Pedido>> ObtenerTodosAsync();

    Task<Pedido?> ObtenerPorIdAsync(string id);

    Task CrearAsync(Pedido pedido);
}
