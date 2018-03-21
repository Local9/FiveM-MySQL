using GHMatti.MySQL.Utilities;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Diagnostics;

namespace GHMatti.MySQL.Core
{
    internal abstract class Interaction<T>
    {
        public string ConnectionString { get; set; }
        public bool Debug { get; set; }
        public string CommandText { get; set; }
        public IDictionary<string, dynamic> Parameters { get; set; }

        public Interaction(string connectionString, bool debug) 
        {
            ConnectionString = connectionString;
            Debug = debug;
        }

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

        protected abstract T Execute(MySqlCommand cmd);

    }
}
