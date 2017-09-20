using System;
using System.Text;
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

		/// <summary>
		/// Adds the initial data to the Block
		/// </summary>
		/// <param name="i">Index</param>
		/// <param name="ph">Previous Hash</param>
        public void NewBlock(int i, string ph)
        {
            previousHash = ph;
            index = i;
			timestamp = DateTime.Now.ToUniversalTime();
            data = new Ledger();
        }

		/// <summary>
		/// Returns the hash of the data included in this Block
		/// </summary>
		/// <returns>Hash of the data in this block as a String</returns>
        public string HashBlock()
        {
            byte[] hash;
            string temp;
            SHA256Managed hasher = new SHA256Managed();
            temp = index.ToString() + timestamp.ToString() + data.getString() + previousHash;
            Byte[] byteArray = Encoding.UTF8.GetBytes(temp);
            hash = hasher.ComputeHash(byteArray);
            string hashString = string.Empty;
            foreach (byte x in hash)
            {
                hashString += String.Format("{0:x2}", x);
            }
            return hashString;
        }

		/// <summary>
		/// Adds the transaction to the block
		/// </summary>
		/// <param name="transaction">The transaction to add</param>
		public void AddTransaction(Transaction transaction)
		{
			data.AddTransaction(transaction);
		}

		/// <summary>
		/// Returns the complete Ledger for this Block
		/// </summary>
		/// <returns>Complete Ledger of this Block</returns>
		public Ledger GetData()
		{
			return data;
		}

		/// <summary>
		/// Returns the index of this Block
		/// </summary>
		/// <returns></returns>
		public int getIndex()
		{
			return index;
		}

		/// <summary>
		/// Returns the hash of the previous Block
		/// </summary>
		/// <returns></returns>
		public string getPreviousHash()
		{
			return previousHash;
		}

		/// <summary>
		/// Returns the timestamp of this Block
		/// </summary>
		/// <returns></returns>
		public DateTime getTimestamp()
		{
			return timestamp;
		}
    }
}
