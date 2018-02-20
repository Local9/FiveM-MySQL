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
    // BaseScript class as a script for FiveM / CitizenFX that gets called
    public class Core : BaseScript
    {
        // Private TaskScheduler to not execute on the main thread
        private GHMattiTaskScheduler taskScheduler;
        private MySQL mysql;
        private MySQLSettings settings;
        private bool initialized;

        // Constructor to set Exports and CitizenFX.Core Handlers
        public Core()
        {
            taskScheduler = new GHMattiTaskScheduler();
            settings = new MySQLSettings();
            initialized = false;
            EventHandlers["onServerResourceStart"] += new Action<string>(Initialization);

            Exports.Add("Query", new Func<string, dynamic, Task<int>>(
                (query, parameters) => Query(query, parameters))
            );
            Exports.Add("QueryResult", new Func<string, dynamic, Task<MySQLResult>>(
                (query, parameters) => QueryResult(query, parameters))
            );
            Exports.Add("QueryScalar", new Func<string, dynamic, Task<dynamic>>(
                (query, parameters) => QueryScalar(query, parameters))
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

            Exports.Add("Insert", new Action<string, dynamic, CallbackDelegate>(
                (table, parameters, cb) => Insert(table, parameters, cb))
            );
        }

        // Initialization function. Nothing will execute before this is not done. Maybe remove the async and await?
        private async void Initialization(string resourcename)
        {
            if (API.GetCurrentResourceName() == resourcename)
            {
                settings.ConvarConnectionString = API.GetConvar("mysql_connection_string", "");
                settings.ConvarDebug = API.GetConvar("mysql_debug", "false");

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

        // Implementation of the standard Execute for a Command with a proper reply (rows changed?); so that lua waits for it to complete
        private async Task<int> Query(string query, dynamic parameters)
        {
            await Initialized();
            return await mysql.Query(query, Parameters.TryParse(parameters));
        }

        // Implementation for the standard Query / Result for a command.
        private async Task<MySQLResult> QueryResult(string query, dynamic parameters)
        {
            await Initialized();
            return await mysql.QueryResult(query, Parameters.TryParse(parameters));
        }

        // Implementation for the standard Scalar command, which only returns a singular value
        private async Task<dynamic> QueryScalar(string query, dynamic parameters)
        {
            await Initialized();
            return await mysql.QueryScalar(query, Parameters.TryParse(parameters));
        }

        // Async Implementation of the Execute command. This is way faster than using the Query method
        private async void QueryAsync(string query, dynamic parameters, CallbackDelegate callback = null)
        {
            await Initialized();
            dynamic result = await mysql.Query(query, Parameters.TryParse(parameters, settings.Debug));
            if (callback != null)
            {
                await Delay(0); // need to wait for the next server tick before invoking, will error otherwise
                callback.Invoke(result);
            }
        }

        // Async Implementation of the Query command.
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

        // Async Implementation of the Scalar command.
        private async void QueryScalarAsync(string query, dynamic parameters, CallbackDelegate callback = null)
        {
            await Initialized();
            dynamic result = await mysql.QueryScalar(query, Parameters.TryParse(parameters, settings.Debug));
            if (callback != null)
            {
                await Delay(0);
                callback.Invoke(result);
            }
        }

        // Insert wrapper for multiple rows, should be able to do single rows too
        private async void Insert(string table, dynamic parameters, CallbackDelegate callback = null)
        {
            await Initialized();
            MultiRow multiRow = await ParseMultiRow(table, parameters);
            await mysql.Query(multiRow.CommandText, multiRow.Parameters);
            if(callback != null)
            {
                // Ineffective, because the entire thing needs to superseede the Query function
                // and be completed in the same task, there might be other inserts resulting in a bad reply
                // move this to the query, not recommended to use at the moment when other inserts could be
                // happening, thus untested
                dynamic result = await mysql.QueryScalar("SELECT LAST_INSERT_ID()");
                await Delay(0);
                callback.Invoke(result);
            }
        }

        // Parsing MultiRow with the TaskScheduler to avoid hitches
        private async Task<MultiRow> ParseMultiRow(string table, dynamic parameters) => await Task.Factory.StartNew(() =>
        {
            return MultiRow.TryParse(table, parameters);
        }, CancellationToken.None, TaskCreationOptions.None, taskScheduler);

        // Wait until the setup is complete
        private async Task Initialized()
        {
            while (!initialized)
                await Delay(0);
        }
    }
}
