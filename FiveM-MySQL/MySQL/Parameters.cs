using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Linq;

namespace GHMatti.MySQL
{
    // Parameter handling class for the MySQL implementation
    public static class Parameters
    {
        // Extension to the MySqlCommand class to add all Parameters in a Dictionary directly
        public static void AddParameters(this MySqlCommand cmd, IDictionary<string, dynamic> parameters)
        {
            if (parameters != null)
                foreach (KeyValuePair<string, dynamic> kvp in parameters)
                    cmd.Parameters.AddWithValue(kvp.Key, kvp.Value);
        }

        // Check if the user supplied parameters are in the correct shape
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

        // Stringify query string for debug information
        public static string Stringify(this MySqlCommand cmd)
        {
            string result = cmd.CommandText;
            foreach (MySqlParameter parameter in cmd.Parameters)
                result = result.Replace(parameter.ParameterName, parameter.Value.ToString());
            return result;
        }

        // Check if the user supplied queries are in the correct shape
        public static IList<string> QueryList(dynamic querys)
        {
            IList<string> parsedList = null;
            try
            {
                parsedList = ((IList<object>)querys).Select(query => query.ToString()).ToList();
            }
            catch
            {
                throw new System.Exception("[GHMattiMySQL ERROR] Parameters are not in List-shape");
            }

            return parsedList;
        }
    }
}
