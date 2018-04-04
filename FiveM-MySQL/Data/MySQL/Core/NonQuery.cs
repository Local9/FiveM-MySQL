using GHMatti.Data.MySQL.Utilities;
using MySql.Data.MySqlClient;

namespace GHMatti.Data.MySQL.Core
{
    internal class NonQuery : Interaction<long>
    {
        /// <summary>
        /// True when the Last Insert Id should be returned
        /// </summary>
        public bool IsInsert { get; set; }

        /// <summary>
        /// Constructor, just uses the base constructor
        /// </summary>
        /// <param name="connectionString">Connection string used to connect to the database</param>
        /// <param name="debug">Print Debug information</param>
        public NonQuery(string connectionString, bool debug) : base(connectionString, debug) { }

        /// <summary>
        /// Execute the MySqlCommand as a NonQuery
        /// </summary>
        /// <param name="cmd">MySqlCommand to be executed</param>
        /// <returns></returns>
        protected override long Execute(MySqlCommand cmd)
        {
            long result = -1;

            cmd.CommandText = CommandText;
            cmd.AddParameters(Parameters);

            try
            {
                result = cmd.ExecuteNonQuery();
            }
            catch(MySqlException mySqlEx)
            {
                Utility.PrintErrorInformation(mySqlEx, Debug);
            }

            if (IsInsert)
                result = cmd.LastInsertedId;

            if (Debug)
                CommandText = cmd.Stringify();

            return result;
        }
    }
}
