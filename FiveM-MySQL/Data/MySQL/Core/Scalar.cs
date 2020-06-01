using GHMatti.Data.MySQL.Utilities;
using MySql.Data.MySqlClient;
using System;

namespace GHMatti.Data.MySQL.Core
{
    internal class Scalar : Interaction<object>
    {
        /// <summary>
        /// Constructor method
        /// </summary>
        /// <param name="connectionString">Connection string used to connect to the database</param>
        /// <param name="debug">Print Debug information</param>
        public Scalar(string connectionString, bool debug) : base(connectionString, debug) { }

        /// <summary>
        /// Executes a scalar
        /// </summary>
        /// <param name="cmd">MySqlCommand to be used to get the Scalar value</param>
        /// <returns>Scalar</returns>
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
