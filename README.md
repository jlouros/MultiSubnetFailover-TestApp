# Multi-subnet-failover test applications

Applications to test [System.Data].SqlClient 'MultiSubnetFailover'.
There's .Net 4.5 and .Net 4.6.1 versions.
The code is exactly the same, just targeting different .Net versions.
Modify 'App.config' with settings that match your environment.
'App.Config' contains the following settings:
 - 'DatabaseHostName': DNS entry targeting your database environment. Should contain at least 2 associated IPs
 - 'SleepTimeForLongPooling': time the application will wait between SQL requests on a long pooling test (in milliseconds)
 - 'dbConnString' (connection string): for .Net 4.5 'MultisubnetFailover=true' must be set. On .Net 4.6.1 that setting is not required
 
Additionally, under the scripts folder you can find a PowerShell script that toggles SQL Server service state for the provided database host name.
 
You can read more about this project at http://johnlouros.com/blog/leveraging-multi-subnet-failover
