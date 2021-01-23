using System.Threading.Tasks;
using Api.Helpers;
using Api.Models.Data;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Logging;

namespace Api.Services
{
    public interface ILinkedAccountService
    {
        Task<LinkedAccount> GetLinkedAccountAsync(string userId, CloudTable table, ILogger logger);
    }

    public class LinkedAccountService : ILinkedAccountService
    {
        public async Task<LinkedAccount> GetLinkedAccountAsync(string userId, CloudTable table, ILogger logger)
        {
            try
            {
                TableOperation retrieveOperation = TableOperation.Retrieve<LinkedAccount>(Constants.LinkedAccountPartitionKey, userId);
                TableResult result = await table.ExecuteAsync(retrieveOperation);
                LinkedAccount linkedAccount = result.Result as LinkedAccount;

                return linkedAccount;
            }
            catch (StorageException ex)
            {
                logger.LogError(ex, "Cannot read linked account from the table storage");
                return null;
            }
        }
    }
}
