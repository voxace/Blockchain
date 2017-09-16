using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace Blockchain
{
    public class Block
    {        
        public string previousHash;
        public int index;
        public DateTime timestamp;
        public Ledger data;

        public Block()
        {
            
        }

        public void newBlock(int i, DateTime t, Ledger d, string ph)
        {
            previousHash = ph;
            index = i;
            timestamp = t;
            data = d;
        }

        public string HashBlock()
        {
            byte[] hash;
            string temp;
            SHA256Managed hasher = new SHA256Managed();
            temp = index.ToString() + timestamp.ToString() + data.ToString() + previousHash;
            Byte[] byteArray = Encoding.UTF8.GetBytes(temp);
            hash = hasher.ComputeHash(byteArray);
            string hashString = string.Empty;
            foreach (byte x in hash)
            {
                hashString += String.Format("{0:x2}", x);
            }
            return hashString;
        }

        public static void saveBlock(Block b)
        {
            Serialize.WriteBlock(b);
        }

        public void addTransaction(string sender, string recipient, float amount)
        {
            data.addTransaction(new Blockchain.Transaction(sender, recipient, amount));
        }

		public Ledger GetData()
		{
			return data;
		}

		public int getIndex()
		{
			return index;
		}

		public string getPreviousHash()
		{
			return previousHash;
		}

		public DateTime getTimestamp()
		{
			return timestamp;
		}
    }
}
