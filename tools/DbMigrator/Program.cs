using DbUp;
using System.Reflection;
using System.Text;

namespace DbMigrator
{
    internal class Program
    {
        static int Main(string[] args)
        {
            string? connectionString = args.FirstOrDefault();

            try
            {
                if (string.IsNullOrWhiteSpace(connectionString))
                    throw new Exception("ConnectionString empty");

                bool verifyOnly = args.Length > 1 && args[1] == "--verify";

                return DatabaseMigrator.MigrateDatabase(connectionString, verifyOnly);
            }
            catch (Exception)
            {
                throw new Exception("Hej");
            }
            
        }
    }

    public class DatabaseMigrator
    {
        public static int MigrateDatabase(string connectionString, bool verifyOnly = false)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new Exception("ConnectionString empty");

            DbUp.Engine.UpgradeEngine? upgrader;
            DbUp.Engine.DatabaseUpgradeResult? result;

            if (verifyOnly)
            {
                upgrader =
                DeployChanges.To
                             .PostgresqlDatabase(connectionString)
                             .WithTransactionAlwaysRollback()
                             .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly(), encoding: Encoding.UTF8)
                             .LogToConsole()
                             .Build();
            }
            else
            {
                upgrader =
                DeployChanges.To
                             .PostgresqlDatabase(connectionString)
                             .WithTransactionPerScript()
                             .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly(), encoding: Encoding.UTF8)
                             .LogToConsole()
                             .Build();
            }

            try
            {
                result = upgrader.PerformUpgrade();
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(e);
                Console.ResetColor();
                return -1;
            }

            if (result is not null && !result.Successful)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(result.Error);
                Console.ResetColor();
                return -1;
            }

            Console.ForegroundColor = ConsoleColor.Green;
            if (verifyOnly)
                Console.WriteLine("Verification successful - all migrations can be applied!");
            else
                Console.WriteLine("Áll migrations applied!");

            Console.ResetColor();

            return 0;
        }
    }}
