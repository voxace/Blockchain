using NetworkCommsDotNet.Connections;
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
using NetworkCommsDotNet.Connections.TCP;
using NetworkCommsDotNet.Connections.UDP;

namespace Blockchain
{

	public class Network
	{
		public Peers peers = new Peers();
		public int connectedPeers = 0;
		static Timer timer;
		const int serverPort = 9999;
		List<ClientInfo> clients = new List<ClientInfo>();
		Random rnd = new Random();

		public class ClientInfo
		{
			public string ip_address;
			public int port;
			
		}

		public Network()
		{
			// Load peers from JSON file
			LoadPeers();

			// Start listening for UDP packets
			Connection.StartListening(ConnectionType.UDP, new IPEndPoint(IPAddress.Any, 8888));

			//Trigger the correct method when a certain packet type is received
			NetworkComms.AppendGlobalIncomingPacketHandler<string>("Announce", AnnounceReply);

			// Timer for main method
			timer = new Timer(5000);
			timer.Elapsed += new ElapsedEventHandler(Main);
			timer.Enabled = true;
		}		

		private void Main(object sender, ElapsedEventArgs e)
		{
			CheckPeersOnline();
			connectedPeers = CountPeersOnline();
		}

		private void CheckPeersOnline()
		{
			if (peers != null)
			{
				foreach (Peer peer in peers.peers_list)
				{
					ConnectionInfo connInfo = new ConnectionInfo(peer.ip_address, serverPort);
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
			}
			
		}

		private void LoadPeers()
		{
			peers = Serialize.ReadPeers();
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
			return count;
		}

		public void PeerDiscovery()
		{
			// Announces IP address over UDP, expect nodes to respond with list of peers
			UDPConnection.SendObject("Announce", CurrentIPAddress(), new IPEndPoint(IPAddress.Broadcast, 8888));

			// change to always annouce peer list, even if empty
			// the more peers are connected, the less frequent the announces
		}

		private void AnnounceReply(PacketHeader packetHeader, Connection connection, string incomingObject)
		{
			// Check to make sure message has not come from this computer
			if(CurrentIPAddress() != incomingObject)
			{
				// Sends TCP Packet containing list of peers in JSON format to IP address that announced itself
				NetworkComms.SendObject("PeerList", incomingObject, serverPort, Serialize.SerializePeers(peers));
			}	
			// merge peer list from sender here
		}

		public void SortPeers()
		{
			// Sort list by order of last connection
			peers.peers_list.Sort((a, b) => a.last_seen.CompareTo(b.last_seen));
		}

		public string CurrentIPAddress()
		{
			var host = Dns.GetHostEntry(Dns.GetHostName());
			foreach (var ip in host.AddressList)
			{
				if (ip.AddressFamily == AddressFamily.InterNetwork)
				{
					MessageBox.Show(ip.ToString());
					return ip.ToString();
				}
			}
			return string.Empty;
		}

		public void SendTransaction(string tx)
		{
			// Send transaction to 5 random peers
			Peers random_peers = new Peers();
			random_peers.peers_list = (List<Peer>)peers.peers_list.OrderBy(x => rnd.Next()).Take(5);
			foreach(Peer peer in random_peers.peers_list)
			{
				NetworkComms.SendObject("SendTransaction", peer.ip_address, serverPort, tx);
			}			
		}
	}

	public class Peers
	{
		public List<Peer> peers_list = new List<Peer>();
	}

	public class Peer
	{
		public string ip_address;
		public DateTime last_seen;

		[JsonIgnoreAttribute]
		public Connection conn;
		public bool connected;

		public Peer()
		{

		}

	}
}
