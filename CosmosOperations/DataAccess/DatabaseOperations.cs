using Microsoft.Azure.Cosmos;
using CosmosOperations.Models;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

using System.Diagnostics;

namespace CosmosOperations.DataAccess
{
    public class DatabaseOperations<T> where T : IContainerItem, new()
    {
        private ILogger _logger;
        private readonly DatabaseClientSingleton _client;
        private Database? _database;
        private Container? _container;
        private readonly String _partitionKey;
        private readonly String _containerId;

        public DatabaseOperations (ILogger<DatabaseOperations<T>> logger, IConfiguration config, DatabaseClientSingleton client)
        {
            _logger = logger;
            _client = client;
            _partitionKey = config.GetSection("Settings").GetValue<String>("partitionKey");
            _containerId =  config.GetSection("Settings").GetValue<String>("containerName");

            try
            {
                _logger.LogInformation("Setting the Database and Container references");
                _database = _client.GetCosmosClient().GetDatabase(config.GetSection("Settings").GetValue<String>("databaseName"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }

        public async Task ConstructTestScaffolding()
        {
            //Delete existing container if it exists
           if (_database != null)
           {
               try
               {
                    var result = await _database.GetContainer(_containerId).DeleteContainerAsync();
                    //Recreate the container
                    ContainerProperties containerProperties = new ContainerProperties(_containerId, _partitionKey);
                    var createContainerResult = await _database.CreateContainerIfNotExistsAsync(containerProperties);
                    _container = createContainerResult.Container;
               }
               catch (Exception ex)
               {
                    _logger.LogError(ex.Message);
                    //Recreate the container
                    ContainerProperties containerProperties = new ContainerProperties(_containerId, _partitionKey);
                    var createContainerResult = await _database.CreateContainerIfNotExistsAsync(containerProperties);
               }
           }
           
        }

        public async Task<long> GetSingleItem(Guid id, String accountNumber)
        {
            try
            {
                if (_container != null)
                {
                    var response = await _container.ReadItemAsync<T>(id.ToString(), new PartitionKey(accountNumber));
                    if (response.StatusCode != System.Net.HttpStatusCode.OK)
                    {
                        _logger.LogInformation($"Item with Id [{id.ToString()}] could not be found resulting in response code: {response.StatusCode}");
                    }

                    return response.Diagnostics.GetClientElapsedTime().Milliseconds;
                }
                else
                {
                    throw new NullReferenceException("Container object is null");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw ex;
            }
        }

        public async Task<long> CreateSingleItem(T item)
        {
            try
            {
                if (_container != null)
                {
                    var options = new ItemRequestOptions() { EnableContentResponseOnWrite = false };

                    var response = await _container.CreateItemAsync<T>(item, new PartitionKey(item.GetPartitionKey()), options);
                    if (response.StatusCode != System.Net.HttpStatusCode.Created)
                    {
                        _logger.LogInformation($"Item rejected due to {response.StatusCode}");
                    }

                    return response.Diagnostics.GetClientElapsedTime().Milliseconds;
                }
                else
                {
                    throw new NullReferenceException("Container or Database object was null");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw ex;
            }
        }

        public async Task<long> DeleteSingleItem(Guid id, String accountNumber)
        {
            try
            {
                if (_container != null)
                {
                    var options = new ItemRequestOptions() { EnableContentResponseOnWrite = false };
                    var response = await _container.DeleteItemAsync<T>(id.ToString(), new PartitionKey(accountNumber), options);
                    if (response.StatusCode != System.Net.HttpStatusCode.NoContent)
                    {
                        _logger.LogInformation($"Item with Id [{id.ToString()}] failed to delete due to {response.StatusCode}");
                    }

                    return response.Diagnostics.GetClientElapsedTime().Milliseconds;
                }
                else
                {
                    throw new NullReferenceException("Container or Database object was null");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                throw ex;
            }
        }
    }
}