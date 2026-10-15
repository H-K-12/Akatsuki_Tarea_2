using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace MicroservicioProducto.Entities
{
    public class ProductoResponse
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string IDProducto { get; set; } = string.Empty;

        [Required]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        public decimal Precio { get; set; }
    }
}
