using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Management.DataFactory;
using Microsoft.Azure.Management.DataFactory.Models;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Rest;
using AuthenticationContext = Microsoft.IdentityModel.Clients.ActiveDirectory.AuthenticationContext;

namespace DataFactoryScan
{
    class DFSDataFactoryClient
    {
        private readonly DataFactoryManagementClient dataFactoryManagementClient;

        public string ResourceGroup { get; }

        internal DFSDataFactoryClient()
        {
            ResourceGroup = DFSKeyVaultClient.GetSecretAsync("DFSResourceGroup").GetAwaiter().GetResult();
            dataFactoryManagementClient = GetDataFactoryManagementClientAsync().GetAwaiter().GetResult();
        }

        /// <summary>
        /// Gets the data factory management client.
        /// </summary>
        /// <remarks>Follow these instructions to get the values for the strings in this function.
        /// https://docs.microsoft.com/en-us/azure/active-directory/develop/howto-create-service-principal-portal#create-an-azure-active-directory-application
        /// </remarks>
        /// <returns>DataFactoryManagementClient.</returns>
        private async Task<DataFactoryManagementClient> GetDataFactoryManagementClientAsync()
        {
            string tenantID = await DFSKeyVaultClient.GetSecretAsync("DFSTenantId").ConfigureAwait(false);
            string applicationId = await DFSKeyVaultClient.GetSecretAsync("DFSApplicationId").ConfigureAwait(false);
            string authenticationKey = await DFSKeyVaultClient.GetSecretAsync("DFSApplicationKey").ConfigureAwait(false);
            string subscriptionId = await DFSKeyVaultClient.GetSecretAsync("DFSSubscriptionId").ConfigureAwait(false);
            // Authenticate and create a data factory management client
            var context = new AuthenticationContext("https://login.windows.net/" + tenantID);
            ClientCredential cc = new ClientCredential(applicationId, authenticationKey);
            AuthenticationResult result = context.AcquireTokenAsync("https://management.azure.com/", cc).Result;
            ServiceClientCredentials credential = new TokenCredentials(result.AccessToken);
            var client = new DataFactoryManagementClient(credential) { SubscriptionId = subscriptionId };
            return client;
        }

        internal IEnumerable<Factory> FetchFactories()
        {
            return dataFactoryManagementClient.Factories.ListByResourceGroup(ResourceGroup);
        }

        internal IEnumerable<PipelineResource> FetchPipelines(Factory factory)
        {
            return dataFactoryManagementClient.Pipelines.ListByFactory(ResourceGroup, factory.Name);
        }

        internal IEnumerable<DatasetResource> FetchDatasets(Factory factory)
        {
            return dataFactoryManagementClient.Datasets.ListByFactory(ResourceGroup, factory.Name);
        }

        internal IEnumerable<LinkedServiceResource> FetchLinkedServices(Factory factory)
        {
            return dataFactoryManagementClient.LinkedServices.ListByFactory(ResourceGroup, factory.Name);
        }
    }
}
