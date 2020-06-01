using GHMatti.Data.MySQL.Utilities;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Diagnostics;

namespace GHMatti.Data.MySQL.Core
{
    /// <summary>
    /// Abstract class to implement basic mysql database interactions
    /// </summary>
    /// <typeparam name="T">Return value type of the interaction</typeparam>
    internal abstract class Interaction<T>
    {
        /// <summary>
        /// Public Properties
        /// </summary>
        public string ConnectionString { get; set; }
        public bool Debug { get; set; }
        public string CommandText { get; set; }
        public IDictionary<string, dynamic> Parameters { get; set; }

        protected MySqlConnection connection;

        /// <summary>
        /// Constructor method
        /// </summary>
        /// <param name="connectionString">Connection string used to connect to the database</param>
        /// <param name="debug">Print Debug information</param>
        public Interaction(string connectionString, bool debug)
        {
            ConnectionString = connectionString;
            Debug = debug;
        }

        /// <summary>
        /// Command that should be used to run the interaction
        /// </summary>
        /// <returns>Return value of the interaction</returns>
        public T Run()
        {
            T result;

            Stopwatch timer = new Stopwatch();
            long connectionTime = 0, interactionTime = 0;

            using (Connection db = new Connection(ConnectionString))
            {
                timer.Start();
                db.connection.Open();
                connectionTime = timer.ElapsedMilliseconds;
                connection = db.connection;

                using (MySqlCommand cmd = db.connection.CreateCommand())
                {
                    timer.Restart();
                    result = Execute(cmd);
                    interactionTime = timer.ElapsedMilliseconds;
                }
            }
            timer.Stop();
            Utility.PrintDebugInformation(connectionTime, interactionTime, CommandText, Debug);

            return result;
        }

        /// <summary>
        /// Internal execution of the Interaction
        /// </summary>
        /// <param name="cmd">MySQl Command</param>
        /// <returns></returns>
        protected abstract T Execute(MySqlCommand cmd);
    }
}
