using System;
using System.Collections.Generic;

namespace GHMatti.MySQL.Core
{
    /// <summary>
    /// Class to handle the settings for MySQL
    /// </summary>
    public class Settings
    {
        /// <summary>
        /// Public attributes anyone can read
        /// </summary>
        public string ConnectionString => connectionString;
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
        private string connectionString = "";
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
                connectionString = convarConnectionString;
            }
            else
            {
                debug = Convert.ToBoolean(xmlConfiguration["MySQL:Debug"]);
                connectionString = String.Format("SERVER={0};PORT={1};DATABASE={2};UID={3};PASSWORD={4}",
                    xmlConfiguration["MySQL:Server"], xmlConfiguration["MySQL:Port"], xmlConfiguration["MySQL:Database"],
                    xmlConfiguration["MySQL:Username"], xmlConfiguration["MySQL:Password"]
                );
            }
        }
    }
}
