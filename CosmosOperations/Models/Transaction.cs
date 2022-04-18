using System.Text.Json.Serialization;
using System.Text.Json;

namespace CosmosOperations.Models;

public class Transaction : IContainerItem
{
    private Random _randomizer;
    public Guid id { get; set; }
    public String AccountNumber { get; set; } = String.Empty;
    [JsonPropertyName("transactionId")]
    public Guid TransactionId { get; set; } = Guid.NewGuid();
    [JsonPropertyName("transactionTimestamp")]
    public DateTime TransactionTimestamp { get; set; }
    public Decimal TransactionAmount { get; set; }
    
    public Transaction() 
    {
        _randomizer = new Random(DateTime.Now.Second * DateTime.Now.DayOfYear);
        this.TransactionId = Guid.NewGuid();
        this.id = TransactionId;
        this.TransactionTimestamp = DateTime.UtcNow;
        this.TransactionAmount = new Decimal(_randomizer.NextInt64() * _randomizer.NextDouble());
    }
    public Transaction(String accountNumber) : this()
    {
        this.AccountNumber = accountNumber;
    }

    public string GetJsonRepresentation()
    {
        return JsonSerializer.Serialize<Transaction>(this);
    }

    public string GetPartitionKey()
    {
        return this.AccountNumber;
    }
}