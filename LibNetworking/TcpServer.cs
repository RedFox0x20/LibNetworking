using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace LibNetworking
{
	public class TcpServer
	{
		public TcpListener _Listener;
		int _Port;
		bool _Running;

		List<TcpClient> _ConnectedClients;
		public int _ConnectedClientCount { get { return _ConnectedClients.Count; } }
		public TcpClient[] GetClients()
		{
			return _ConnectedClients == null ? null : _ConnectedClients.ToArray();
		}
		Thread _ListenForClientsThread;
		
		public event ConnectionEvent OnClientConnect, OnClientDisconnect;
		public event MessageEvent OnMessageSent, OnMessageRecieved;

		public TcpServer(int Port)
		{
			_Port = Port;
			_ConnectedClients = new List<TcpClient>();
		}

		public void Start()
		{
			if (_Running) { return; }
			_Running = true;
			_Listener = new TcpListener(IPAddress.Any, _Port);
			_Listener.Start();

			_ListenForClientsThread = new Thread(ListenForClients);
			_ListenForClientsThread.Name = "SV_ListenForClientsThread";
			_ListenForClientsThread.IsBackground = true;
			_ListenForClientsThread.Start();
		}

		public void Restart()
		{
			Stop();
			Start();
		}

		public void Stop()
		{
			if (!_Running) { return; }
			_Running = false;
			SendMessageToAllClients("DISCONNECT");
			// Complains due to the modification of the List<TcpClient>
			//_ConnectedClients.ForEach(client => { client.Disconnect(); });
			//_ConnectedClients.Clear();
			_Listener.Stop();
		}

		public void ListenForClients()
		{
			TcpClient ConnectingClient;
			/* Unfortunately the TryCatch is a must so that no errors occour when the listener is closed as AcceptTcpClient is a blocking call */
			try
			{
				while (_Running)
				{
					#if DEBUG
					Console.WriteLine("Wait for client!");
					#endif	
					ConnectingClient = new TcpClient(_ConnectedClientCount, _Listener.AcceptTcpClient());
					ConnectingClient.OnConnect += OnClientConnect;
					ConnectingClient.OnDisconnect += (TcpClient Client) => { _ConnectedClients.Remove(Client); OnClientDisconnect(Client); };
					ConnectingClient.OnMessageRecieved += OnMessageRecieved;
					ConnectingClient.OnMessageSent += OnMessageSent;

					_ConnectedClients.Add(ConnectingClient);
					OnClientConnect(ConnectingClient);
				}
			}
			catch (Exception ex) { }
		}

		public void SendMessageToAllClients(string Message)
		{
			List<TcpClient> ClientsToRemove = new List<TcpClient>();
			foreach (TcpClient Client in _ConnectedClients)
			{
				if (!Client.Send(Message))
				{
					ClientsToRemove.Add(Client);
				}
			}
			foreach (TcpClient Client in ClientsToRemove)
			{
				_ConnectedClients.Remove(Client);
			}
		}

		public void SendMessageToClientWithID(int ID, string Message)
		{
			TcpClient Client = _ConnectedClients.First(x => x._ID == ID);
			if (Client == null) { return; }
			if (!Client.Send(Message))
			{ 
				_ConnectedClients.Remove(Client);
			}
		}

		public void SendMessageToClientsWithIDInList(int[] IDList, string Message)
		{
			List<TcpClient> ClientsToRemove = new List<TcpClient>();
			List<TcpClient> ClientsToMessage = _ConnectedClients.FindAll(x => IDList.Contains(x._ID));
			foreach (TcpClient Client in ClientsToMessage)
			{
				if (!Client.Send(Message))
				{
					ClientsToRemove.Add(Client);
				}
			}
			foreach (TcpClient Client in ClientsToRemove)
			{
				_ConnectedClients.Remove(Client);
			}
		}

		public void SendMessageToClientsInRange(int Min, int Max, string Message)
		{
			List<TcpClient> ClientsToRemove = new List<TcpClient>();
			List<TcpClient> ClientsToMessage = _ConnectedClients.FindAll(x => x._ID >= Min && x._ID <= Max);
			foreach (TcpClient Client in ClientsToMessage)
			{
				if (!Client.Send(Message))
				{
					ClientsToRemove.Add(Client);
				}
			}
			foreach (TcpClient Client in ClientsToRemove)
			{
				_ConnectedClients.Remove(Client);
			}
		}
	}
}
