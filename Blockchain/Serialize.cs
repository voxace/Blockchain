using System;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
using Newtonsoft.Json;

namespace Blockchain
{
    class Serialize
    {
        static string _StartupXML = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "//Serialization.xml";
        
        public static void WriteBlock(Block data)
        {
            string index = data.index.ToString("0000000000");
            string json = JsonConvert.SerializeObject(data, Newtonsoft.Json.Formatting.Indented);
            //MessageBox.Show(json);
            File.WriteAllText(System.IO.Directory.GetCurrentDirectory() + "\\Chain\\" + index + ".json", JsonConvert.SerializeObject(data, Newtonsoft.Json.Formatting.Indented));
        }
        public static Block ReadBlock(string index)
        {
            Block b = new Block();
            using (StreamReader file = File.OpenText(System.IO.Directory.GetCurrentDirectory() + "\\Chain\\" + index + ".json"))
            {
                JsonSerializer serializer = new JsonSerializer();
                b = (Block)serializer.Deserialize(file, typeof(Block));
            }
            return b;
        }
    }
}
