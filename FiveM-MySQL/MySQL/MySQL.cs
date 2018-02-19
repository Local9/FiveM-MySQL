using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GHMatti.MySQL
{
    public class MySQL
    {
        private Core.GHMattiTaskScheduler queryScheduler;
        private string connectionString;
        private bool Debug;

        public MySQL(string server, string port, string databasename, string username, string password, 
            bool debug, Core.GHMattiTaskScheduler taskScheduler)
        {
            connectionString = String.Format("SERVER={0};PORT={1};DATABASE={2};UID={3};PASSWORD={4}",
                server, port, databasename, username, password    
            );
            Debug = debug;
            queryScheduler = taskScheduler;
            // Cannot execute that connection in on the server thread, but we need to test if the connection string is actually correct
            // This will cause a hitch if the constructor is not put in a Task on a different thread
            using (Connection db = new Connection(connectionString)) { }
        }

        public Task<int> Query(string query, IDictionary<string, dynamic> parameters = null) => Task.Factory.StartNew(() => 
        {
            int result = -1;

            System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
            long connectionTime = 0, queryTime = 0;

            using(Connection db = new Connection(connectionString))
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
                    }
                }
                catch(MySqlException mysqlEx)
                {
                    PrintErrorInformation(mysqlEx);
                }
                // I don't think I want to catch the other exceptions. Just throw for now.
            }

            timer.Stop();
            PrintDebugInformation(connectionTime,queryTime,0,query);

            return result;
        }, CancellationToken.None, TaskCreationOptions.None, queryScheduler);

        public Task<dynamic> QueryScalar(string query, IDictionary<string, dynamic> parameters = null) => Task.Factory.StartNew(() => 
        {
            dynamic result = null;

            System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
            long connectionTime = 0, queryTime = 0;

            using (Connection db = new Connection(connectionString))
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


        public Task<MySQLResult> QueryResult(string query, IDictionary<string, dynamic> parameters = null) => Task.Factory.StartNew(() => 
        {
            MySQLResult result = new MySQLResult();

            System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
            long connectionTime = 0, queryTime = 0, readTime = 0;

            using (Connection db = new Connection(connectionString))
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
                                result.Add(Enumerable.Range(0, reader.FieldCount).ToDictionary(reader.GetName, reader.GetValue));
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

        private void PrintErrorInformation(MySqlException mysqlEx)
        {
            if (Debug)
                CitizenFX.Core.Debug.Write(String.Format("[GHMattiMySQL ERROR] [ERROR] {0}\n{1}", mysqlEx.Message, mysqlEx.StackTrace));
            else
                CitizenFX.Core.Debug.Write(String.Format("[GHMattiMySQL ERROR] {0}\n", mysqlEx.Message));
        }

        private void PrintDebugInformation(long ctime, long qtime, long rtime, string query)
        {
            if(Debug)
                CitizenFX.Core.Debug.WriteLine(String.Format(
                    "[MySQL Debug] Connection: {0}ms; Query: {1}ms; Read: {2}ms; Total {3}ms for Query: {4}",
                    ctime, qtime, rtime, ctime+qtime+rtime, query
                ));
        }
    }
}
