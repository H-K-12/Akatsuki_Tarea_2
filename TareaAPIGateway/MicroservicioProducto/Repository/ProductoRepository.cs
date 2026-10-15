using MicroservicioProducto.Entities;
using MongoDB.Driver;

namespace MicroservicioProducto.Repository
{
    public class ProductoRepository : IProductoRepository
    {
        private readonly IMongoCollection<ProductoResponse> _productosCollection;

        public ProductoRepository(IDBConnectionFactory dbConnectionFactory)
        {
            var database = dbConnectionFactory.CreateConnection();
            _productosCollection = database.GetCollection<ProductoResponse>("Productos");
        }

        public async Task<ProductoResponse> CreateAsync(ProductoRequest request)
        {
            try
            {
                var producto = new ProductoResponse
                {
                    Nombre = request.Nombre,
                    Precio = request.Precio
                };

                await _productosCollection.InsertOneAsync(producto);
                return producto;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al crear el producto: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<ProductoResponse>> GetAllAsync()
        {
            try
            {
                return await _productosCollection.Find(_ => true).ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener los productos: {ex.Message}", ex);
            }
        }

        public async Task<ProductoResponse?> GetByIdAsync(string id)
        {
            try
            {
                return await _productosCollection
                    .Find(p => p.IDProducto == id)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener el producto con ID {id}: {ex.Message}", ex);
            }
        }
    }
}