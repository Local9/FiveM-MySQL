using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GHMatti.MySQL
{
    // MySQL Wrapper Class using a custom task scheduler
    public class MySQL
    {
        // This is where we store the TaskScheduler
        private Core.GHMattiTaskScheduler queryScheduler;
        // This contains the settings needed for this wrapper
        private MySQLSettings settings;

        // Constructor, should be called in the task scheduler itself to avoid hitches
        public MySQL(MySQLSettings mysqlSettings, Core.GHMattiTaskScheduler taskScheduler)
        {
            settings = mysqlSettings;
            settings.Apply();
            queryScheduler = taskScheduler;
            // Cannot execute that connection in on the server thread, but we need to test if the connection string is actually correct
            // This will cause a hitch if the constructor is not put in a Task on a different thread
            using (Connection db = new Connection(settings.ConnectionString)) { }
        }

        // This is the ExecuteNonQuery command wrapper
        public Task<long> Query(string query, IDictionary<string, dynamic> parameters = null, bool isInsert = false) => Task.Factory.StartNew(() =>
        {
            long result = -1;

            System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
            long connectionTime = 0, queryTime = 0;

            using (Connection db = new Connection(settings.ConnectionString))
            {
                timer.Start();
                db.connection.Open();
                connectionTime = timer.ElapsedMilliseconds;

                try
                {
                    using (MySqlCommand cmd = db.connection.CreateCommand())
                    {
                        cmd.CommandText = query;
                        cmd.AddParameters(parameters);

                        timer.Restart();
                        result = cmd.ExecuteNonQuery();
                        queryTime = timer.ElapsedMilliseconds;

                        if (isInsert)
                            result = cmd.LastInsertedId;

                        if (settings.Debug)
                            query = cmd.Stringify();
                    }
                }
                catch (MySqlException mysqlEx)
                {
                    PrintErrorInformation(mysqlEx);
                }
                // I don't think I want to catch the other exceptions. Just throw for now.
            }

            timer.Stop();
            PrintDebugInformation(connectionTime, queryTime, 0, query);

            return result;
        }, CancellationToken.None, TaskCreationOptions.None, queryScheduler);

        // This is the ExecuteScalar wrapper
        public Task<object> QueryScalar(string query, IDictionary<string, dynamic> parameters = null) => Task.Factory.StartNew(() =>
        {
            object result = null;

            System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
            long connectionTime = 0, queryTime = 0;

            using (Connection db = new Connection(settings.ConnectionString))
            {
                timer.Start();
                db.connection.Open();
                connectionTime = timer.ElapsedMilliseconds;

                using (MySqlCommand cmd = db.connection.CreateCommand())
                {
                    cmd.CommandText = query;
                    cmd.AddParameters(parameters);

                    try
                    {
                        timer.Restart();
                        result = cmd.ExecuteScalar();
                        queryTime = timer.ElapsedMilliseconds;

                        if (settings.Debug)
                            query = cmd.Stringify();
                    }
                    catch (MySqlException mysqlEx)
                    {
                        PrintErrorInformation(mysqlEx);
                    }
                    // I don't think I want to catch the other exceptions. Just throw for now.
                }
            }

            timer.Stop();
            PrintDebugInformation(connectionTime, queryTime, 0, query);

            return result;
        }, CancellationToken.None, TaskCreationOptions.None, queryScheduler);

        // This is the actual query wrapper where you read from the database more than a singular value
        public Task<MySQLResult> QueryResult(string query, IDictionary<string, dynamic> parameters = null) => Task.Factory.StartNew(() =>
        {
            MySQLResult result = new MySQLResult();

            System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
            long connectionTime = 0, queryTime = 0, readTime = 0;

            using (Connection db = new Connection(settings.ConnectionString))
            {
                timer.Start();
                db.connection.Open();
                connectionTime = timer.ElapsedMilliseconds;

                using (MySqlCommand cmd = db.connection.CreateCommand())
                {
                    cmd.CommandText = query;
                    cmd.AddParameters(parameters);

                    try
                    {
                        timer.Restart();
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            queryTime = timer.ElapsedMilliseconds;
                            timer.Restart();
                            while (reader.Read())
                                result.Add(Enumerable.Range(0, reader.FieldCount).ToDictionary(reader.GetName,
                                    i => (reader.IsDBNull(i)) ? null : reader.GetValue(i)));

                            if (settings.Debug)
                                query = cmd.Stringify();
                        }
                        readTime = timer.ElapsedMilliseconds;
                    }
                    catch (MySqlException mysqlEx)
                    {
                        PrintErrorInformation(mysqlEx);
                    }
                    // I don't think I want to catch the other exceptions. Just throw for now.
                }
            }

            timer.Stop();
            PrintDebugInformation(connectionTime, queryTime, readTime, query);

            return result;
        }, CancellationToken.None, TaskCreationOptions.None, queryScheduler);

        // wrapper for transactions
        public Task<bool> Transaction(IList<string> querys, IDictionary<string, dynamic> parameters = null) => Task.Factory.StartNew(() =>
        {
            bool result = false;

            System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
            long connectionTime = 0, queryTime = 0;

            using (Connection db = new Connection(settings.ConnectionString))
            {
                timer.Start();
                db.connection.Open();
                connectionTime = timer.ElapsedMilliseconds;

                using (MySqlCommand cmd = db.connection.CreateCommand())
                {
                    MySqlTransaction transaction = db.connection.BeginTransaction();
                    cmd.AddParameters(parameters);
                    cmd.Transaction = transaction;

                    timer.Restart();

                    try
                    {
                        foreach (string query in querys)
                        {
                            cmd.CommandText = query;
                            cmd.ExecuteNonQuery();
                        }
                        transaction.Commit();
                    }
                    catch (Exception exception)
                    {
                        if (settings.Debug)
                            CitizenFX.Core.Debug.Write(String.Format("[GHMattiMySQL WARNING] [Failed Transaction] {0}\n{1}\n", exception.Message, exception.StackTrace));
                        else
                            CitizenFX.Core.Debug.Write(String.Format("[GHMattiMySQL WARNING] [Failed Transaction] {0}\n", exception.Message));
                        // Don't try it, throw on failure
                        transaction.Rollback();
                    }

                    queryTime = timer.ElapsedMilliseconds;
                }
            }

            timer.Stop();
            PrintDebugInformation(connectionTime, queryTime, 0, "Transaction");

            return result;
        }, CancellationToken.None, TaskCreationOptions.None, queryScheduler);

        // Helper function to display MySQL error information
        private void PrintErrorInformation(MySqlException mysqlEx)
        {
            if (settings.Debug)
                CitizenFX.Core.Debug.Write(String.Format("[GHMattiMySQL ERROR] [ERROR] {0}\n{1}\n", mysqlEx.Message, mysqlEx.StackTrace));
            else
                CitizenFX.Core.Debug.Write(String.Format("[GHMattiMySQL ERROR] {0}\n", mysqlEx.Message));
        }

        // Helper function to display MySQL client<->server performance
        private void PrintDebugInformation(long ctime, long qtime, long rtime, string query)
        {
            if (settings.Debug)
                CitizenFX.Core.Debug.Write(String.Format(
                    "[MySQL Debug] Connection: {0}ms; Query: {1}ms; Read: {2}ms; Total {3}ms for Query: {4}\n",
                    ctime, qtime, rtime, ctime + qtime + rtime, query
                ));
        }
    }
}
