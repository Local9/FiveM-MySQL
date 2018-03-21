using System;
using System.Collections.Generic;
using GHMatti.MySQL.Utilities;
using MySql.Data.MySqlClient;

namespace GHMatti.MySQL.Core
{
    internal class Transaction : Interaction<bool>
    {
        public MySqlConnection Connection { get; set; }
        public IList<string> Commands { get; set; }

        public Transaction(string connectionString, bool debug) : base(connectionString, debug) { }

        protected override bool Execute(MySqlCommand cmd)
        {
            bool result = false;
            CommandText = "Transaction";
            using (MySqlTransaction transaction = Connection.BeginTransaction())
            {
                cmd.AddParameters(Parameters);
                cmd.Transaction = transaction;

                try
                {
                    foreach (string query in Commands)
                    {
                        cmd.CommandText = query;
                        cmd.ExecuteNonQuery();
                    }
                    transaction.Commit();
                    result = true;
                }
                catch (Exception exception)
                {
                    transaction.Rollback();
                    if (Debug)
                        CitizenFX.Core.Debug.Write(String.Format("[GHMattiMySQL] [Failed Transaction] {0}\n{1}\n", exception.Message, exception.StackTrace));
                    else
                        CitizenFX.Core.Debug.Write(String.Format("[GHMattiMySQL] [Failed Transaction] {0}\n", exception.Message));
                }
            }

            return result;
        }
    }
}
