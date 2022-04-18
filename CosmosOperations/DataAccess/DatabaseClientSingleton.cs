using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;


namespace CosmosOperations.DataAccess
{
    public class DatabaseClientSingleton
    {
        private const string DB_CONNECTION_STRING_KEY = "cosmosConnectionString";
        private CosmosClient _client;
        public DatabaseClientSingleton(IConfiguration config)
        {
            string connectionString = config.GetConnectionString(DB_CONNECTION_STRING_KEY);
            _client = new CosmosClient(connectionString, new CosmosClientOptions() {
                ConnectionMode = ConnectionMode.Direct,
                EnableTcpConnectionEndpointRediscovery = true,
            });
        }

        public CosmosClient GetCosmosClient()
        {
            return _client;
        }
    }
}