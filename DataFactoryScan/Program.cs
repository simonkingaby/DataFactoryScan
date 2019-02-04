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
            var scanner = new Scanner();
            var allTablesMatch = await scanner.ScanDataFactoryPipelinesAsync().ConfigureAwait(false);  //You could parameterize this to take an array of PipelineNames to look through

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
    }
}
