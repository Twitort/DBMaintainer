using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;

namespace DBMaintainer
{
    internal class Program
    {
        // Pompts for any needed inputs from user:
        private static List<string> paramPrompts = new List<string>();

        private static string completeMsg;

        private static List<string> paramInputs = new List<string>();

        private static string connectionString;
        private static string[] sqlStatements;

        static void Main(string[] args)
        {
            int statementsExecuted = 0;

            try
            {
                // Get the current directory (where the ScriptFile.txt file should reside):
                string curDirectory = Directory.GetCurrentDirectory();

                // Read the text file containing the connection string and SQL statements
                string filePath = Path.Combine(curDirectory, "ScriptFile.txt");

                // If the script file is usable:
                if (ParseScriptFile(Path.Combine(curDirectory, "ScriptFile.txt")))
                {
                    // Prompt for update parameters, if any:
                    GetUserParams();

                    // Modify the SQL statements with the paramters as needed:
                    ApplyUserParams(sqlStatements);

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

                    if (!string.IsNullOrEmpty(completeMsg))
                        Console.WriteLine(completeMsg);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }

            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
        }

        private static bool ParseScriptFile(string filePath)
        {
            bool result = false;
            int commandStartLine = 2;

            try 
            {
                // Read in the file:
                string scriptContents = File.ReadAllText(filePath);
                string[] statements = scriptContents.Split(';');

                // Pull out the server and DB info to make the connection string:
                string servName = statements[0].Trim();
                string dbName = statements[1].Trim();
                connectionString = MakeSQLConnectionString(servName, dbName);

                // Pull out needed parameter prompts:
                while (statements[commandStartLine].Trim().StartsWith("PARAM:"))
                {
                    paramPrompts.Add(statements[commandStartLine].Trim().Substring(6).Trim());
                    commandStartLine++;
                }

                // Pull out the success message:
                if (statements[commandStartLine].Trim().StartsWith("MSG:"))
                {
                    completeMsg = statements[commandStartLine].Trim().Substring(4).Trim();
                    commandStartLine++;
                }

                // Pull out the SQL commands:
                sqlStatements = new string[statements.Length - commandStartLine];
                Array.Copy(statements, commandStartLine, sqlStatements, 0, sqlStatements.Length);

                result = true;
            }
            catch (Exception ex) 
            {
                Console.WriteLine("Could not parse the script file: " + ex.Message);
            }

            return result;
        }

        private static void GetUserParams()
        {
            // Must have prompts to show. If no prompts, then there must not be a need
            // for user-entered parameters:
            if (paramPrompts != null && paramPrompts.Count > 0)
            {
                // Display each prompt and load the user's input into the param list:
                foreach(string prompt in paramPrompts)
                {
                    if (!string.IsNullOrWhiteSpace(prompt))
                    {
                        Console.Write($"{prompt} ");
                        paramInputs.Add(Console.ReadLine());
                    }
                }
            }
        }

        private static void ApplyUserParams(string[] sqlStatements)
        {
            // If there are user params to apply:
            if (paramInputs != null && paramInputs.Count > 0)
            {
                // If there are database update commands:
                if (sqlStatements != null && sqlStatements.Length > 0)
                {
                    for (int stmtIndex = 0; stmtIndex < sqlStatements.Length; stmtIndex++)
                    {
                        // Replace each param placeholder found in this statement with the
                        // corresponding user input:
                        for (int i = 0; i < paramInputs.Count; i++)
                        {
                            sqlStatements[stmtIndex] = sqlStatements[stmtIndex].Replace("{"+i+"}", paramInputs[i]);
                        }
                    }
                }
            }
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
