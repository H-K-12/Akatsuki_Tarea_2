
using System.Net;
using MicroservicioPedido.Entities;
using MicroservicioPedido.Repository;

namespace MicroservicioPedido.Services;

public class PedidoService : IPedidoService
{
    private readonly IPedidoRepository _repositorio;
    private readonly HttpClient _clienteProductos;
    private readonly ILogger<PedidoService> _logger;

    public PedidoService(IPedidoRepository repositorio, HttpClient clienteProductos, ILogger<PedidoService> logger)
    {
        _repositorio = repositorio;
        _clienteProductos = clienteProductos;
        _logger = logger;
    }

    public async Task<List<Pedido>> ObtenerTodosAsync()
    {
        return await _repositorio.ObtenerTodosAsync();
    }

    public async Task<Pedido?> ObtenerPorIdAsync(string id)
    {
        return await _repositorio.ObtenerPorIdAsync(id);
    }

    public async Task<ResultadoCreacionPedido> CrearAsync(string productoId, int cantidad)
    {
        if (string.IsNullOrWhiteSpace(productoId) || cantidad <= 0)
            return new ResultadoCreacionPedido(EstadoCreacionPedido.DatosInvalidos);

        productoId = productoId.Trim();

        try
        {
            var respuesta = await _clienteProductos.GetAsync($"api/productos/{Uri.EscapeDataString(productoId)}");

            if (respuesta.StatusCode == HttpStatusCode.NotFound)
                return new ResultadoCreacionPedido(EstadoCreacionPedido.ProductoNoEncontrado);

            if (!respuesta.IsSuccessStatusCode)
                return new ResultadoCreacionPedido(EstadoCreacionPedido.ProductosNoDisponible);
        }
        catch (HttpRequestException excepcion)
        {
            _logger.LogWarning(excepcion, "No se pudo consultar el microservicio de Productos");
            return new ResultadoCreacionPedido(EstadoCreacionPedido.ProductosNoDisponible);
        }
        catch (TaskCanceledException excepcion)
        {
            _logger.LogWarning(excepcion, "Tiempo de espera agotado al consultar Productos");
            return new ResultadoCreacionPedido(EstadoCreacionPedido.ProductosNoDisponible);
        }

        var pedido = new Pedido { ProductoId = productoId, Cantidad = cantidad };

        await _repositorio.CrearAsync(pedido);
        _logger.LogInformation("Pedido creado correctamente: {PedidoId}", pedido.Id);

        return new ResultadoCreacionPedido(EstadoCreacionPedido.Creado, pedido);
    }
}
