using System.Net;
using System.Text.Json.Nodes;

namespace APIGateway.Services;

public sealed class ResumenOrdenService(
    IHttpClientFactory httpClientFactory,
    ILogger<ResumenOrdenService> logger)
{
    private static readonly HashSet<string> NombresProductoId = new(StringComparer.OrdinalIgnoreCase)
    {
        "productoId",
        "productId",
        "idProducto"
    };

    public async Task<ResultadoResumenOrden> ObtenerAsync(
        string ordenId,
        CancellationToken cancellationToken)
    {
        var pedidosClient = httpClientFactory.CreateClient("Pedidos");
        var productosClient = httpClientFactory.CreateClient("Productos");

        try
        {
            using var ordenResponse = await pedidosClient.GetAsync(
                $"api/ordenes/{Uri.EscapeDataString(ordenId)}",
                cancellationToken);

            if (!ordenResponse.IsSuccessStatusCode)
            {
                return new ResultadoResumenOrden(
                    (int)ordenResponse.StatusCode,
                    null,
                    $"El servicio de pedidos respondió {(int)ordenResponse.StatusCode}.");
            }

            var ordenTexto = await ordenResponse.Content.ReadAsStringAsync(cancellationToken);
            var orden = JsonNode.Parse(ordenTexto);

            if (orden is null)
            {
                return new ResultadoResumenOrden(
                    StatusCodes.Status502BadGateway,
                    null,
                    "El servicio de pedidos devolvió una respuesta vacía.");
            }

            var productoIds = ExtraerProductoIds(orden).Distinct(StringComparer.OrdinalIgnoreCase);
            var productos = new JsonArray();

            foreach (var productoId in productoIds)
            {
                using var productoResponse = await productosClient.GetAsync(
                    $"api/productos/{Uri.EscapeDataString(productoId)}",
                    cancellationToken);

                if (productoResponse.IsSuccessStatusCode)
                {
                    var productoTexto = await productoResponse.Content.ReadAsStringAsync(cancellationToken);
                    productos.Add(new JsonObject
                    {
                        ["productoId"] = productoId,
                        ["detalle"] = JsonNode.Parse(productoTexto)
                    });
                }
                else
                {
                    productos.Add(new JsonObject
                    {
                        ["productoId"] = productoId,
                        ["estado"] = (int)productoResponse.StatusCode,
                        ["error"] = "No fue posible obtener el producto."
                    });
                }
            }

            var contenido = new JsonObject
            {
                ["orden"] = orden.DeepClone(),
                ["productos"] = productos,
                ["generadoUtc"] = DateTimeOffset.UtcNow
            };

            return new ResultadoResumenOrden(StatusCodes.Status200OK, contenido, null);
        }
        catch (HttpRequestException exception)
        {
            logger.LogError(exception, "Error de comunicación al generar el resumen de la orden {OrdenId}", ordenId);
            return new ResultadoResumenOrden(
                StatusCodes.Status503ServiceUnavailable,
                null,
                "Uno de los microservicios requeridos no está disponible.");
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return new ResultadoResumenOrden(
                StatusCodes.Status504GatewayTimeout,
                null,
                "Uno de los microservicios excedió el tiempo de respuesta permitido.");
        }
        catch (System.Text.Json.JsonException exception)
        {
            logger.LogError(exception, "Respuesta JSON inválida al generar el resumen de la orden {OrdenId}", ordenId);
            return new ResultadoResumenOrden(
                StatusCodes.Status502BadGateway,
                null,
                "Uno de los microservicios devolvió una respuesta JSON inválida.");
        }
    }

    private static IEnumerable<string> ExtraerProductoIds(JsonNode node)
    {
        if (node is JsonObject objeto)
        {
            foreach (var propiedad in objeto)
            {
                if (propiedad.Value is not null && NombresProductoId.Contains(propiedad.Key))
                {
                    var valor = ObtenerValorTexto(propiedad.Value);
                    if (!string.IsNullOrWhiteSpace(valor))
                    {
                        yield return valor;
                    }
                }

                if (propiedad.Value is not null)
                {
                    foreach (var valor in ExtraerProductoIds(propiedad.Value))
                    {
                        yield return valor;
                    }
                }
            }
        }
        else if (node is JsonArray arreglo)
        {
            foreach (var elemento in arreglo)
            {
                if (elemento is null)
                {
                    continue;
                }

                foreach (var valor in ExtraerProductoIds(elemento))
                {
                    yield return valor;
                }
            }
        }
    }

    private static string? ObtenerValorTexto(JsonNode node)
    {
        if (node is not JsonValue valor)
        {
            return null;
        }

        if (valor.TryGetValue<string>(out var texto))
        {
            return texto;
        }

        if (valor.TryGetValue<int>(out var entero))
        {
            return entero.ToString();
        }

        if (valor.TryGetValue<long>(out var largo))
        {
            return largo.ToString();
        }

        if (valor.TryGetValue<Guid>(out var guid))
        {
            return guid.ToString();
        }

        return null;
    }
}

public sealed record ResultadoResumenOrden(
    int CodigoEstado,
    JsonNode? Contenido,
    string? Error);
