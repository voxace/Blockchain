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
        public static void WriteBlock(Block data)
        {
            string index = data.index.ToString("0000000000");
            string json = JsonConvert.SerializeObject(data, Newtonsoft.Json.Formatting.Indented);
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

		public static string SerializeTransaction(Transaction transaction)
		{
			return JsonConvert.SerializeObject(transaction, Newtonsoft.Json.Formatting.Indented);
		}

		public static Transaction DeserializeTransaction(string data)
		{
			Transaction transaction = new Transaction();
			JsonSerializer serializer = new JsonSerializer();
			transaction = JsonConvert.DeserializeObject<Transaction>(data);
			return transaction;
		}

		public static string SerializeBlock(Block block)
		{
			return JsonConvert.SerializeObject(block, Newtonsoft.Json.Formatting.Indented);
		}

		public static Block DeserializeBlock(string data)
		{
			Block block = new Block();
			JsonSerializer serializer = new JsonSerializer();
			block = JsonConvert.DeserializeObject<Block>(data);
			return block;
		}

		public static string SerializePeers(Peers peers)
		{
			return JsonConvert.SerializeObject(peers, Newtonsoft.Json.Formatting.Indented);
		}

		public static Peers DeserializePeers(string data)
		{
			Peers peers = new Peers();
			JsonSerializer serializer = new JsonSerializer();
			peers = JsonConvert.DeserializeObject<Peers>(data);
			return peers;
		}

		public static void WritePeers(Peers peers)
		{
			File.WriteAllText(System.IO.Directory.GetCurrentDirectory() + "\\peers.json", SerializePeers(peers));
		}

		public static Peers ReadPeers()
		{
			string path = System.IO.Directory.GetCurrentDirectory() + "\\peers.json";
			Peers peers = new Peers();
			using (StreamReader file = File.OpenText(path))
			{
				JsonSerializer serializer = new JsonSerializer();
				peers = (Peers)serializer.Deserialize(file, typeof(Peers));
			}
			return peers;
		}
	}
}
