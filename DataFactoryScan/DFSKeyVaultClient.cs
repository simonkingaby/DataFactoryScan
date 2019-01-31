using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using System.Threading.Tasks;

namespace DataFactoryScan
{
    class DFSKeyVaultClient
    {
        /// <summary>
        /// Use this to get a secret from your Key Vault.
        /// </summary>
        /// <remarks>This can be used to securely get the connection strings, passwords and authentication keys.</remarks>
        /// <param name="secretName">Name of the secret.</param>
        /// <returns>Task&lt;System.String&gt;.</returns>
        internal static async Task<string> GetSecretAsync(string secretName)
        {
            var url = $"https://omwtmdatafactorykv.vault.azure.net/secrets/{secretName}";
            /* The below 4 lines of code shows you how to use AppAuthentication library to fetch secrets from your Key Vault*/
            var azureServiceTokenProvider = new AzureServiceTokenProvider();
            var keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));
            var secret = await keyVaultClient.GetSecretAsync(url).ConfigureAwait(false);
            return secret.Value;
        }
    }
}
