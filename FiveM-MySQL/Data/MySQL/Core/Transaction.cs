using System;
using System.Collections.Generic;
using GHMatti.Data.MySQL.Utilities;
using MySql.Data.MySqlClient;

namespace GHMatti.Data.MySQL.Core
{
    internal class Transaction : Interaction<bool>
    {
        /// <summary>
        /// Commands are passed to a transaction via this property
        /// </summary>
        public IList<string> Commands { get; set; }

        /// <summary>
        /// Constructor method
        /// </summary>
        /// <param name="connectionString">Connection string used to connect to the database</param>
        /// <param name="debug">Print Debug information</param>
        public Transaction(string connectionString, bool debug) : base(connectionString, debug) { }

        /// <summary>
        /// Execute a transaction
        /// </summary>
        /// <param name="cmd">MySqlCommand to be used for the transaction</param>
        /// <returns>bool transactionSucceeded</returns>
        protected override bool Execute(MySqlCommand cmd)
        {
            bool result = false;
            CommandText = "Transaction";
            using (MySqlTransaction transaction = connection.BeginTransaction())
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
