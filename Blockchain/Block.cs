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

		/// <summary>
		/// Attempts to find the correct hash for the current block.
		/// </summary>
		/// <param name="miningWindow">Shows the mining window if set to true.</param>
		/// <returns>Returns true if this user successfully mined the block.</returns>
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
			Double duration = finish.Subtract(start).TotalSeconds;

			mw.ConsoleOutput.AppendText(Environment.NewLine + "Block time: " + duration.ToString() + " Seconds. Average speed: " + ((count / duration) / 1000).ToString("N2") + " KH/sec.");

			return true;

		}

        public static void saveBlock(Block b)
        {
            Serialize.WriteBlock(b);
        }

        public void addTransaction(string sender, string recipient, float amount)
        {
			Transaction temp = new Transaction(sender, recipient, amount);
			if (Node.verifyTransaction(temp))
			{
				data.addTransaction(temp);
			}
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

		// TODO: rewardMiner(pubkey)
		// Rewards the miner of the current block with the current block reward
    }
}
