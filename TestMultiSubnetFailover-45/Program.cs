using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Net;
using System.Threading;

namespace TestMultiSubnetFailover_45
{
    class Program
    {
        static string dbHostname = ConfigurationManager.AppSettings["DatabaseHostName"];
        static int poolingSleepTime { get { return int.Parse(ConfigurationManager.AppSettings["SleepTimeForLongPooling"]); } }

        static void Main(string[] args)
        {
            FailoverTester failTest = new FailoverTester();

            Console.WriteLine("This application will run 'SELECT SERVERPROPERTY('MachineName')' against the server specified in the configuration settings.\n");

            // list IPs from the database hostname
            Console.WriteLine("Gathering list of IP addresses associated with '{0}'", dbHostname);

            foreach (IPAddress add in Dns.GetHostEntry(dbHostname).AddressList)
            {
                Console.WriteLine("   {0}", add);
            }

            Console.WriteLine("\nExecution options: ");
            Console.WriteLine("  [e] run a single test");
            Console.WriteLine("  [r] run long pooling test execution");
            Console.WriteLine("  [q] exit the application");
            Console.WriteLine();

            ConsoleKeyInfo userSel;
            do
            {
                userSel = Console.ReadKey(true);

                switch (userSel.Key)
                {
                    case ConsoleKey.E:
                        Console.WriteLine("running a single test...");
                        failTest.RunTests();
                        break;
                    case ConsoleKey.R:
                        Console.WriteLine("running long pooling tests... Press [r] to stop!");
                        LongPoolAction(failTest.RunTests, ConsoleKey.R);
                        break;
                }

            } while (userSel.Key != ConsoleKey.Q);

            Console.WriteLine("Bye!");
        }

        static void LongPoolAction(Action act, ConsoleKey stopKey)
        {
            bool runningTests = true;
            while (runningTests)
            {
                if (Console.KeyAvailable)
                {
                    ConsoleKeyInfo key = Console.ReadKey(true);
                    if (key.Key == stopKey)
                    {
                        Console.WriteLine("stopping...");
                        runningTests = false;
                    }
                }

                if (runningTests)
                {
                    // perform the requested action
                    act();
                    Thread.Sleep(poolingSleepTime);
                }
            }

        }
    }

    public class FailoverTester
    {
        static string dbConnString { get { return ConfigurationManager.ConnectionStrings["dbConnString"].ConnectionString; } }
        
        public object TestRequest()
        {
            // get machine name from SQL Server
            using (SqlConnection sqlConn = new SqlConnection(dbConnString))
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
                Stopwatch sw = new Stopwatch();
                sw.Start();

                object result = TestRequest();

                sw.Stop();

                Console.WriteLine(">> '{0}'; elapsed: '{1}' seconds", result, sw.Elapsed.TotalSeconds);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
