using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using NetworkCommsDotNet;
using System.Net;
using System.Net.Sockets;
using System.Windows;
using Newtonsoft.Json;
using NetworkCommsDotNet.Connections;
using NetworkCommsDotNet.Connections.TCP;
using NetworkCommsDotNet.Connections.UDP;

namespace Blockchain{

	// TODO: use a temporary peers list to add new connections
	// Merge with the real list on a regular basis, prevent list from being modified during access
	// Add remove duplicates method
	// maybe check if exists before calling add method

	public class Network
	{
		public Peers peers;
		private List<Peers> to_merge = new List<Peers>();
		private List<Peers> merging_now = new List<Peers>();
		public int connectedPeers = 0;
		static Timer timer;
		const int serverPort = 9999;
		Random rnd = new Random();

		public Network()
		{
			// Load peers from JSON file
			LoadPeers();

			// Add self to peers list
			AddSelf();

			//MessageBox.Show(Serialize.SerializePeers(peers));

			// Assign local IP address
			//peers = new Peers {	ip_address = CurrentIPAddress()	};

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
			CheckPeersOnline();
			PeerDiscovery();
			TransferMerged();
			MergePeers();
			SortPeers();
			AdjustTimer();
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

		public void MergePeers()
		{
			if(peers != null)
			{
				try
				{
					// iterate through each of the new peer lists
					foreach (Peers mn in merging_now)
					{
						foreach(Peer np in mn.peers_list)
						{							
							foreach (Peer p in peers.peers_list)
							{
								bool found = false;
								// compare to what is in current list
								if (p.ip_address == np.ip_address)
								{
									// flag the ip_address as duplicate
									found = true;

									MessageBox.Show("duplicate");

									// Update last_seen datetime to latest value
									if (DateTime.Compare(np.last_seen, p.last_seen) > 0)
									{
										p.last_seen = np.last_seen;
									}
								}
								if (!found)
								{
									MessageBox.Show("new ip");
									// add to the list if not found
									peers.peers_list.Add(np);
								}
							}
						}						
					}
				}
				catch (Exception)
				{
					// Collection may have been modified
				}
			}			
		}

		private void CheckPeersOnline()
		{
			if (peers != null)
			{
				try
				{
					foreach (Peer peer in peers.peers_list)
					{
						ConnectionInfo connInfo = new ConnectionInfo(peer.ip_address, serverPort);

						if (peer.ip_address != CurrentIPAddress())
						{
							try
							{
								peer.conn = TCPConnection.GetConnection(connInfo);

								if (peer.conn.ConnectionAlive(100))
								{
									MessageBox.Show("Connection to " + peer.ip_address.ToString() + " is alive.");
									peer.last_seen = DateTime.UtcNow;
									peer.connected = true;
								}
								else
								{
									peer.connected = false;
								}
							}
							catch (Exception)
							{
								// Error connecting
							}

						}
					}
				}
				catch (Exception)
				{
					// Collection may have been modified
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
					if (peer.connected)
					{
						count++;
					}
				}
			}
			return count - 1;
		}

		public void PeerDiscovery()
		{
			// Announces IP address over UDP, expect nodes to respond with list of peers
			UDPConnection.SendObject("Announce", CurrentIPAddress(), new IPEndPoint(IPAddress.Broadcast, 10000));
			//NetworkComms.SendObject("PeerList", "192.168.1.249", serverPort, Serialize.SerializePeers(peers));
			// the more peers are connected, the less frequent the announces
		}

		private void Announce(PacketHeader packetHeader, Connection connection, string sender_ip)
		{			
			// Check to make sure message has not come from this computer
			if (sender_ip != CurrentIPAddress() && sender_ip != "127.0.0.1")
			{
				//MessageBox.Show("Sending message to: " + sender_ip);
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

		public string CurrentIPAddress()
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

		public void SendTransaction(string tx)
		{
			// Send transaction to 5 random peers
			if(peers != null)
			{
				Peers random_peers = new Peers();
				random_peers.peers_list = (List<Peer>)peers.peers_list.OrderBy(x => rnd.Next()).Take(5);
				foreach (Peer peer in random_peers.peers_list)
				{
					NetworkComms.SendObject("SendTransaction", peer.ip_address, serverPort, tx);
				}
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

		[JsonIgnoreAttribute]
		public Connection conn;

		[JsonIgnoreAttribute]
		public bool connected;

		[JsonIgnoreAttribute]
		public int blockheight;

		public Peer()
		{

		}

	}
}
