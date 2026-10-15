using MicroservicioProducto.Entities;

namespace MicroservicioProducto.Services
{
    public interface IProductoService
    {
        Task<ProductoResponse> CreateAsync(ProductoRequest request);
        Task<IEnumerable<ProductoResponse>> GetAllAsync();
        Task<ProductoResponse?> GetByIdAsync(string id);
    }
}