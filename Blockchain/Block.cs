using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Threading;

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

		public async Task<bool> mineBlock(bool miningWindow)
		{
			int count = 0;
			int count2 = 0;
			string hashString = "1234567890";

			DateTime start = DateTime.Now;
			DateTime previous = DateTime.Now;

			Miner mw = new Miner();

			if (miningWindow)
			{				
				mw.Show();
			}
			

			await Task.Run(() =>
			{
				while (hashString.Substring(0, 5) != "00000")
				{
					byte[] hash;
					string temp;
					SHA256Managed hasher = new SHA256Managed();
					temp = count.ToString() + index.ToString() + timestamp.ToString() + data.ToString() + previousHash;
					Byte[] byteArray = Encoding.UTF8.GetBytes(temp);
					hash = hasher.ComputeHash(byteArray);

					hashString = string.Empty;

					foreach (byte x in hash)
					{
						hashString += String.Format("{0:x2}", x);
					}

					if(!miningWindow)
					{
						Console.WriteLine(count.ToString() + "  -  " + hashString);
					}

					if (miningWindow)
					{

						if (count % 1000 == 0)
						{
							mw.Dispatcher.Invoke(DispatcherPriority.Render, new Action(() =>
							{
								mw.ConsoleOutput.AppendText(Environment.NewLine + count.ToString() + " - " + hashString);
								mw.ConsoleOutput.ScrollToEnd();
							}));
						}

						DateTime elapsed = DateTime.Now;

						if (elapsed.Subtract(previous).TotalMilliseconds >= 1000)
						{
							mw.Dispatcher.Invoke(DispatcherPriority.Render, new Action(() =>
							{
								mw.speedWindow.Text = (count2 / 1000).ToString() + " KH/sec";
							}));

							count2 = 0;
							previous = DateTime.Now;
						}
					}
					
					count++;
					count2++;
				}
			});

			if (miningWindow)
			{
				mw.Dispatcher.Invoke(DispatcherPriority.Render, new Action(() =>
				{
					mw.ConsoleOutput.AppendText(Environment.NewLine + count.ToString() + " - " + hashString);
					mw.ConsoleOutput.ScrollToEnd();
				}));
			}

			DateTime finish = DateTime.Now;
			Double duration = finish.Subtract(start).TotalMilliseconds;

			MessageBox.Show("The final hash is: " + hashString + " - it took " + count.ToString() + " attempts and " + duration.ToString() + " Milliseconds.");

			return true;

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

		public static double getBalance(string pubkey)
		{
			double balance = 0.0;
			string[] files = System.IO.Directory.GetFiles(System.IO.Directory.GetCurrentDirectory() + "\\Chain\\");
			int count = 0;

			foreach (string file in files)
			{
				Block b = new Block();
				string index = count.ToString("0000000000");
				b = Serialize.ReadBlock(index);
				//string json = Newtonsoft.Json.JsonConvert.SerializeObject(b, Newtonsoft.Json.Formatting.Indented);
				//MessageBox.Show(json);
				Ledger l = new Ledger();
				l = b.GetData();
				balance += l.getBalance(pubkey);
				count++;
			}

			return balance;
		}

		// TODO: sendTransaction(sender, recipient, amount)
		// Calls getBalance to ensure there is enough in wallet
		// Transmits the transaction to the network

		// TODO: receiveTransaction(sender, recipient, amount)
		// Verfies transaction before adding it to the Transaction list

		// TODO: getBalance(pubkey)
		// Loops through each block looking at inputs/outputs of pubkey address specified

		// TODO: getTransactions(pubkey)
		// Loops through all transactions in a single block and returns the overall input/output of pubkey address specified

		// TODO: verifyTransaction(sender's pubkey, amount)
		// Calls getBalance() and then checks that the amount is valid
		// Loops through all blocks to check that the sender has enough money to send.

		// TODO: mineBlock()
		// Attempts to find the correct hash of the block
		// Includes a special transaction to the owners address to reward
		// Hash of the block is whatever random number it takes to get the first 3 numbers to be 0
		// Calls rewardMiner(pubkey)

		// TODO: rewardMiner(pubkey)
		// Rewards the miner of the current block with the current block reward
    }
}
