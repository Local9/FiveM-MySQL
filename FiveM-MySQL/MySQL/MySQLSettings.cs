using System;
using System.Collections.Generic;

namespace GHMatti.MySQL
{
    public class MySQLSettings
    {
        public string ConnectionString => connectionString;
        public bool Debug => debug;

        public Dictionary<string, string> XMLConfiguration { set => xmlConfiguration = value; }
        public string ConvarConnectionString { set => convarConnectionString = value; }
        public string ConvarDebug { set => convarDebug = value; }

        private string connectionString = "";
        private bool debug = false;

        private Dictionary<string, string> xmlConfiguration = new Dictionary<string, string>();
        private string convarDebug = "";
        private string convarConnectionString = "";

        public MySQLSettings() { }

        public void Apply()
        {
            if(Convert.ToBoolean(xmlConfiguration["MySQL:UseConvars"]))
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
