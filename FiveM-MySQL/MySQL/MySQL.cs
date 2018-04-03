using GHMatti.Core;
using GHMatti.MySQL.Core;
using System.Collections.Generic;
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
        private GHMattiTaskScheduler queryScheduler;
        /// <summary>
        /// This contains the settings needed for this wrapper
        /// </summary>
        private Settings settings;

        /// <summary>
        /// Constructor, should be called in the task scheduler itself to avoid hitches
        /// </summary>
        /// <param name="mysqlSettings"></param>
        /// <param name="taskScheduler"></param>
        public MySQL(Settings mysqlSettings, GHMattiTaskScheduler taskScheduler)
        {
            settings = mysqlSettings;
            settings.Apply();
            queryScheduler = taskScheduler;
            // Test the Database Connection
            Query("SELECT 1;");
        }

        /// <summary>
        /// This is the ExecuteNonQuery command wrapper
        /// </summary>
        /// <param name="query">Query string</param>
        /// <param name="parameters">Parameters in dictionary form</param>
        /// <param name="isInsert">If true, then the return value will be the last inserted id</param>
        /// <returns>rows affected</returns>
        public Task<long> Query(string query, IDictionary<string, dynamic> parameters = null, bool isInsert = false)
        {
            return Task.Factory.StartNew(() =>
            {
                NonQuery nonQuery = new NonQuery(settings.ConnectionString, settings.Debug);
                nonQuery.CommandText = query;
                nonQuery.Parameters = parameters;
                nonQuery.IsInsert = isInsert;
                return nonQuery.Run();
            }, CancellationToken.None, TaskCreationOptions.None, queryScheduler);
        }

        /// <summary>
        /// This is the ExecuteScalar wrapper
        /// </summary>
        /// <param name="query">Query string</param>
        /// <param name="parameters">Parameters in dictionary form</param>
        /// <returns>A singular value selected, like SELECT 1; => 1</returns>
        public Task<object> QueryScalar(string query, IDictionary<string, dynamic> parameters = null)
        {
            return Task.Factory.StartNew(() =>
            {
                Scalar scalar = new Scalar(settings.ConnectionString, settings.Debug);
                scalar.CommandText = query;
                scalar.Parameters = parameters;
                return scalar.Run();
            }, CancellationToken.None, TaskCreationOptions.None, queryScheduler);
        }

        /// <summary>
        /// This is the actual query wrapper where you read from the database more than a singular value
        /// </summary>
        /// <param name="query">Query string</param>
        /// <param name="parameters">Parameters in dictionary form</param>
        /// <returns>Result of the Query, List of rows containing dictionarys representing each row</returns>
        public Task<ResultSet> QueryResult(string query, IDictionary<string, dynamic> parameters = null)
        {
            return Task.Factory.StartNew(() =>
            {
                Reader reader = new Reader(settings.ConnectionString, settings.Debug);
                reader.CommandText = query;
                reader.Parameters = parameters;
                return reader.Run();
            }, CancellationToken.None, TaskCreationOptions.None, queryScheduler);
        }

        /// <summary>
        /// wrapper for transactions
        /// </summary>
        /// <param name="querys">List of query strings</param>
        /// <param name="parameters">Dictionary of parameters which count for all transactions</param>
        /// <returns>true or false depending on whether the transaction succeeded or not</returns>
        public Task<bool> Transaction(IList<string> querys, IDictionary<string, dynamic> parameters = null)
        {
            return Task.Factory.StartNew(() =>
            {
                Transaction transaction = new Transaction(settings.ConnectionString, settings.Debug);
                transaction.Commands = querys;
                transaction.Parameters = parameters;
                return transaction.Run();
            }, CancellationToken.None, TaskCreationOptions.None, queryScheduler);
        }
    }
}
