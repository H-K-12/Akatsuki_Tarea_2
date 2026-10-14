namespace MicroservicioProducto
{
    public static class ProductoEndpoints
    {
        public static void MapProductoEndpoints(this WebApplication app)
        {
            var group = app.MapGroup("/api/producto");


        }
    }
}
