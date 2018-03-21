using MySql.Data.MySqlClient;
using System.Collections.Generic;

namespace GHMatti.MySQL
{
    /// <summary>
    /// Parameter handling class for the MySQL implementation
    /// </summary>
    public static class Parameters
    {
        /// <summary>
        /// Extension to the MySqlCommand class to add all Parameters in a Dictionary directly
        /// </summary>
        /// <param name="cmd">Extension variable</param>
        /// <param name="parameters">Parameters to add</param>
        public static void AddParameters(this MySqlCommand cmd, IDictionary<string, dynamic> parameters)
        {
            if (parameters != null)
                foreach (KeyValuePair<string, dynamic> kvp in parameters)
                    cmd.Parameters.AddWithValue(kvp.Key, kvp.Value);
        }

        /// <summary>
        /// Check if the user supplied parameters are in the correct shape
        /// </summary>
        /// <param name="parameters">Parameters to parse</param>
        /// <param name="debug">if true write a warning for incorrectly-shaped parameters</param>
        /// <returns>Parameters in dictionary form parsed</returns>
        public static IDictionary<string, dynamic> TryParse(dynamic parameters, bool debug = true)
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
        /// Stringify query string for debug information
        /// </summary>
        /// <param name="cmd">The MysqlCommand</param>
        /// <returns>Returns the MysqlCommand stringified</returns>
        public static string Stringify(this MySqlCommand cmd)
        {
            string result = cmd.CommandText;
            foreach (MySqlParameter parameter in cmd.Parameters)
                result = result.Replace(parameter.ParameterName, parameter.Value.ToString());
            return result;
        }
    }
}
