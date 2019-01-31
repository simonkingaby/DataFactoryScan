using Microsoft.Azure.Management.DataFactory.Models;
using System;
using System.Data;

namespace DataFactoryScan
{
    class SchemaComparer
    {
        internal static bool CompareStructures(DatasetDataElement[] structure, DataTable tableSchema)
        {
            var match = true;
            var columnCount = 0;
            foreach (var dataElement in structure)
            {
                var colName = dataElement.Name.ToString();
                var colType = dataElement.Type.ToString();

                var foundColumn = false;
                foreach (DataRow row in tableSchema.Rows)
                {
                    if (row[(int)SchemaTableColumn.COLUMN_NAME].ToString() == colName)
                    {
                        foundColumn = true;
                        columnCount++;
                        var column = row[(int)SchemaTableColumn.COLUMN_NAME].ToString();
                        var columnType = row[(int)SchemaTableColumn.DATA_TYPE].ToString();

                        var colTypeMatch = DoDataTypesMatch(colType, columnType, row);

                        Console.WriteLine($"Column: {colName}  DataType in Data factory: {colType}  DataType in Table: {columnType}  Match: {colTypeMatch} ");

                        if (!colTypeMatch)
                        {
                            match = false;
                        }

                        break;
                    }
                }
                if (!foundColumn)
                {
                    Console.WriteLine($"{colName} column is in DataFactory but does not exist in the table.");
                    match = false;
                }
            }

            //make sure we accounted for all the columns in the Table
            if (columnCount < tableSchema.Rows.Count)
            {
                Console.WriteLine($"There are more columns in table {tableSchema.TableName} than in the DataFactory job.");
                match = false;
            }

            return match;
        }

        private static bool DoDataTypesMatch(string colType, string columnType, DataRow row)
        {
            if (colType == columnType)
                return true;

            switch (columnType)
            {
                case "char":
                case "nchar":
                case "varchar":
                case "nvarchar":
                    if (colType == "String") return true;
                    break;

                case "int":
                    if (colType == "Int32") return true;
                    break;

                //TODO:  Handle other SQL data types

                default:
                    return false;
            }

            return false;
        }
    }
}
