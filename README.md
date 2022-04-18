# AzurePaaSPerfTests
Repository to hold projects for testing different Azure PaaS services

## CosmosOperations
This project allows for a variable number of transactions to be sent to CosmosDB using the .NET SDK v3. It flexes three major access patterns: Create, Retrieve, and Delete. Configurability is done via an appsettings.<environment>.json file. Ideally, future versions could externalize this configuration. 
