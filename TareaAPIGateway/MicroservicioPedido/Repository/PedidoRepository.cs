
using MicroservicioPedido.Entities;
using MongoDB.Driver;

namespace MicroservicioPedido.Repository;

public class PedidoRepository : IPedidoRepository
{
    private readonly IMongoCollection<Pedido> _pedidos;

    public PedidoRepository(IMongoClient cliente, IConfiguration configuracion)
    {
        var nombreBaseDatos = configuracion["MongoDB:BaseDatos"]
            ?? throw new InvalidOperationException("No se configuro la base de datos MongoDB");

        var baseDatos = cliente.GetDatabase(nombreBaseDatos);
        _pedidos = baseDatos.GetCollection<Pedido>("Pedidos");
    }

    public async Task<List<Pedido>> ObtenerTodosAsync()
    {
        return await _pedidos.Find(_ => true).ToListAsync();
    }

    public async Task<Pedido?> ObtenerPorIdAsync(string id)
    {
        return await _pedidos.Find(pedido => pedido.Id == id).FirstOrDefaultAsync();
    }

    public async Task CrearAsync(Pedido pedido)
    {
        await _pedidos.InsertOneAsync(pedido);
    }
}
