using MySqlConnector;
using System;
using System.Collections.Generic;

namespace GHMatti.Data.MySQL.Core
{
    /// <summary>
    /// Class to handle the settings for MySQL
    /// </summary>
    public class Settings
    {
        /// <summary>
        /// Public attributes anyone can read
        /// </summary>
        public string ConnectionString => connectionStringBuilder.ConnectionString;
        public bool Debug => debug;

        /// <summary>
        /// Public attributes to set
        /// </summary>
        public Dictionary<string, string> XMLConfiguration { set => xmlConfiguration = value; }
        public string ConvarConnectionString { set => convarConnectionString = value; }
        public string ConvarDebug { set => convarDebug = value; }

        /// <summary>
        /// Actual variables that the class manages
        /// </summary>
        private MySqlConnectionStringBuilder connectionStringBuilder = new MySqlConnectionStringBuilder();
        private bool debug = false;

        /// <summary>
        /// internal xmlConfiguration
        /// </summary>
        private Dictionary<string, string> xmlConfiguration;
        /// <summary>
        /// internal convar variables
        /// </summary>
        private string convarDebug = "";
        private string convarConnectionString = "";

        /// <summary>
        /// empty constructor, got nothing to do
        /// </summary>
        public Settings() { }

        /// <summary>
        /// Apply the configuration from the internal variables to the actual variables
        /// </summary>
        public void Apply()
        {
            if (Convert.ToBoolean(xmlConfiguration["MySQL:UseConvars"]))
            {
                debug = Convert.ToBoolean(convarDebug);
                connectionStringBuilder.ConnectionString = convarConnectionString;
            }
            else
            {
                debug = Convert.ToBoolean(xmlConfiguration["MySQL:Debug"]);
                connectionStringBuilder.Server = xmlConfiguration["MySQL:Server"];
                connectionStringBuilder.Port = Convert.ToUInt32(xmlConfiguration["MySQL:Port"]);
                connectionStringBuilder.Database = xmlConfiguration["MySQL:Database"];
                connectionStringBuilder.UserID = xmlConfiguration["MySQL:Username"];
                connectionStringBuilder.Password = xmlConfiguration["MySQL:Password"];
                connectionStringBuilder.MinimumPoolSize = Convert.ToUInt32(xmlConfiguration["MySQL:MinPoolSize"]);
            }
        }
    }
}
