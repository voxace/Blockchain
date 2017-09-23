using System;
using System.Text;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Windows;

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
            SHA256Managed hasher = new SHA256Managed();

			// Convert contents of block to a string
			System.Text.StringBuilder temp = new System.Text.StringBuilder();
			temp.Append(index.ToString());
			temp.Append(timestamp.ToUniversalTime().ToString());
			temp.Append(data.getString());
			temp.Append(previousHash);

			// Compute hash
			Byte[] byteArray = Encoding.UTF8.GetBytes(temp.ToString());
            hash = hasher.ComputeHash(byteArray, 0, Encoding.UTF8.GetByteCount(temp.ToString()));
			System.Text.StringBuilder hashString = new System.Text.StringBuilder();
			foreach (byte x in hash)
            {
				hashString.Append(x.ToString("x2"));
			}

			// Return the computed hash
            return hashString.ToString();
        }

		/// <summary>
		/// Adds the transaction to the block
		/// </summary>
		/// <param name="transaction">The transaction to add</param>
		public void AddTransaction(Transaction transaction, List<Block> chain)
		{
			bool match = false;
			foreach(Block b in chain)
			{
				foreach(Transaction t in data.transactions)
				{
					if(t.txid == transaction.txid)
					{
						// Transaction already included
						match = true;
					}
				}
			}
			if(!match)
			{
				// Add the transaction since it does not exist in chain
				data.AddTransaction(transaction);
			}
			
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

		/// <summary>
		/// Returns the timestamp in a specific string format that will ensure compatibility across all systems
		/// </summary>
		/// <returns></returns>
		public string getTimeString()
		{
			return timestamp.ToString("yyyy/MM/dd HH:mm:ss");
		}
	}
}
