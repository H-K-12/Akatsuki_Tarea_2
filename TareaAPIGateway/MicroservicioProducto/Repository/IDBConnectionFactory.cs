using MongoDB.Driver;
using System.Data;

namespace MicroservicioProducto.Repository
{
    public interface IDBConnectionFactory
    {
        IMongoDatabase CreateConnection();
    }
}
