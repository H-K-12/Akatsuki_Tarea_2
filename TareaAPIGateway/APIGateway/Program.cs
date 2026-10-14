using System.Diagnostics;
using APIGateway.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));
builder.Services.AddHttpClient("Pedidos", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:Pedidos"]
        ?? throw new InvalidOperationException("No se configuró Services:Pedidos."));
    client.Timeout = TimeSpan.FromSeconds(10);
});
builder.Services.AddHttpClient("Productos", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:Productos"]
        ?? throw new InvalidOperationException("No se configuró Services:Productos."));
    client.Timeout = TimeSpan.FromSeconds(10);
});
builder.Services.AddScoped<ResumenOrdenService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseExceptionHandler();

app.Use(async (context, next) =>
{
    var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault();
    if (string.IsNullOrWhiteSpace(correlationId))
    {
        correlationId = Guid.NewGuid().ToString("N");
    }

    context.Request.Headers["X-Correlation-ID"] = correlationId;
    context.Response.Headers["X-Correlation-ID"] = correlationId;

    var logger = context.RequestServices.GetRequiredService<ILoggerFactory>()
        .CreateLogger("GatewayRequests");
    var stopwatch = Stopwatch.StartNew();

    using (logger.BeginScope(new Dictionary<string, object>
    {
        ["CorrelationId"] = correlationId
    }))
    {
        logger.LogInformation(
            "Solicitud recibida {Metodo} {Ruta}",
            context.Request.Method,
            context.Request.Path);

        await next();

        stopwatch.Stop();
        logger.LogInformation(
            "Solicitud completada {Metodo} {Ruta} {Estado} en {DuracionMs} ms",
            context.Request.Method,
            context.Request.Path,
            context.Response.StatusCode,
            stopwatch.ElapsedMilliseconds);
    }
});

app.MapGet("/health", () => Results.Ok(new
{
    servicio = "APIGateway",
    estado = "Disponible",
    fechaUtc = DateTimeOffset.UtcNow
}));

app.MapGet("/api/resumen-orden/{id}", async (
    string id,
    ResumenOrdenService service,
    CancellationToken cancellationToken) =>
{
    var resultado = await service.ObtenerAsync(id, cancellationToken);

    return resultado.Contenido is not null
        ? Results.Json(resultado.Contenido, statusCode: resultado.CodigoEstado)
        : Results.Problem(
            title: "No fue posible generar el resumen de la orden",
            detail: resultado.Error,
            statusCode: resultado.CodigoEstado);
});

app.MapReverseProxy();

app.Run();
