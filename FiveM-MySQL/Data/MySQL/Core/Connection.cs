using MySql.Data.MySqlClient;
using System;

namespace GHMatti.Data.MySQL.Core
{
    /// <summary>
    /// Connection Managing class, so we do not have to check if we actually close connections
    /// </summary>
    public class Connection : IDisposable
    {
        /// <summary>
        /// Connection variable
        /// </summary>
        public readonly MySqlConnection connection;

        /// <summary>
        /// Constructor to initialize the connection variable
        /// </summary>
        /// <param name="connectionString">sets the connectionstring via which the connection happens</param>
        public Connection(string connectionString)
        {
            connection = new MySqlConnection(connectionString);
        }

        /// <summary>
        /// IDisposable call
        /// </summary>
        public void Dispose()
        {
            connection.Close();
        }
    }
}
