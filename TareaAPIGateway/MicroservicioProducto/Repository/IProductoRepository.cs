using MicroservicioProducto.Entities;

namespace MicroservicioProducto.Repository
{
    public interface IProductoRepository
    {
        Task<ProductoResponse> CreateAsync(ProductoRequest request);
        Task<IEnumerable<ProductoResponse>> GetAllAsync();
        Task<ProductoResponse?> GetByIdAsync(string id);
    }
}