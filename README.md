# DataFactoryScanner
This C# solution connects to Data Factory and compares the Table structures defined in the Copy Activity to the Table Definition in the Database.  Reporting on the differences.

There is one "trick" in this solution that is not obvious from the code.  In order to find the connection string to the right database, I have added an 'Annotation' to the Data Factory Dataset definition with the name of the connection string in it.  For example, in the JSON definition of the Source Dataset, you will see:

"annotations": [
            "DS:DFSDB"
        ]

Which corresponds to this entry in the app.config file:

<add name="DFSDB" connectionString="Server=...



