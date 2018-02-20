using MySql.Data.MySqlClient;
using System;

namespace GHMatti.MySQL
{
    // Connection Managing class, so we do not have to check if we actually close connections
    public class Connection : IDisposable
    {
        // Connection variable
        public readonly MySqlConnection connection;

        // Constructor to initialize the connection variable
        public Connection(string connectionString)
        {
            connection = new MySqlConnection(connectionString);
        }

        // IDisposable call
        public void Dispose()
        {
            connection.Close();
        }
    }
}
