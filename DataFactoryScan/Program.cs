using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Management.DataFactory;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Rest;
using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace DataFactoryScan
{
    class Program
    {
        /// <summary>
        /// Defines the entry point of the application.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <returns>System.Int32.  1 if the tables don't match data factory.  0 if they do.</returns>
        static int Main(string[] args)
        {
            return RunAsync().GetAwaiter().GetResult();
        }

        private static async Task<int> RunAsync()
        {
            var secret = await GetSecretAsync("ApplicationSecret").ConfigureAwait(false);
            //Console.WriteLine(secret);

            var client = GetDataFactoryManagementClient();

            var scanner = new Scanner(client);
            var allTablesMatch = scanner.ScanDataFactoryPipelines();  //You could parameterize this to take an array of PipelineNames to look through

            if (!allTablesMatch)
            {
                Console.WriteLine("ALERT!  THERE ARE SOME DIFFERENCES BETWEEN THE DATA FACTORY DEFINITIONS AND THE TABLE DEFINITIONS.");
                return 1;
            }
            else
            {
                Console.WriteLine("All tables match.");
                return 0;
            }
        }

        /// <summary>
        /// Use this to get a secret from your Key Vault.
        /// </summary>
        /// <remarks>This can be used to securely get the connection strings, passwords and authentication keys.</remarks>
        /// <param name="secretName">Name of the secret.</param>
        /// <returns>Task&lt;System.String&gt;.</returns>
        private static async Task<string> GetSecretAsync(string secretName)
        {
            var url = $"https://omwtmdatafactorykv.vault.azure.net/secrets/{secretName}" ;
            /* The below 4 lines of code shows you how to use AppAuthentication library to fetch secrets from your Key Vault*/
            var azureServiceTokenProvider = new AzureServiceTokenProvider();
            var keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));
            var secret = await keyVaultClient.GetSecretAsync(url).ConfigureAwait(false);
            return secret.Value;
        }

        /// <summary>
        /// Gets the data factory management client.
        /// </summary>
        /// <remarks>Follow these instructions to get the values for the strings in this function.
        /// https://docs.microsoft.com/en-us/azure/active-directory/develop/howto-create-service-principal-portal#create-an-azure-active-directory-application
        /// </remarks>
        /// <returns>DataFactoryManagementClient.</returns>
        private static DataFactoryManagementClient GetDataFactoryManagementClient()
        {
            string tenantID = "8a500803-5806-41c7-b3ce-44682c13fdd5";
            string applicationId = "84ca7b37-f85b-4a53-a1a7-0e5f9f66356b";
            string authenticationKey = "FeOH6+sSPIYtq0vGPK6tnsri8/OExAQyx0/dxzO9YOA=";
            string subscriptionId = "7810278c-6fd2-48d8-b77e-135508bea8b6";
            // Authenticate and create a data factory management client
            var context = new AuthenticationContext("https://login.windows.net/" + tenantID);
            ClientCredential cc = new ClientCredential(applicationId, authenticationKey);
            AuthenticationResult result = context.AcquireTokenAsync("https://management.azure.com/", cc).Result;
            ServiceClientCredentials cred = new TokenCredentials(result.AccessToken);
            var client = new DataFactoryManagementClient(cred) { SubscriptionId = subscriptionId };
            return client;
        }

        /// <summary>
        /// Gets the database client using the named connection string from app.config.
        /// </summary>
        /// <remarks>Though this is not used here, I left it in the project as a sample of how to do it.
        /// You could also replace the connString with a KeyVault secret if you want.</remarks>
        /// <param name="connStringName">Name of the connection string.</param>
        /// <returns>SqlConnection.</returns>
        private static SqlConnection GetDatabaseClient(string connStringName = "DFSDB")
        {
            var connString = ConfigurationManager.ConnectionStrings[connStringName].ConnectionString;
            //Or do this:  var connString = await GetSecretAsync(connStringName).ConfigureAwait(false);
            var db = new SqlConnection(connString);
            //db.Open();
            return db;
        }

    }
}
