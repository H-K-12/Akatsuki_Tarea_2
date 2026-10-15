
using MicroservicioPedido;
using MicroservicioPedido.Repository;
using MicroservicioPedido.Services;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();

builder.Services.AddSingleton<IMongoClient>(_ =>
{
    var conexion = builder.Configuration["MongoDB:CadenaConexion"]
        ?? throw new InvalidOperationException("No se configuro la conexion a MongoDB");

    return new MongoClient(conexion);
});

builder.Services.AddScoped<IPedidoRepository, PedidoRepository>();

builder.Services.AddHttpClient<IPedidoService, PedidoService>(cliente =>
{
    var url = builder.Configuration["Productos:UrlBase"]
        ?? throw new InvalidOperationException("No se configuro la URL de Productos");

    cliente.BaseAddress = new Uri(url);
    cliente.Timeout = TimeSpan.FromSeconds(5);
});

var app = builder.Build();

app.UseExceptionHandler();

app.Use(async (contexto, siguiente) =>
{
    app.Logger.LogInformation("Solicitud recibida: {Metodo} {Ruta}", contexto.Request.Method, contexto.Request.Path);

    await siguiente(contexto);

    app.Logger.LogInformation("Respuesta enviada: HTTP {Estado}", contexto.Response.StatusCode);
});

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapearPedidos();

app.Run();
