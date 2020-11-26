using GHMatti.Data.MySQL.Utilities;
using MySqlConnector;
using System.Linq;

namespace GHMatti.Data.MySQL.Core
{
    internal class Reader : Interaction<ResultSet>
    {
        /// <summary>
        /// Constructor method
        /// </summary>
        /// <param name="connectionString">Connection string used to connect to the database</param>
        /// <param name="debug">Print Debug information</param>
        public Reader(string connectionString, bool debug) : base(connectionString, debug) { }

        /// <summary>
        /// Returns a ResultSet from a select command
        /// </summary>
        /// <param name="cmd">MySqlCommand to use for the interaction</param>
        /// <returns>ResultSet containing the rows</returns>
        protected override ResultSet Execute(MySqlCommand cmd)
        {
            ResultSet result = new ResultSet();
            cmd.CommandText = CommandText;
            cmd.AddParameters(Parameters);

            try
            {
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                        result.Add(Enumerable.Range(0, reader.FieldCount).ToDictionary(reader.GetName,
                            i => (reader.IsDBNull(i)) ? null : reader.GetValue(i)));
                }
            }
            catch (MySqlException mysqlEx)
            {
                Utility.PrintErrorInformation(mysqlEx, Debug);
            }

            if (Debug)
                CommandText = cmd.Stringify();

            return result;
        }
    }
}
