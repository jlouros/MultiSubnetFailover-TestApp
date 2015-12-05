using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Net;
using System.Threading;

namespace TestMultiSubnetFailover_461
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("This application will run 'SELECT SERVERPROPERTY('MachineName')' against the server specified in the configuration settings.\n");

            // list hosts
            string dbFqdn = ConfigurationManager.AppSettings["DatabaseFullyQualifiedDomainName"];
            Console.WriteLine("Gathering list of IP addresses associated with '{0}'", dbFqdn);

            foreach (IPAddress add in Dns.GetHostEntry(dbFqdn).AddressList)
            {
                Console.WriteLine("   {0}", add);
            }

            Console.WriteLine("\nExecution options: ");
            Console.WriteLine("  [e] runs a single test");
            Console.WriteLine("  [r] runs long pooling test execution, press [Ctrl] + [c] to exit");
            Console.WriteLine("  [q] exit the application");
            Console.WriteLine();

            ConsoleKeyInfo userSel;
            do
            {
                userSel = Console.ReadKey(true);

                FailoverTester failTest = new FailoverTester();

                if (userSel.Key == ConsoleKey.E)
                {
                    failTest.RunTests();
                }

                if (userSel.Key == ConsoleKey.R)
                {
                    // never ending loop, use [Ctrl] + [c] to terminate the program
                    while (true)
                    {
                        failTest.RunTests();
                    }
                }

            } while (userSel.Key != ConsoleKey.Q);


        }
    }

    public class FailoverTester
    {
        static string motorboat { get { return ConfigurationManager.ConnectionStrings["motorboat"].ConnectionString; } }
        static int poolingSleepTime { get { return int.Parse(ConfigurationManager.AppSettings["SleepTimeForLongPooling"]); } }

        public object TestRequest()
        {
            // get machine name from SQL Server
            using (SqlConnection sqlConn = new SqlConnection(motorboat))
            using (SqlCommand sqlCmd = new SqlCommand("SELECT SERVERPROPERTY('MachineName')", sqlConn))
            {
                sqlConn.Open();

                return sqlCmd.ExecuteScalar();
            }
        }

        public void RunTests()
        {
            try
            {
                Console.WriteLine(">> '{0}'", TestRequest());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            Thread.Sleep(poolingSleepTime);
        }
    }
}
