﻿using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using NetworkCommsDotNet;
using NetworkCommsDotNet.Connections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Windows;
using System.Timers;

namespace Blockchain
{
	// TODO: Interaction between peers, consensus about longest valid chain
	// TODO: Transition from UDP to TCP. Use UDP for peer discovery, build list of peers for P2P
	// TODO: Ability to download blockchain from peers, rather than relying on loading from directory
	// TODO: Check loaded blockchain from directory against longest chain on network (match hashes)

	class Node
	{
		public Block currentBlock;
		public Block previousBlock;
		public List<Block> chain = new List<Block>();
		public Network network = new Network();
		public int blockHeight = 0;
        public int networkBlockHeight = 0;
		public Miner mw = new Miner();
		public bool mining = false;
		public string miner_id = "";
        static Timer timer;

        public Node()
		{
			//Trigger the correct method when a certain packet type is received
			NetworkComms.AppendGlobalIncomingPacketHandler<string>("SendTransaction", ReceiveTransaction);
			NetworkComms.AppendGlobalIncomingPacketHandler<string>("SendBlock", ReceiveBlock);
			NetworkComms.AppendGlobalIncomingPacketHandler<Tuple<string,int>>("RequestBlock", SendBlockAtHeight);
			NetworkComms.AppendGlobalIncomingPacketHandler<string>("PeerList", ReceivePeers);
		
			//Start listening for incoming connections
			Connection.StartListening(ConnectionType.TCP, new System.Net.IPEndPoint(System.Net.IPAddress.Any, 9999));

            // Timer for syncing blockchain
            timer = new Timer(1000);
            timer.Elapsed += new ElapsedEventHandler(Sync);
            timer.Enabled = false;
        }

        private void Sync(object sender, ElapsedEventArgs e)
        {
            timer.Enabled = false;
            SyncBlockchain();
            timer.Enabled = true;
        }

		private void ReceivePeers(PacketHeader packetHeader, Connection connection, string peer_list)
		{
			Peers new_peers = Serialize.DeserializePeers(peer_list);
			network.NewPeers(new_peers);
		}

		private Block LoadGenesisBlock()
		{
			return Serialize.ReadBlock("0000000000");
		}

		public async Task<bool> LoadBlockchain()
		{
			// TODO: When loading chain confirm integrity against peers

			string[] files = System.IO.Directory.GetFiles(System.IO.Directory.GetCurrentDirectory() + "\\Chain\\");

			await Task.Run(() =>
			{
				foreach (string file in files)
				{
					string index = ConvertToChainString(blockHeight);
					Block b = Serialize.ReadBlock(index);
					if (blockHeight > 0)
					{
						// Testing: Compare hashes
						// MessageBox.Show(chain.ElementAt(blockHeight - 1).HashBlock() + "\n\n" + b.previousHash);

						// Check to see if blockchain was tampered with
						if (b.previousHash != chain.ElementAt(blockHeight - 1).HashBlock())
						{							
							//MessageBox.Show("Chain has been modified. Exiting...");
							//break;
						}
					}
					
					chain.Add(b);
					blockHeight++;
					network.blockheight = blockHeight;
				}

                // Start timer to keep chain in sync
                timer.Enabled = true;

                previousBlock = chain.ElementAt(blockHeight - 1);
				currentBlock = new Block();
				currentBlock.NewBlock(blockHeight, previousBlock.HashBlock());

			});
			return true;
		}

		private string ConvertToChainString(int index)
		{
			if(index == 0)
			{
				return "0000000000";
			}
			else
			{
				return index.ToString("0000000000");
			}			
		}

		public Tuple<string,string,string> queryBlockInfo(int index)
		{
			string data = chain.ElementAt(index).GetData().getString();
			string timestamp = chain.ElementAt(index).getTimeString();
			string previousHash = chain.ElementAt(index).getPreviousHash();
			return new Tuple<string, string, string>(data, timestamp, previousHash);
		}

		public static double getBalance(string pubkey)
		{
			double balance = 0.0;
			string[] files = System.IO.Directory.GetFiles(System.IO.Directory.GetCurrentDirectory() + "\\Chain\\");
			int count = 0;

			foreach (string file in files)
			{
				string index = count.ToString("0000000000");
				Block b = Serialize.ReadBlock(index);
				Ledger l = b.GetData();
				balance += l.getBalance(pubkey);
				count++;
			}

			return balance;
		}

		public double getUnconfirmedBalance(string pubkey)
		{
			return currentBlock.data.getBalance(pubkey);
		}

		public Tuple<bool, string> AddTransaction(string sender, string recipient, double amount, string txid)
		{
			Transaction t = new Transaction(sender, recipient, amount, txid);
			Tuple<bool, string> result = verifyTransaction(t);

			if (result.Item1)
			{	
				// Add transaction to current block
				currentBlock.AddTransaction(t,chain);
				
				// Broadcast transaction to network
				SendTransaction(t);
				return result;
			}
			else
			{
				return result;
			}
		}

		public static Tuple<bool, string> verifyTransaction(Transaction t)
		{
            // check signature via asymmetric signature verification
            if(Keys.VerifyData((t.sender + t.recipient + t.amount.ToString()), t.txid, t.sender))
			{
				if (getBalance(t.sender) >= t.amount)
				{
					return new Tuple<bool,string>(true, "Transaction ID: \n\n" + t.txid + "\n\nHas been verified successfully.");
				}
				else
				{
					return new Tuple<bool, string>(false, "Transaction failed. Insufficient balance.");
				}
			}
			else
			{
				return new Tuple<bool, string>(false, "Transaction failed. Invalid TXID.");
			}            
		}

		public void verifyTransactions(Transaction t)
		{
			if (getBalance(t.sender) >= t.amount)
			{
				// Transaction OK
			}
			else
			{
				// Delete transaction
				currentBlock.data.removeTransaction(t);
			}
		}

		#region Networking
		// Broadcast to network
		public void SendTransaction(Transaction transaction)
		{
			// Serialize transaction as JSON data first
			string tx = Serialize.SerializeTransaction(transaction);

			// Broadcast to network via TCP
			network.BroadcastTransaction(tx);

			// Show confirmation
			//MessageBox.Show("Transaction Sent with TXID: \n\n" + transaction.txid);
		}

		private void SendBlockAtHeight(PacketHeader packetHeader, Connection connection, Tuple<string,int> blockRequest)
		{
			// Serialize transaction as JSON data first
			string blk = Serialize.SerializeBlock(chain.ElementAt(blockRequest.Item2));

			// Broadcast to network via TCP
			NetworkComms.SendObject("SendBlock", blockRequest.Item1, network.serverPort, blk);

			// Show confirmation
			//MessageBox.Show("Block content sent: \n\n" + blk);
		}

		private void SendBlock(Block block)
		{
			// Serialize transaction as JSON data first
			string blk = Serialize.SerializeBlock(block);

			// Send Block
			network.BroadcastBlock(blk);

			// Show confirmation
			//MessageBox.Show("Block content sent: \n\n" + blk);
		}

		// Receive from network - add transaction to currentBlock
		private void ReceiveTransaction(PacketHeader packetHeader, Connection connection, string incomingObject)
		{
			// Deserialize JSON data that is received
			Transaction transaction = Serialize.DeserializeTransaction(incomingObject);

			// Testing
			//MessageBox.Show("Transaction Received with TXID: \n\n" + transaction.txid);

			// Verify data signature and account balance
			Tuple<bool, string> result = verifyTransaction(transaction);

			if (result.Item1)
			{
				// Add transaction to current block
				currentBlock.AddTransaction(transaction, chain);
			}
		}

		private void ReceiveBlock(PacketHeader packetHeader, Connection connection, string blk)
		{
			// Deserialize JSON data that is received
			Block block = Serialize.DeserializeBlock(blk);

			// Testing
			//MessageBox.Show("Block Received with Hash: \n\n" + block.HashBlock());

			// TODO: If it is the same height, disregard.
			if (block.getIndex() == blockHeight - 1)
			{
				//MessageBox.Show("Block received has same height as current block, disregarding...");
			}
			else if(block.getIndex() > blockHeight)
			{
				//MessageBox.Show("Block received is more than one block ahead. Finish syncing blockchain...");
				// Load method to sync missing blocks
                // Request block from random peer
			}
			else if(block.getIndex() == blockHeight)
			{
				if(block.previousHash == previousBlock.HashBlock())
				{
					Serialize.WriteBlock(block);
					chain.Add(block);
					blockHeight++;
					network.blockheight = blockHeight;
					previousBlock = chain.ElementAt(blockHeight - 1);
					SendBlock(block);
					currentBlock = new Block();
					currentBlock.NewBlock(blockHeight, previousBlock.HashBlock());
				}
				else
				{
                    // Block not valid
					//MessageBox.Show("Hashes do not match, disregarding block...");
				}				
			}			
		}

        public void SyncBlockchain()
        {            
            for (int i = blockHeight; i < network.longestChain.Item1; i++)
            {                
                if(network.RequestBlock(i))
                {
                    //MessageBox.Show("Syncing Blockchain... Block: " + i);
                }
                else
                {
                    //MessageBox.Show("Syncing Blockchain Failed");
                    break;
                }
            }
        }
		#endregion

		/// <summary>
		/// Attempts to find the correct hash for the current block.
		/// </summary>
		/// <param name="miningWindow">Shows the mining window if set to true.</param>
		/// <returns>Returns true if this user successfully mined the block.</returns>
		public async void MineBlock()
		{
			// Asks for address to collect mining rewards. Defaults to my address if cancelled.
			if (miner_id == "")
			{
				miner_id = Microsoft.VisualBasic.Interaction.InputBox("Enter the public key that you would like your mining rewards sent to.", "Mining Rewards", 
					"BgIAAACkAABSU0ExAAQAAAEAAQA/KlTgRpoq4gx6RQFiPz+1FAEq5VOUZrfOWJ593nwm7gjsV6x+uxEJRScSLOmBda2PEmVTlFim2Y/Aund29KDXryz16sl6719hR3qZVUkEohAbvPayDJ7r70EAM+oGAU8KiPnyoQqF6bLexEE9yXtA39q94KTPMC4wT7jIhi1E9Q==");
			}

			int count = 0;
			int count2 = 0;
			string hashString = "1234567890";			

			DateTime start = DateTime.Now;
			DateTime previous = DateTime.Now;

			mw.ConsoleOutput.Document.Blocks.Clear();
			mw.speedWindow.Clear();

			await Task.Run(() =>
			{
				while (hashString.Substring(0, 5) != "00")
				{
					if (mining == false)
					{
						break;
					}

					byte[] hash;
					string temp;
					SHA256Managed hasher = new SHA256Managed();
					temp = count.ToString() + previousBlock.index.ToString() + previousBlock.getTimeString() + previousBlock.data.ToString() + previousBlock.previousHash;
					Byte[] byteArray = Encoding.UTF8.GetBytes(temp);
					hash = hasher.ComputeHash(byteArray);

					hashString = string.Empty;

					foreach (byte x in hash)
					{
						hashString += String.Format("{0:x2}", x);
					}

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

					count++;
					count2++;
				}
			});
			
			if (mining == true)
			{
				mw.Dispatcher.Invoke(DispatcherPriority.Render, new Action(() =>
				{
					mw.ConsoleOutput.AppendText(Environment.NewLine + count.ToString() + " - " + hashString);
					mw.ConsoleOutput.ScrollToEnd();
				}));

				DateTime finish = DateTime.Now;
				Double duration = finish.Subtract(start).TotalSeconds;

				mw.ConsoleOutput.AppendText(Environment.NewLine + "Block time: " + duration.ToString() + " Seconds. Average speed: " + ((count / duration) / 1000).ToString("N2") + " KH/sec.");

				// Include mining reward in block
				Transaction reward = new Transaction("miningReward", miner_id, MiningReward(), HashCount(count));
				currentBlock.AddTransaction(reward,chain);

				// Add block to chain
				Serialize.WriteBlock(currentBlock);
				chain.Add(currentBlock);
				blockHeight++;
				network.blockheight = blockHeight;
				previousBlock = chain.ElementAt(blockHeight - 1);

				// Broadcast Block
				SendBlock(currentBlock);

				// Create new block
				currentBlock = new Block();
				currentBlock.NewBlock(blockHeight, previousBlock.HashBlock());				

				// Start mining again
				MineBlock();
			}
			
		}

		private double MiningReward()
		{
			return 1.0;
		}

		// Hashes the nonce
		public string HashCount(int count)
		{
			byte[] hash;
			SHA256Managed hasher = new SHA256Managed();
			Byte[] byteArray = Encoding.UTF8.GetBytes(count.ToString());
			hash = hasher.ComputeHash(byteArray);
			string hashString = string.Empty;
			foreach (byte x in hash)
			{
				hashString += String.Format("{0:x2}", x);
			}
			return hashString;
		}

		public static void SaveBlock(Block b)
		{
			Serialize.WriteBlock(b);
		}
	}
}
