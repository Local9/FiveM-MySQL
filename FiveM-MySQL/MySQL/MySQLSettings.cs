using System;
using System.Collections.Generic;

namespace GHMatti.MySQL
{
    // Class to handle the settings for MySQL
    public class MySQLSettings
    {
        // Public attributes anyone can read
        public string ConnectionString => connectionString;
        public bool Debug => debug;
        public int ThreadLimit => threadLimit;

        // Public attributes to set
        public Dictionary<string, string> XMLConfiguration { set => xmlConfiguration = value; }
        public string ConvarConnectionString { set => convarConnectionString = value; }
        public string ConvarDebug { set => convarDebug = value; }

        // Actual variables that the class manages
        private string connectionString = "";
        private bool debug = false;
        private int threadLimit = 0;

        // internal xmlConfiguration
        private Dictionary<string, string> xmlConfiguration;
        // internal convar variables
        private string convarDebug = "";
        private string convarConnectionString = "";

        // empty constructor, got nothing to do
        public MySQLSettings() { }

        // Apply the configuration from the internal variables to the actual variables
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
            threadLimit = Convert.ToInt32(xmlConfiguration["MySQL:ThreadLimit"]);
        }
    }
}
