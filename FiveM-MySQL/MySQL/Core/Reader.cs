using System.Linq;
using GHMatti.MySQL.Utilities;
using MySql.Data.MySqlClient;

namespace GHMatti.MySQL.Core
{
    internal class Reader : Interaction<ResultSet>
    {
        public Reader(string connectionString, bool debug) : base(connectionString, debug) { }

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
