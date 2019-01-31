using System.Threading.Tasks;
using Microsoft.Azure.Management.DataFactory;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Rest;
using AuthenticationContext = Microsoft.IdentityModel.Clients.ActiveDirectory.AuthenticationContext;

namespace DataFactoryScan
{
    class DFSDataFactoryClient
    {
        /// <summary>
        /// Gets the data factory management client.
        /// </summary>
        /// <remarks>Follow these instructions to get the values for the strings in this function.
        /// https://docs.microsoft.com/en-us/azure/active-directory/develop/howto-create-service-principal-portal#create-an-azure-active-directory-application
        /// </remarks>
        /// <returns>DataFactoryManagementClient.</returns>
        internal static async Task<DataFactoryManagementClient> GetDataFactoryManagementClientAsync()
        {
            string tenantID = await DFSKeyVaultClient.GetSecretAsync("DFSTenantId").ConfigureAwait(false);
            string applicationId = await DFSKeyVaultClient.GetSecretAsync("DFSApplicationId").ConfigureAwait(false);
            string authenticationKey = await DFSKeyVaultClient.GetSecretAsync("DFSApplicationKey").ConfigureAwait(false);
            string subscriptionId = await DFSKeyVaultClient.GetSecretAsync("DFSSubscriptionId").ConfigureAwait(false);
            // Authenticate and create a data factory management client
            var context = new AuthenticationContext("https://login.windows.net/" + tenantID);
            ClientCredential cc = new ClientCredential(applicationId, authenticationKey);
            AuthenticationResult result = context.AcquireTokenAsync("https://management.azure.com/", cc).Result;
            ServiceClientCredentials cred = new TokenCredentials(result.AccessToken);
            var client = new DataFactoryManagementClient(cred) { SubscriptionId = subscriptionId };
            return client;
        }
    }
}
