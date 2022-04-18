using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

using CosmosOperations.DataAccess;
using CosmosOperations.Models;

namespace CosmosOperations 
{
    public class TestDriverConsole : IHostedService
    {
        private ILogger _logger;
        private IConfiguration _config;
        private IHostApplicationLifetime _appLifetime;
        private DatabaseOperations<Transaction> _transactionDatabaseOperations;
        private List<String> accountIdList = new List<string>() { "000001", "000010", "000011", "000100", "000101", "000111", "001000", "001001", "001010", "001011", "001100", "001101", "001110", "001111", "010000" };

        public TestDriverConsole(ILogger<TestDriverConsole> logger, IConfiguration config, IHostApplicationLifetime applicationLifetime, DatabaseOperations<Transaction> transactionDatabaseOperations)
        {
            _logger = logger;
            _config = config;
            _appLifetime = applicationLifetime;
            _transactionDatabaseOperations = transactionDatabaseOperations;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Started the TestDriverConsole hosted service");
            
            _appLifetime.ApplicationStarted.Register(() =>
            {
                Task.Run(async () =>
                {
                    try
                    {
                        //Clean the CosmosDB container for a fresh test
                        await _transactionDatabaseOperations.ConstructTestScaffolding();

                        _logger.LogInformation("Beginning simulated load");

                        Random accountSelector = new Random(DateTime.Now.Millisecond);
                        Dictionary<Guid, String> partitionMap = new Dictionary<Guid, String>();

                        List<Transaction> simulationLoad = new List<Transaction>();
                        int transactionCount = _config.GetSection("Settings").GetValue<int>("numberOfSimulatedTransactions");

                        for (int i = 0; i < transactionCount; i++) 
                        { 
                            var accountNumber = accountIdList[accountSelector.Next(0, accountIdList.Count - 1)];
                            var transaction = new Transaction(accountNumber);
                            simulationLoad.Add(transaction);
                            partitionMap.Add(transaction.TransactionId, accountNumber);
                        }

                        long writeOperations = 0;
                        long getOperations = 0;
                        long deleteOperations = 0;
                        foreach (Transaction transaction in simulationLoad)
                        {
                            writeOperations += await _transactionDatabaseOperations.CreateSingleItem(transaction);
                        }

                        foreach (KeyValuePair<Guid, String> entry in partitionMap)
                        {
                            getOperations += await _transactionDatabaseOperations.GetSingleItem(entry.Key, entry.Value);
                        }

                        foreach (KeyValuePair<Guid, String> entry in partitionMap)
                        {
                            deleteOperations += await _transactionDatabaseOperations.DeleteSingleItem(entry.Key, entry.Value);
                        }

                        _logger.LogInformation($"Summary of Simluations for {transactionCount} Transactions:\n Create Operations:\t{writeOperations/transactionCount}\n Get Operations:\t{getOperations/transactionCount}\n Delete Operations:\t{deleteOperations/transactionCount}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Unhandled exception!");
                    }
                    finally
                    {
                        // Stop the application once the work is done
                        _appLifetime.StopApplication();
                    }
                });
            });
            
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}