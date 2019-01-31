using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using Microsoft.Azure.Management.DataFactory;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.KeyVault.Models;
using Microsoft.Azure.Services.AppAuthentication;
using System.Threading.Tasks;

namespace DataFactoryScan.Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestDataBaseConnection()
        {
            var connString = ConfigurationManager.ConnectionStrings["DFSDB"].ConnectionString;
            var db = new SqlConnection(connString);
            db.Open();
            Assert.AreEqual(ConnectionState.Open, db.State);
            db.Close();
            db.Dispose();
        }

        [TestMethod]
        public async Task TestDataFactoryConnectionAsync()
        {
            //string tenantId = "8a500803-5806-41c7-b3ce-44682c13fdd5";
            //string clientId = "";
            //string clientKey = "";
            //var login = new MSILoginInformation(MSIResourceType.AppService);
            //var tokenCredentials = "" ;
            //var subscriptionId = "7810278c-6fd2-48d8-b77e-135508bea8b6";
            //var dfClient = new DataFactoryManagementClient(tokenCredentials) { SubscriptionId = subscriptionId };

            //var azureServiceTokenProvider = new AzureServiceTokenProvider();
            //var keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));
            //var secret = await keyVaultClient.GetSecretAsync()
            //        .ConfigureAwait(false);
            //Message = secret.Value;

            var secret = await LoginWithSecretAsync();
            Assert.AreEqual("uOU8v K AtP QS9x(:.7yj Z f s0t96j/C43!+0rPu/gzK1|w", secret);




        }

        private async Task<string> LoginWithSecretAsync()
        {
            var url = "https://omwtmdatafactorykv.vault.azure.net/secrets/ApplicationSecret/55b35de4703b44b89deccd92506b4669";
            /* The below 4 lines of code shows you how to use AppAuthentication library to fetch secrets from your Key Vault*/
            var azureServiceTokenProvider = new AzureServiceTokenProvider();
            var keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));
            var secret = await keyVaultClient.GetSecretAsync(url).ConfigureAwait(false);
            return secret.Value;
        }

        // This method implements exponential back off in case of 429 errors from Azure Key Vault
        private static long getWaitTime(int retryCount)
        {
            long waitTime = ((long)Math.Pow(2, retryCount) * 100L);
            return waitTime;
        }

        // This method fetches a token from Azure Active Directory which can then be provided to Azure Key Vault to authenticate
        public async Task<string> GetAccessTokenAsync()
        {
            var azureServiceTokenProvider = new AzureServiceTokenProvider();
            string accessToken = await azureServiceTokenProvider.GetAccessTokenAsync("https://vault.azure.net");
            return accessToken;
        }
    }
}
