using MySql.Data.MySqlClient;
using System.Collections.Generic;

namespace GHMatti.Data.MySQL.Utilities
{
    /// <summary>
    /// Extension class for the MySqlCommand
    /// </summary>
    public static class MySqlCommandExtensions
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
