﻿using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GHMatti.Data.MySQL.Utilities
{
    /// <summary>
    /// Static utility class to print debug information
    /// </summary>
    internal static class Utility
    {
        /// <summary>
        /// Helper function to display MySQL error information
        /// </summary>
        /// <param name="mysqlEx">The MySqlException thrown</param>
        internal static void PrintErrorInformation(MySqlException mysqlEx, bool debug)
        {
            if (debug)
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
        internal static void PrintDebugInformation(long ctime, long qtime, string query, bool debug)
        {
            if (debug)
                CitizenFX.Core.Debug.Write(String.Format(
                    "[GHMattiMySQL Debug] Connection: {0}ms; Interaction: {1}ms; Total {2}ms for Query: {3}\n",
                    ctime, qtime, ctime + qtime, query
                ));
        }

        /// <summary>
        /// Check if the user supplied parameters are in the correct shape
        /// </summary>
        /// <param name="parameters">Parameters to parse</param>
        /// <param name="debug">if true write a warning for incorrectly-shaped parameters</param>
        /// <returns>Parameters in dictionary form parsed</returns>
        public static IDictionary<string, dynamic> TryParseParameters(dynamic parameters, bool debug = true)
        {
            IDictionary<string, dynamic> parsedParameters = null;
            try
            {
                parsedParameters = parameters;
            }
            catch
            {
                // Only Warn that the user supplied bad parameters when debug is set to true
                if (debug)
                    CitizenFX.Core.Debug.WriteLine("[GHMattiMySQL Warning] Parameters are not in Dictionary-shape");
                parsedParameters = null;
            }

            return parsedParameters;
        }

        /// <summary>
        /// Check if the user supplied queries are in the correct shape, move this somewhere else later
        /// </summary>
        /// <param name="querys">List of queries, if not it errors</param>
        /// <returns>Parsed List of queries</returns>
        public static IList<string> TryParseTransactionQuerys(dynamic querys)
        {
            IList<string> parsedList = null;
            try
            {
                parsedList = ((IList<object>)querys).Select(query => query.ToString()).ToList();
            }
            catch
            {
                throw new Exception("[GHMattiMySQL ERROR] Parameters are not in List-shape");
            }

            return parsedList;
        }
    }
}
