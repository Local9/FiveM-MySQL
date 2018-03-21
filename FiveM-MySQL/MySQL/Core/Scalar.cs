using System;
using GHMatti.MySQL.Utilities;
using MySql.Data.MySqlClient;

namespace GHMatti.MySQL.Core
{
    internal class Scalar : Interaction<object>
    {
        public Scalar(string connectionString, bool debug) : base(connectionString, debug) { }

        protected override object Execute(MySqlCommand cmd)
        {
            object result = null;
            cmd.CommandText = CommandText;
            cmd.AddParameters(Parameters);

            try
            {
                result = cmd.ExecuteScalar();
            }
            catch (MySqlException mysqlEx)
            {
                Utility.PrintErrorInformation(mysqlEx, Debug);
            }

            if (result != null && result.GetType() == typeof(DBNull))
                result = null;

            if (Debug)
                CommandText = cmd.Stringify();

            return result;
        }
    }
}
