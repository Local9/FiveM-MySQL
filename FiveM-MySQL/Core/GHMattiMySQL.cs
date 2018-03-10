using CitizenFX.Core;
using CitizenFX.Core.Native;
using GHMatti.Core;
using GHMatti.MySQL;
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
        private MySQLSettings settings;
        private bool initialized;

        /// <summary>
        /// Constructor to set Exports and CitizenFX.Core Handlers
        /// </summary>
        public Core()
        {
            taskScheduler = new GHMattiTaskScheduler();
            settings = new MySQLSettings();
            initialized = false;
            EventHandlers["onServerResourceStart"] += new Action<string>(Initialization);

            Exports.Add("Query", new Func<string, dynamic, Task<long>>(
                (query, parameters) => Query(query, parameters))
            );
            Exports.Add("QueryResult", new Func<string, dynamic, Task<MySQLResult>>(
                (query, parameters) => QueryResult(query, parameters))
            );
            Exports.Add("QueryScalar", new Func<string, dynamic, Task<object>>(
                (query, parameters) => QueryScalar(query, parameters))
            );
            Exports.Add("TransactionAsync", new Func<dynamic, dynamic, Task<bool>>(
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
        private async void Initialization(string resourcename)
        {
            if (API.GetCurrentResourceName() == resourcename)
            {
                settings.ConvarConnectionString = API.GetConvar("mysql_connection_string", "");
                settings.ConvarDebug = API.GetConvar("mysql_debug", "false");
                taskScheduler.ThreadLimit = API.GetConvarInt("mysql_thread_limit", 0);
                // You cannot do API Calls in these Threads, you need to do them before or inbetween. Use them only for heavy duty work,
                // (file operations, database interaction or transformation of data), or when working with an external library.
                await Task.Factory.StartNew(() =>
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
            return await mysql.Query(query, Parameters.TryParse(parameters));
        }

        /// <summary>
        /// Implementation for the standard Query / Result for a command.
        /// </summary>
        /// <param name="query">The mysql database query string</param>
        /// <param name="parameters">Ideally an IDictionary or table of parameters, can be null, will be parsed</param>
        /// <returns>The result table that was queried</returns>
        private async Task<MySQLResult> QueryResult(string query, dynamic parameters)
        {
            await Initialized();
            return await mysql.QueryResult(query, Parameters.TryParse(parameters));
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
            return await mysql.QueryScalar(query, Parameters.TryParse(parameters));
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
            long result = await mysql.Query(query, Parameters.TryParse(parameters, settings.Debug));
            if (callback != null)
            {
                await Delay(0); // need to wait for the next server tick before invoking, will error otherwise
                callback.Invoke(result);
            }
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
            dynamic result = await mysql.QueryResult(query, Parameters.TryParse(parameters, settings.Debug));
            if (callback != null)
            {
                await Delay(0);
                callback.Invoke(result);
            }
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
            object result = await mysql.QueryScalar(query, Parameters.TryParse(parameters, settings.Debug));
            if (callback != null)
            {
                await Delay(0);
                callback.Invoke(result);
            }
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
            MultiRow multiRow = await ParseMultiRow(table, parameters);
            bool isInsert = (callback == null) ? false : lastInsertId;
            long result = await mysql.Query(multiRow.CommandText, multiRow.Parameters, isInsert);
            if (callback != null)
            {
                await Delay(0);
                callback.Invoke(result);
            }
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
            return await mysql.Transaction(TryParseTransactionQuerys(querys), Parameters.TryParse(parameters));
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
            bool result = await mysql.Transaction(TryParseTransactionQuerys(querys), Parameters.TryParse(parameters));
            if (callback != null)
            {
                await Delay(0);
                callback.Invoke(result);
            }
        }

        /// <summary>
        /// Parsing MultiRow with the TaskScheduler to avoid hitches
        /// </summary>
        /// <param name="table">Name of the table</param>
        /// <param name="parameters">List of dictionarys which represent each row inserted</param>
        /// <returns>Returns the MultiRow object which consists of a built query string and a set of parameters</returns>
        private async Task<MultiRow> ParseMultiRow(string table, dynamic parameters) => await Task.Factory.StartNew(() =>
        {
            return MultiRow.TryParse(table, parameters);
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

        /// <summary>
        /// Check if the user supplied queries are in the correct shape, move this somewhere else later
        /// </summary>
        /// <param name="querys">List of queries, if not it errors</param>
        /// <returns>Parsed List of queries</returns>
        public static System.Collections.Generic.IList<string> TryParseTransactionQuerys(dynamic querys)
        {
            System.Collections.Generic.IList<string> parsedList = null;
            try
            {
                parsedList = ((System.Collections.Generic.IList<object>)querys).Select(query => query.ToString()).ToList();
            }
            catch
            {
                throw new System.Exception("[GHMattiMySQL ERROR] Parameters are not in List-shape");
            }

            return parsedList;
        }
    }
}
