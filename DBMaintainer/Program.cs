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
            try
            {
                // Get the current directory (where the ScriptFile.txt file should reside):
                string curDirectory = Directory.GetCurrentDirectory();

                // Read the text file containing the connection string and SQL statements
                string filePath = Path.Combine(curDirectory, "ScriptFile.txt");
                string scriptContents = File.ReadAllText(filePath);

                // Split the script contents by semicolon to separate individual SQL statements
                string[] statements = scriptContents.Split(';');

                // Get the connection string from the first line of the script file
                string connectionString = statements[0].Trim();

                // Remove the connection string from the statements array
                string[] sqlStatements = new string[statements.Length - 1];
                Array.Copy(statements, 1, sqlStatements, 0, sqlStatements.Length);

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
                            }
                        }
                    }
                }

                Console.WriteLine($"Database Maintenance complete. Executed {sqlStatements.Count()} statements.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }

            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
        }
    }
}
