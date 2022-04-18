namespace CosmosOperations.Models
{
    public interface IContainerItem
    {
        public String GetJsonRepresentation();
        public String GetPartitionKey();
    }
}