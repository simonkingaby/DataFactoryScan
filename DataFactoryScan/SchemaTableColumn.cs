namespace DataFactoryScan
{
    //https://docs.microsoft.com/en-us/dotnet/framework/data/adonet/sql-server-schema-collections#columns
    internal enum SchemaTableColumn
    {
        TABLE_CATALOG,
        TABLE_SCHEMA,
        TABLE_NAME,
        COLUMN_NAME,
        ORDINAL_POSITION,
        COLUMN_DEFAULT,
        IS_NULLABLE,
        DATA_TYPE,
        CHARACTER_MAX_LENGTH,
        CHARACTER_OCTET_LENGTH,
        NUMERIC_PRECISION,
        NUMERIC_PRECISION_RADIX,
        NUMERIC_SCALE,
        DATETIME_PRECISION
        //There's a few more, but we don't need them
    }
}
