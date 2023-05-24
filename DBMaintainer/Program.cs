using System;
using System.Data.SqlClient;
using System.IO;
using System.Linq;

namespace DBMaintainer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            int statementsExecuted = 0;

            try
            {
                // Get the current directory (where the ScriptFile.txt file should reside):
                string curDirectory = Directory.GetCurrentDirectory();

                // Read the text file containing the connection string and SQL statements
                string filePath = Path.Combine(curDirectory, "ScriptFile.txt");
                string scriptContents = File.ReadAllText(filePath);

                // Split the script contents by semicolon to separate individual SQL statements
                string[] statements = scriptContents.Split(';');

                // Get the server name and DB name from the first 2 lines of the script file
                string servName = statements[0].Trim();
                string dbName = statements[1].Trim();  
                string connectionString = MakeSQLConnectionString(servName, dbName);

                // Remove the connection string from the statements array
                string[] sqlStatements = new string[statements.Length - 2];
                Array.Copy(statements, 2, sqlStatements, 0, sqlStatements.Length);

                // Connect to the database and execute the SQL statements
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    foreach (string sqlStatement in sqlStatements)
                    {
                        if (!string.IsNullOrWhiteSpace(sqlStatement))
                        {
                            using (SqlCommand command = new SqlCommand(sqlStatement, connection))
                            {
                                command.ExecuteNonQuery();
                                statementsExecuted++;
                            }
                        }
                    }
                }

                Console.WriteLine($"Database Maintenance complete. Executed {statementsExecuted} statements.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }

            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
        }

        private static string MakeSQLConnectionString(string serverName, string dbName)
        {
            string theConnStr = "";

            // Server and DB must be specified:
            if (!string.IsNullOrEmpty(serverName) && !string.IsNullOrEmpty(dbName))
            {
                // Initialize the connection string builder for the
                // underlying provider:
                SqlConnectionStringBuilder sqlBuilder = new SqlConnectionStringBuilder();

                // Set the properties for the data source:
                sqlBuilder.DataSource = serverName;
                sqlBuilder.InitialCatalog = dbName;
                sqlBuilder.IntegratedSecurity = true;

                theConnStr = sqlBuilder.ToString();
            }

            return theConnStr;
        }
    }


}
