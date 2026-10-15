
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MicroservicioPedido.Entities;

public class Pedido
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    public string ProductoId { get; set; } = string.Empty;

    public int Cantidad { get; set; }

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
}
