using Microsoft.Azure.Management.DataFactory;
using Microsoft.Azure.Management.DataFactory.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace DataFactoryScan
{
    class Scanner
    {
        private readonly DataFactoryManagementClient dataFactoryManagementClient;
        private readonly string resourceGroup = "DataFactoryRG";

        /// <summary>
        /// Initializes a new instance of the <see cref="Scanner"/> class.
        /// </summary>
        /// <param name="dataFactoryManagementClient">The data factory management client.</param>
        internal Scanner(DataFactoryManagementClient dataFactoryManagementClient)
        {
            this.dataFactoryManagementClient = dataFactoryManagementClient;
        }

        /// <summary>
        /// Scans the data factory pipelines to compare the tables defined in the pipeline activities and the corresponding database tables.
        /// </summary>
        /// <returns><c>true</c> if all tables match, <c>false</c> otherwise.</returns>
        internal bool ScanDataFactoryPipelines()
        {
            //foreach pipeline in DataFactory
            //foreach activity in pipeline.properties.activities where type = "Copy"
            //read typeProperties.source.type and typeProperties.sink.type
            //if source type or sink type are databases
            //read inputs.referenceName (DataSet Name)
            //read outputs.referenceName (DataSet Name)
            //fetch input dataset.schema/dataset.structure/typeProperties.tableName
            //compare them

            var allTablesMatch = true;

            var factories = FetchFactories();
            foreach (var factory in factories)
            {
                Console.WriteLine($"Factory:      {factory.Name}");

                var datasets = FetchDatasets(factory);
                var linkedServices = FetchLinkedServices(factory);
                var pipelines = FetchPipelines(factory);
                foreach (var pipeline in pipelines)
                {
                    Console.WriteLine($"Pipeline:     {pipeline.Name}");

                    foreach (var activity in pipeline.Activities)
                    {
                        Console.WriteLine($"Activity:     {activity.Name}");

                        if (typeof(ControlActivity).IsAssignableFrom(activity.GetType()))
                        {
                            //TODO:  Handle nested activities
                        }
                        else if (typeof(ExecutionActivity).IsAssignableFrom(activity.GetType()))
                        {
                            if (typeof(CopyActivity).IsAssignableFrom(activity.GetType())) //TODO:  Make this work for other Activity Types
                            {
                                var copyActivity = (CopyActivity)activity;
                                Console.WriteLine($"CopyActivity: {copyActivity.Name}");

                                foreach (var datasetReference in copyActivity.Inputs)  //TODO:  Make the code below run for Inputs and Outputs
                                {
                                    var datasetResource = datasets.Where(x => x.Name == datasetReference.ReferenceName).FirstOrDefault();
                                    if (datasetResource != null)
                                    {
                                        var dataset = datasetResource.Properties;
                                        //https://docs.microsoft.com/en-us/dotnet/api/microsoft.azure.management.datafactory.models.dataset?view=azure-dotnet
                                        if (typeof(AzureSqlTableDataset).IsAssignableFrom(dataset.GetType()))
                                        {
                                            var azureSqlTableDataset = (AzureSqlTableDataset)dataset;
                                            Console.WriteLine($"DatasetTable: {azureSqlTableDataset.TableName}");

                                            var tableName = azureSqlTableDataset.TableName.ToString();
                                            var jArray = (JArray)azureSqlTableDataset.Structure;
                                            var structure = jArray.ToObject<DatasetDataElement[]>();
                                            var annotation = azureSqlTableDataset.Annotations.Where(x => x.ToString().StartsWith("DS:")).FirstOrDefault().ToString();
                                            Console.WriteLine($"Annotation:   {annotation}");

                                            if (annotation != null)
                                            {
                                                var connectionName = annotation.Substring(3);
                                                var connectionString = ConfigurationManager.ConnectionStrings[connectionName].ConnectionString;
                                                Console.WriteLine($"Connection:   {connectionString}");
                                                using (var conn = new SqlConnection(connectionString))
                                                {
                                                    conn.Open();
                                                    Console.WriteLine($"Database Is Open: {conn.State}");

                                                    // You can specify the Catalog, Schema, Table Name, Table Type to get   
                                                    // the specified table.  
                                                    // You can use four restrictions for Table, so you should create a 4 members array.  
                                                    string[] tableRestrictions = new string[4];

                                                    // For the array, 0-member represents Catalog; 1-member represents Schema;   
                                                    // 2-member represents Table Name; 3-member represents Column Name.   
                                                    // Now we specify the Table Name of the table that we want to get schema information for. 
                                                    var parts = tableName.Split('.');
                                                    tableRestrictions[(int)SchemaTableColumn.TABLE_SCHEMA] = parts[0];
                                                    tableRestrictions[(int)SchemaTableColumn.TABLE_NAME] = parts[1];

                                                    DataTable tableSchema = conn.GetSchema("Columns", tableRestrictions);

                                                    var match = SchemaComparer.CompareStructures(structure, tableSchema);

                                                    if (match)
                                                    {
                                                        Console.WriteLine($"Table {tableName} matches.");
                                                    }
                                                    else
                                                    {
                                                        allTablesMatch = false;
                                                        Console.WriteLine($"ALERT! Table {tableName} DOES NOT MATCH.  Data Factory Definition and SQL Table Definition are different.");
                                                    }
                                                    conn.Close();
                                                    Console.WriteLine($"Database Is Closed: {conn.State}");
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return allTablesMatch;
        }

        private IEnumerable<Factory> FetchFactories()
        {
            return dataFactoryManagementClient.Factories.ListByResourceGroup(resourceGroup);
        }

        private IEnumerable<PipelineResource> FetchPipelines(Factory factory)
        {
            return dataFactoryManagementClient.Pipelines.ListByFactory(resourceGroup, factory.Name);
        }

        private IEnumerable<DatasetResource> FetchDatasets(Factory factory)
        {
            return dataFactoryManagementClient.Datasets.ListByFactory(resourceGroup, factory.Name);
        }

        private IEnumerable<LinkedServiceResource> FetchLinkedServices(Factory factory)
        {
            return dataFactoryManagementClient.LinkedServices.ListByFactory(resourceGroup, factory.Name);
        }
    }
}
