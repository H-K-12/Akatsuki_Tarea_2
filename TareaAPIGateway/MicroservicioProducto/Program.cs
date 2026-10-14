using MicroservicioProducto;
using MicroservicioProducto.Repository;
using MicroservicioProducto.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddOpenApi();

// Inyección de Dependencias
builder.Services.AddScoped<IProductoRepository, ProductoRepository>();
builder.Services.AddScoped<IProductoService, ProductoService>(); 

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapProductoEndpoints();

app.Run();
