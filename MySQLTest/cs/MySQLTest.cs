using CitizenFX.Core;
using CitizenFX.Core.Native;
using GHMatti.MySQL;
using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace MySQLTest
{
    public class MySQLTest : BaseScript
    {
        private GHMatti.Core.GHMattiTaskScheduler taskScheduler;
        private GHMatti.MySQL.MySQL mysql;

        public MySQLTest()
        {
            taskScheduler = new GHMatti.Core.GHMattiTaskScheduler();
            EventHandlers["onServerResourceStart"] += new Action<string>(Initialization);
        }

        private void Initialization(string resourcename)
        {
            if(API.GetCurrentResourceName() == resourcename)
            {
                MySQLSettings settings = new MySQLSettings();
                settings.ConvarConnectionString = API.GetConvar("mysql_connection_string", "");
                settings.ConvarDebug = API.GetConvar("mysql_debug", "true");
                XDocument xDocument = XDocument.Load(Path.Combine("resources", resourcename, "settings.xml"));
                settings.XMLConfiguration = xDocument.Descendants("setting").ToDictionary(
                    setting => setting.Attribute("key").Value,
                    setting => setting.Value
                );
                mysql = new MySQL(settings, taskScheduler);
                ExecuteQueries(resourcename);
            }
        }

        private async void ExecuteQueries(string resourcename)
        {
            //await Delay(60000);
            string line;
            System.Diagnostics.Stopwatch timer = new System.Diagnostics.Stopwatch();
            StreamReader file = new StreamReader(Path.Combine("resources",resourcename,"sql","MySQLTest.sql"));
            timer.Start();
            while ((line = file.ReadLine()) != null)
            {
                await mysql.Query(line);
            }
            timer.Stop();
            file.Close();
            Debug.WriteLine(String.Format("C# executed all Queries in: {0}ms", timer.ElapsedMilliseconds));
        }
    }
}
