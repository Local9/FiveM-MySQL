using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GHMatti.MySQL
{
    /// <summary>
    /// MySQL Wrapper Class using a custom task scheduler
    /// </summary>
    public class MySQL
    {
        /// <summary>
        /// This is where we store the TaskScheduler
        /// </summary>
        private Core.GHMattiTaskScheduler queryScheduler;
        /// <summary>
        /// This contains the settings needed for this wrapper
        /// </summary>
        private MySQLSettings settings;

        /// <summary>
        /// Constructor, should be called in the task scheduler itself to avoid hitches
        /// </summary>
        /// <param name="mysqlSettings"></param>
        /// <param name="taskScheduler"></param>
        public MySQL(MySQLSettings mysqlSettings, Core.GHMattiTaskScheduler taskScheduler)
        {
            settings = mysqlSettings;
            settings.Apply();
            queryScheduler = taskScheduler;
            // Cannot execute that connection in on the server thread, but we need to test if the connection string is actually correct
            // This will cause a hitch if the constructor is not put in a Task on a different thread
            using (Connection db = new Connection(settings.ConnectionString)) { }
        }

        /// <summary>
        /// This is the ExecuteNonQuery command wrapper
        /// </summary>
        /// <param name="query">Query string</param>
        /// <param name="parameters">Parameters in dictionary form</param>
        /// <param name="isInsert">If true, then the return value will be the last inserted id</param>
        /// <returns>rows affected</returns>
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

        /// <summary>
        /// This is the ExecuteScalar wrapper
        /// </summary>
        /// <param name="query">Query string</param>
        /// <param name="parameters">Parameters in dictionary form</param>
        /// <returns>A singular value selected, like SELECT 1; => 1</returns>
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
                        if (result != null && result.GetType() == typeof(DBNull))
                            result = null;

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

        /// <summary>
        /// This is the actual query wrapper where you read from the database more than a singular value
        /// </summary>
        /// <param name="query">Query string</param>
        /// <param name="parameters">Parameters in dictionary form</param>
        /// <returns>Result of the Query, List of rows containing dictionarys representing each row</returns>
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

        /// <summary>
        /// wrapper for transactions
        /// </summary>
        /// <param name="querys">List of query strings</param>
        /// <param name="parameters">Dictionary of parameters which count for all transactions</param>
        /// <returns>true or false depending on whether the transaction succeeded or not</returns>
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
                    using (MySqlTransaction transaction = db.connection.BeginTransaction())
                    {
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
                            result = true;
                        }
                        catch (Exception exception)
                        {
                            if (settings.Debug)
                                CitizenFX.Core.Debug.Write(String.Format("[GHMattiMySQL] [Failed Transaction] {0}\n{1}\n", exception.Message, exception.StackTrace));
                            else
                                CitizenFX.Core.Debug.Write(String.Format("[GHMattiMySQL] [Failed Transaction] {0}\n", exception.Message));
                            // Don't try it, throw on failure
                            transaction.Rollback();
                        }

                    }
                    queryTime = timer.ElapsedMilliseconds;
                }
            }

            timer.Stop();
            PrintDebugInformation(connectionTime, queryTime, 0, "Transaction");

            return result;
        }, CancellationToken.None, TaskCreationOptions.None, queryScheduler);

        /// <summary>
        /// Helper function to display MySQL error information
        /// </summary>
        /// <param name="mysqlEx">The MySqlException thrown</param>
        private void PrintErrorInformation(MySqlException mysqlEx)
        {
            if (settings.Debug)
                CitizenFX.Core.Debug.Write(String.Format("[GHMattiMySQL ERROR] [ERROR] {0}\n{1}\n", mysqlEx.Message, mysqlEx.StackTrace));
            else
                CitizenFX.Core.Debug.Write(String.Format("[GHMattiMySQL ERROR] {0}\n", mysqlEx.Message));
        }

        /// <summary>
        /// Helper function to display MySQL client<->server performance
        /// </summary>
        /// <param name="ctime">Connection time</param>
        /// <param name="qtime">Query time</param>
        /// <param name="rtime">Read time</param>
        /// <param name="query">MySqlCommand text</param>
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
