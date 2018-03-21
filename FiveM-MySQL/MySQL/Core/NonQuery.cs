using GHMatti.MySQL.Utilities;
using MySql.Data.MySqlClient;

namespace GHMatti.MySQL.Core
{
    internal class NonQuery : Interaction<long>
    {
        public bool IsInsert { get; set; }

        public NonQuery(string connectionString, bool debug) : base(connectionString, debug) { }

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
