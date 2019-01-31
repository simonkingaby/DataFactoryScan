# Data Factory Scanner
This C# solution connects to Data Factory and compares the Table structures defined in the Copy Activity to the Table Definition in the Database.  Reporting on the differences.

#### Note the Use of Annotations in Data Factory
> Note:
There is one "trick" in this solution that is not obvious from the code.  In order to find the connection string to the right database, I have added an 'Annotation' to the Data Factory Dataset definition with the name of the connection string in it.  For example, in the JSON definition of the Source Dataset, you will see:

```JSON
"annotations": [
    "DS:DFSDB"
]
```

> Which corresponds to this entry in the Azure KeyVault:

    SecretName: "DFSDB" 
    Secret: "Server=tcp:datafactoryscanserver.database.windows.net,1433;Initial Catalog=DataFactoryScanDB;Persist Security Info=False;User ID=ETLUser;Password=***;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=0;"





