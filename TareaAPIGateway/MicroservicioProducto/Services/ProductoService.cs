using MicroservicioProducto.Entities;
using MicroservicioProducto.Repository;
using System.ComponentModel.DataAnnotations;

namespace MicroservicioProducto.Services
{
    public class ProductoService : IProductoService
    {
        private readonly IProductoRepository _repository;

        public ProductoService(IProductoRepository repository)
        {
            _repository = repository;
        }

        public async Task<ProductoResponse> CreateAsync(ProductoRequest request)
        {
            // Validación con DataAnnotations
            var validationResults = new List<ValidationResult>();
            var validationContext = new ValidationContext(request);

            if (!Validator.TryValidateObject(request, validationContext, validationResults, true))
            {
                var errores = string.Join(" | ", validationResults.Select(v => v.ErrorMessage));
                throw new ArgumentException(errores);
            }

            // Validaciones de negocio adicionales
            if (string.IsNullOrWhiteSpace(request.Nombre))
                throw new ArgumentException("El nombre no puede estar vacío");

            if (request.Precio <= 0)
                throw new ArgumentException("El precio debe ser mayor a cero");

            return await _repository.CreateAsync(request);
        }

        public async Task<IEnumerable<ProductoResponse>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<ProductoResponse?> GetByIdAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("El ID del producto es obligatorio");

            return await _repository.GetByIdAsync(id);
        }
    }
}