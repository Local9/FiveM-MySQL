using CitizenFX.Core;
using CitizenFX.Core.Native;
using GHMatti.Data.MySQL;
using GHMatti.Data.MySQL.Core;
using GHMatti.Data.MySQL.Utilities;
using GHMatti.Utilities;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace GHMattiMySQL
{
    /// <summary>
    /// BaseScript class as a script for FiveM / CitizenFX that gets called
    /// </summary>
    public class Core : BaseScript
    {
        /// <summary>
        /// Private TaskScheduler to not execute on the main thread
        /// </summary>
        private GHMattiTaskScheduler taskScheduler;
        private MySQL mysql;
        private Settings settings;
        private bool initialized;

        /// <summary>
        /// Constructor to set Exports and CitizenFX.Core Handlers
        /// </summary>
        public Core()
        {
            taskScheduler = new GHMattiTaskScheduler();
            settings = new Settings();
            initialized = false;
            EventHandlers["onServerResourceStart"] += new Action<string>(Initialization);

            Exports.Add("Query", new Func<string, dynamic, Task<long>>(
                (query, parameters) => Query(query, parameters))
            );
            Exports.Add("QueryResult", new Func<string, dynamic, Task<ResultSet>>(
                (query, parameters) => QueryResult(query, parameters))
            );
            Exports.Add("QueryScalar", new Func<string, dynamic, Task<object>>(
                (query, parameters) => QueryScalar(query, parameters))
            );
            Exports.Add("Transaction", new Func<dynamic, dynamic, Task<bool>>(
                (querys, parameters) => Transaction(querys, parameters))
            );

            Exports.Add("QueryAsync", new Action<string, dynamic, CallbackDelegate>(
                (query, parameters, cb) => QueryAsync(query, parameters, cb))
            );
            Exports.Add("QueryResultAsync", new Action<string, dynamic, CallbackDelegate>(
                (query, parameters, cb) => QueryResultAsync(query, parameters, cb))
            );
            Exports.Add("QueryScalarAsync", new Action<string, dynamic, CallbackDelegate>(
                (query, parameters, cb) => QueryScalarAsync(query, parameters, cb))
            );
            Exports.Add("TransactionAsync", new Action<dynamic, dynamic, CallbackDelegate>(
                (querys, parameters, cb) => TransactionAsync(querys, parameters, cb))
            );

            Exports.Add("Insert", new Action<string, dynamic, CallbackDelegate, bool>(
                (table, parameters, cb, lastinsertid) => Insert(table, parameters, cb, lastinsertid))
            );
        }

        /// <summary>
        /// Initialization function. Nothing will execute before this is not done. Maybe remove the async and await?
        /// </summary>
        /// <param name="resourcename">Gets autoset to the resource that is started</param>
        private void Initialization(string resourcename)
        {
            if (API.GetCurrentResourceName() == resourcename)
            {
                settings.ConvarConnectionString = API.GetConvar("mysql_connection_string", "");
                settings.ConvarDebug = API.GetConvar("mysql_debug", "false");
                taskScheduler.ThreadLimit = API.GetConvarInt("mysql_thread_limit", 0);
                // You cannot do API Calls in these Threads, you need to do them before or inbetween. Use them only for heavy duty work,
                // (file operations, database interaction or transformation of data), or when working with an external library.
                Task.Factory.StartNew(() =>
                {
                    XDocument xDocument = XDocument.Load(Path.Combine("resources", resourcename, "settings.xml"));
                    settings.XMLConfiguration = xDocument.Descendants("setting").ToDictionary(
                        setting => setting.Attribute("key").Value,
                        setting => setting.Value
                    );
                    settings.Apply();
                    mysql = new MySQL(settings, taskScheduler);

                    initialized = true;
                }, CancellationToken.None, TaskCreationOptions.None, taskScheduler);
            }
        }

        /// <summary>
        /// Implementation of the standard Execute for a Command with a proper reply (rows changed?); so that lua waits for it to complete
        /// </summary>
        /// <param name="query">The mysql database query string</param>
        /// <param name="parameters">Ideally an IDictionary or table of parameters, can be null, will be parsed</param>
        /// <returns>Returns the number of affected rows</returns>
        private async Task<long> Query(string query, dynamic parameters)
        {
            await Initialized();
            return await mysql.Query(query, Utility.TryParseParameters(parameters));
        }

        /// <summary>
        /// Implementation for the standard Query / Result for a command.
        /// </summary>
        /// <param name="query">The mysql database query string</param>
        /// <param name="parameters">Ideally an IDictionary or table of parameters, can be null, will be parsed</param>
        /// <returns>The result table that was queried</returns>
        private async Task<ResultSet> QueryResult(string query, dynamic parameters)
        {
            await Initialized();
            return await mysql.QueryResult(query, Utility.TryParseParameters(parameters));
        }

        /// <summary>
        /// Implementation for the standard Scalar command, which only returns a singular value
        /// </summary>
        /// <param name="query">The mysql database query string</param>
        /// <param name="parameters">Ideally an IDictionary or table of parameters, can be null, will be parsed</param>
        /// <returns>A singular value that was queried</returns>
        private async Task<dynamic> QueryScalar(string query, dynamic parameters)
        {
            await Initialized();
            return await mysql.QueryScalar(query, Utility.TryParseParameters(parameters));
        }

        /// <summary>
        /// Async Implementation of the Execute command. This is way faster than using the Query method
        /// </summary>
        /// <param name="query">The mysql database query string</param>
        /// <param name="parameters">Ideally an IDictionary or table of parameters, can be null, will be parsed</param>
        /// <param name="callback">FiveM callback function</param>
        private async void QueryAsync(string query, dynamic parameters, CallbackDelegate callback = null)
        {
            await Initialized();
            Task<long> resultTask = mysql.Query(query, Utility.TryParseParameters(parameters, settings.Debug));
#pragma warning disable CS4014
            resultTask.ContinueWith((task) => callback?.Invoke(task.Result));
#pragma warning restore CS4014
        }

        /// <summary>
        /// Async Implementation of the Query command.
        /// </summary>
        /// <param name="query">The mysql database query string</param>
        /// <param name="parameters">Ideally an IDictionary or table of parameters, can be null, will be parsed</param>
        /// <param name="callback">FiveM callback function</param>
        private async void QueryResultAsync(string query, dynamic parameters, CallbackDelegate callback = null)
        {
            await Initialized();
            Task<ResultSet> resultTask = mysql.QueryResult(query, Utility.TryParseParameters(parameters, settings.Debug));
#pragma warning disable CS4014
            resultTask.ContinueWith((task) => callback?.Invoke(task.Result));
#pragma warning restore CS4014
        }

        /// <summary>
        /// Async Implementation of the Scalar command.
        /// </summary>
        /// <param name="query">The mysql database query string</param>
        /// <param name="parameters">Ideally an IDictionary or table of parameters, can be null, will be parsed</param>
        /// <param name="callback">FiveM callback function</param>
        private async void QueryScalarAsync(string query, dynamic parameters, CallbackDelegate callback = null)
        {
            await Initialized();
            Task<object> resultTask = mysql.QueryScalar(query, Utility.TryParseParameters(parameters, settings.Debug));
#pragma warning disable CS4014
            resultTask.ContinueWith((task) => callback?.Invoke(task.Result));
#pragma warning restore CS4014
        }

        /// <summary>
        /// Insert wrapper for multiple rows, should be able to do single rows too
        /// </summary>
        /// <param name="table">Name of the table where the data is inserted</param>
        /// <param name="parameters">List of dictionaries each representing a row to be inserted</param>
        /// <param name="callback">FiveM callback function</param>
        /// <param name="lastInsertId">return the last insert id if true, otherwise affected rows</param>
        private async void Insert(string table, dynamic parameters, CallbackDelegate callback = null, bool lastInsertId = false)
        {
            await Initialized();
            MultiRowCommandBuilder multiRow = await ParseMultiRow(table, parameters);
            bool isInsert = (callback == null) ? false : lastInsertId;
            Task<long> resultTask = mysql.Query(multiRow.CommandText, multiRow.Parameters, isInsert);
#pragma warning disable CS4014
            resultTask.ContinueWith((task) => callback?.Invoke(task.Result));
#pragma warning restore CS4014 
        }

        /// <summary>
        /// Wrapper for Transactions
        /// </summary>
        /// <param name="querys">List of database queries</param>
        /// <param name="parameters">Parameters of the queries</param>
        /// <returns>true or false depending on whether the transaction succeeded or not</returns>
        private async Task<bool> Transaction(dynamic querys, dynamic parameters)
        {
            await Initialized();
            return await mysql.Transaction(Utility.TryParseTransactionQuerys(querys), Utility.TryParseParameters(parameters));
        }

        /// <summary>
        /// Async Wrapper for Transactions
        /// </summary>
        /// <param name="querys">List of database queries</param>
        /// <param name="parameters">Parameters of the queries</param>
        /// <param name="callback">FiveM callback function</param>
        private async void TransactionAsync(dynamic querys, dynamic parameters, CallbackDelegate callback = null)
        {
            await Initialized();
            Task<bool> resultTask = mysql.Transaction(Utility.TryParseTransactionQuerys(querys), Utility.TryParseParameters(parameters));
#pragma warning disable CS4014
            resultTask.ContinueWith((task) => callback?.Invoke(task.Result));
#pragma warning restore CS4014 
        }

        /// <summary>
        /// Parsing MultiRow with the TaskScheduler to avoid hitches
        /// </summary>
        /// <param name="table">Name of the table</param>
        /// <param name="parameters">List of dictionarys which represent each row inserted</param>
        /// <returns>Returns the MultiRow object which consists of a built query string and a set of parameters</returns>
        private async Task<MultiRowCommandBuilder> ParseMultiRow(string table, dynamic parameters) => await Task.Factory.StartNew(() =>
        {
            return MultiRowCommandBuilder.TryParse(table, parameters);
        }, CancellationToken.None, TaskCreationOptions.None, taskScheduler);

        /// <summary>
        /// Wait until the setup is complete
        /// </summary>
        /// <returns>awaitable Task until the class is initialized</returns>
        private async Task Initialized()
        {
            while (!initialized)
                await Delay(0);
        }
    }
}
