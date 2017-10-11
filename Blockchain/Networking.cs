using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using NetworkCommsDotNet;
using System.Net;
using System.Net.Sockets;
using System.Windows;
using Newtonsoft.Json;
using NetworkCommsDotNet.Connections;
using NetworkCommsDotNet.Connections.TCP;
using NetworkCommsDotNet.Connections.UDP;
using System.Net.NetworkInformation;

namespace Blockchain
{
	// TODO: Get Unconfirmed transactions
	// Request list of unconfirmed transactions in current block on load from random peer
	// Send current block over via TCP to requesting address
	// Set current_block to received block

	public class Network
	{
		public Peers peers;
		private List<Peers> to_merge = new List<Peers>();
		private List<Peers> merging_now = new List<Peers>();
		public int connectedPeers = 0;
		static Timer timer;
		public int blockheight;
        public Tuple<int, string> longestChain;
		public int serverPort = 9999;
		Random rnd = new Random();

		public Network()
		{
			// Load peers from JSON file
			LoadPeers();

			// Add self to peers list
			AddSelf();

            // Assume current node is longest chain for now
            longestChain = new Tuple<int, string>(blockheight, CurrentIPAddress());

            // Trigger the correct method when a certain packet type is received
            NetworkComms.AppendGlobalIncomingPacketHandler<string>("Announce", Announce);

			// Start listening for UDP packets
			Connection.StartListening(ConnectionType.UDP, new IPEndPoint(IPAddress.Any, 10000));

			// Timer for main method
			timer = new Timer(5000);
			timer.Elapsed += new ElapsedEventHandler(Main);
			timer.Enabled = true;
		}		

		private void Main(object sender, ElapsedEventArgs e)
		{
			timer.Stop();			
			PeerDiscovery();
			UpdateSelf();
			TransferMerged();
			MergePeers();
			SortPeers();
			CheckPeersOnline();
			GetMaxBlockHeight();
			AdjustTimer();
			SavePeers();
			timer.Start();
		}

		public void GetMaxBlockHeight()
		{
			//Returns the highest blockheight and ip address of node
			int height = 0;
			string ip = "";

			foreach (Peer peer in peers.peers_list)
			{
				if (peer.blockheight > height)
				{
					height = peer.blockheight;
					ip = peer.ip_address;
				}
			}
			longestChain = new Tuple<int, string>(height, ip);
		}

		private void TransferMerged()
		{
			merging_now = to_merge;
			to_merge = new List<Peers>();
		}

		private void AdjustTimer()
		{
			if(connectedPeers < 1)
			{
				timer.Interval = 5000;
			}
			else if(connectedPeers < 5)
			{
				timer.Interval = 10000;
			}
			else
			{
				timer.Interval = 30000;
			}
		}

		public void SavePeers()
		{
			Serialize.WritePeers(peers);
		}

		public void NewPeers(Peers new_peers)
		{
			to_merge.Add(new_peers);
		}

		public void AddSelf()
		{			
			if(peers != null)
			{
				bool found = false;
				foreach (Peer p in peers.peers_list)
				{
					if (p.ip_address == CurrentIPAddress())
					{
						p.last_seen = DateTime.UtcNow;
						p.connected = false;
						found = true;
					}
				}
				if (found == false)
				{
					peers.peers_list.Add(new Peer { ip_address = CurrentIPAddress(), last_seen = DateTime.UtcNow });
				}
			}
			else
			{
				// Create and fill new Peers class
				Peer temp = new Peer { ip_address = CurrentIPAddress(), last_seen = DateTime.UtcNow, connected=true };
				List<Peer> temp_list = new List<Peer>();
				temp_list.Add(temp);
				peers = new Peers { ip_address = CurrentIPAddress(), peers_list = temp_list };
			}			
		}

		public void UpdateSelf()
		{
			var dict = peers.peers_list.ToDictionary(p => p.ip_address);
			dict[CurrentIPAddress()].last_seen = DateTime.UtcNow;
			dict[CurrentIPAddress()].connected = true;
			dict[CurrentIPAddress()].blockheight = blockheight;
			peers.peers_list = dict.Values.ToList();
		}
		
		public void MergePeers()
		{
			foreach (Peers mn in merging_now)
			{
				//MessageBox.Show("Old: " + Serialize.SerializePeers(mn));
				var dict = mn.peers_list.ToDictionary(p => p.ip_address);
				foreach (var old_peers in peers.peers_list)
				{
					// create peer if it doesn't exist, otherwise overwrite
					dict[old_peers.ip_address] = old_peers;

					// set default connected state to true
					dict[old_peers.ip_address].connected = true;
				}
				peers.peers_list = dict.Values.ToList();
				//MessageBox.Show("New: " + Serialize.SerializePeers(peers));
			}
		}

		private void CheckPeersOnline()
		{
			if (peers != null)
			{
				foreach (Peer peer in peers.peers_list)
				{
					ConnectionInfo connInfo = new ConnectionInfo(peer.ip_address, serverPort);

					if (peer.ip_address != CurrentIPAddress() && peer.ip_address != "127.0.0.1")
					{
						try
						{
							peer.conn = TCPConnection.GetConnection(connInfo,true);							

							if (peer.conn.ConnectionAlive(100))
							{
								peer.last_seen = DateTime.UtcNow;
								peer.connected = true;
							}
						}
						catch (Exception ex)
						{
							Console.WriteLine(ex);
							peer.connected = false;
						}
					}
					else
					{
						peer.connected = true;
						peer.last_seen = DateTime.UtcNow;
					}
				}
			}
			connectedPeers = CountPeersOnline();
		}

		private void LoadPeers()
		{
			NewPeers(Serialize.ReadPeers());
		}

		private int CountPeersOnline()
		{
			int count = 0;
			if (peers != null)
			{
				foreach (Peer peer in peers.peers_list)
				{
					if (peer.connected && peer.ip_address != CurrentIPAddress())
					{
						count++;
					}
				}
			}
			return count;
		}

		public void PeerDiscovery()
		{
			// Announces IP address over UDP, expect nodes to respond with list of peers
			UDPConnection.SendObject("Announce", CurrentIPAddress(), new IPEndPoint(IPAddress.Broadcast, 10000));
		}

		private void Announce(PacketHeader packetHeader, Connection connection, string sender_ip)
		{			
			// Check to make sure message has not come from this computer
			if (sender_ip != CurrentIPAddress() && sender_ip != "127.0.0.1")
			{
				// Sends TCP Packet containing list of peers in JSON format to IP address that announced itself
				try
				{
					NetworkComms.SendObject("PeerList", sender_ip, serverPort, Serialize.SerializePeers(peers));
				}
				catch (Exception)
				{
					// Peer offline
				}				
			}			
		}

		public static string GetIpAddress(Connection connection)
		{			
			string peer_connInfo = connection.ToString().Split(']').Last().Split('-').First().Trim();
			string peer_ip = peer_connInfo.Split(':').First();
			//MessageBox.Show("Connection Info: " + connection.ToString() + "\n\nIP Address: " + peer_ip);
			return peer_ip;
		}

		public void SortPeers()
		{
			// Sort list by order of last connection
			if(peers != null)
			{
				peers.peers_list.Sort((a, b) => a.last_seen.CompareTo(b.last_seen));
			}
			
		}

        public static string[] GetAllLocalIPv4(NetworkInterfaceType _type)
        {
            List<string> ipAddrList = new List<string>();
            foreach (NetworkInterface item in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (item.NetworkInterfaceType == _type && item.OperationalStatus == OperationalStatus.Up)
                {
                    foreach (UnicastIPAddressInformation ip in item.GetIPProperties().UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            ipAddrList.Add(ip.Address.ToString());
                        }
                    }
                }
            }
            return ipAddrList.ToArray();
        }

        public string CurrentIPAddress()
		{
            try
            {
                var host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        //MessageBox.Show(ip.ToString());
                        return ip.ToString();
                    }
                }
                return string.Empty;
            }
            catch (Exception)
            {                
                return GetAllLocalIPv4(NetworkInterfaceType.Ethernet).FirstOrDefault();
            }
            
		}

		public void BroadcastTransaction(string tx)
		{
			if(peers != null)
			{
				int count = 0;				
				foreach (Peer peer in peers.peers_list)
				{
					MessageBox.Show("Peer: " + peer.ip_address + "\nConnected: " + peer.connected.ToString());
					// Don't send to yourself
					if(peer.ip_address != CurrentIPAddress())
					{
						// Only send to connected peers
						if(peer.connected)
						{
							try
							{
								// Try sending the transaction
								NetworkComms.SendObject("SendTransaction", peer.ip_address, serverPort, tx);
								count++;
							}
							catch (Exception)
							{
								// If the transaction didn't go through, the peer is most likely offline
								peer.connected = false;
							}
							
						}						
					}					
				}
				MessageBox.Show("Transaction sent to " + count.ToString() + " peers.");
			}					
		}

		public void BroadcastBlock(string blk)
		{
			if (peers != null)
			{
				int count = 0;
				foreach (Peer peer in peers.peers_list)
				{
					MessageBox.Show("Peer: " + peer.ip_address + "\nConnected: " + peer.connected.ToString());
					// Don't send to yourself
					if (peer.ip_address != CurrentIPAddress())
					{
						// Only send to connected peers
						if (peer.connected)
						{
							try
							{
								// Try sending the block
								NetworkComms.SendObject("SendBlock", peer.ip_address, serverPort, blk);
								count++;
							}
							catch (Exception)
							{
								// If the transaction didn't go through, the peer is most likely offline
								peer.connected = false;
							}
						}
					}
				}
				MessageBox.Show("Block sent to " + count.ToString() + " peers.");
			}
			
		}

        public bool RequestBlock(int height)
        {
            List<Peer> onlinePeers = new List<Peer>();
            foreach (Peer peer in peers.peers_list)
            {
                if (peer.connected && peer.ip_address != CurrentIPAddress() && peer.blockheight > height)
                {
                    onlinePeers.Add(peer);
                }
            }
            if(onlinePeers.Count > 0)
            {
                Random rnd = new Random();
                Peer random_peer = onlinePeers.ElementAt(rnd.Next(onlinePeers.Count));

                MessageBox.Show("Requesting Block from " + random_peer.ip_address);
                NetworkComms.SendObject("SendBlock", random_peer.ip_address, serverPort, new Tuple<string, int>(CurrentIPAddress(), height));

                return true;
            }
            else
            {
                return false;
            }
            
        }
	}

	public class Peers
	{
		[JsonIgnoreAttribute]
		public string ip_address;		

		public List<Peer> peers_list = new List<Peer>();
	}

	public class Peer
	{
		public string ip_address;
		public DateTime last_seen;
		public int blockheight;

		[JsonIgnoreAttribute]
		public Connection conn;

		[JsonIgnoreAttribute]
		public bool connected;

		public Peer()
		{

		}

	}
}
