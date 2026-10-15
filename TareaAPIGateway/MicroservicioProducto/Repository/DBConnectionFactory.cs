using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace MicroservicioProducto.Repository
{
    public class DBConnectionFactory : IDBConnectionFactory
    {
        private readonly IMongoDatabase _database;

        public DBConnectionFactory(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("MongoConnection");
            var databaseName = configuration["DatabaseName"] ?? "ProductosDB";

            var client = new MongoClient(connectionString);
            _database = client.GetDatabase(databaseName);
        }

        public IMongoDatabase CreateConnection()
        {
            return _database;
        }
    }
} 