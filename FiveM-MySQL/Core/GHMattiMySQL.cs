using CitizenFX.Core;
using CitizenFX.Core.Native;
using GHMatti.Core;
using GHMatti.MySQL;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace GHMattiMySQL
{
    public class Core : BaseScript
    {
        private GHMattiTaskScheduler taskScheduler;
        private MySQL mysql;
        private MySQLSettings settings;
        private bool initialized;

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

            Exports.Add("Insert", new Action<string, dynamic>(
                (table, parameters) => Insert(table, parameters))
            );
        }

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

        private async Task<int> Query(string query, dynamic parameters)
        {
            await Initialized();
            return await mysql.Query(query, Parameters.TryParse(parameters));
        }

        private async Task<MySQLResult> QueryResult(string query, dynamic parameters)
        {
            await Initialized();
            return await mysql.QueryResult(query, Parameters.TryParse(parameters));
        }

        private async Task<dynamic> QueryScalar(string query, dynamic parameters)
        {
            await Initialized();
            return await mysql.QueryScalar(query, Parameters.TryParse(parameters));
        }

        private async void QueryAsync(string query, dynamic parameters, CallbackDelegate callback = null)
        {
            await Initialized();
            dynamic result = await mysql.Query(query, Parameters.TryParse(parameters, settings.Debug));
            if(callback != null)
            {
                await Delay(0); // need to wait for the next server tick before invoking, will error otherwise
                callback.Invoke(result);
            }
        }

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

        private async void Insert(string table, dynamic parameters)
        {
            await Initialized();
            MultiRow multiRow = await ParseMultiRow(table, parameters);
            await mysql.Query(multiRow.CommandText, multiRow.Parameters);
        }

        private async Task<MultiRow> ParseMultiRow(string table, dynamic parameters) => await Task.Factory.StartNew(() =>
        {
            return MultiRow.TryParse(table, parameters);
        }, CancellationToken.None, TaskCreationOptions.None, taskScheduler);

        private async Task Initialized()
        {
            while (!initialized)
                await Delay(0);
        }
    }
}
