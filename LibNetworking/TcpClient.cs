using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;

namespace LibNetworking
{
	public class TcpClient
	{
		private const int SizeOfInt32 = 4;

		System.Net.Sockets.TcpClient _Client;
		string _Hostname;
		int _Port;
		public readonly int _ID;

		public bool _Connected { get { return _Client == null ? false : _Client.Connected; } }
		NetworkStream _Stream { get { return _Client == null ? null : _Client.GetStream(); } }

		Thread _ListenThread;
		public Thread GetListenThread() { return _ListenThread; }

		public event ConnectionEvent OnConnect, OnDisconnect;
		public event MessageEvent OnMessageSent, OnMessageRecieved;

		public string ClientInfoString;

		public TcpClient(string Hostname, int Port, int ID = -1)
		{
			_Client = new System.Net.Sockets.TcpClient();
			_Hostname = Hostname;
			_Port = Port;
			_ID = ID;

			_ListenThread = new Thread(Listen);
			_ListenThread.IsBackground = true;
		}
		
		public TcpClient(int ID, System.Net.Sockets.TcpClient Client)
		{
			_Client = Client;
			_Hostname = _Client.Client.RemoteEndPoint.ToString().Split(':')[0];
			_Port = Convert.ToInt32(_Client.Client.RemoteEndPoint.ToString().Split(':')[1]);
			_ID = ID;

			_ListenThread = new Thread(Listen);
			_ListenThread.IsBackground = true;
			_ListenThread.Start();
		}

		public bool Connect()
		{
			if (_Connected) { return true; }
			try
			{
				_Client.Connect(_Hostname, _Port);
				_ListenThread.Start();
				OnConnect(this);
				return true;
			} catch { return false; }
		}

		public void Reconnect()
		{
			Connect();
			Disconnect();
		}

		public void Disconnect()
		{
			if (!_Connected) { return; }
			_Client.Close();
			OnDisconnect(this);
		}

		void Listen()
		{
			byte[] SizeDataBytes = new byte[SizeOfInt32];
			int Size;
			byte[] MainDataBytes;
			string MainDataString;

			while (_Connected)
			{
				if (_Stream.DataAvailable)
				{
					// Get size
					SizeDataBytes = new byte[SizeOfInt32];
					_Stream.Read(SizeDataBytes, 0, SizeOfInt32);
					Size = BitConverter.ToInt32(SizeDataBytes, 0);
					// Read primary data
					MainDataBytes = new byte[Size];
					_Stream.Read(MainDataBytes, 0, Size);
					MainDataString = Encoding.ASCII.GetString(MainDataBytes);

					if (MainDataString == "IMAGE")
					{
						// Get size
						SizeDataBytes = new byte[SizeOfInt32];
						_Stream.Read(SizeDataBytes, 0, SizeOfInt32);
						Size = BitConverter.ToInt32(SizeDataBytes, 0);
						// Read primary data
						MainDataBytes = new byte[Size];
						_Stream.Read(MainDataBytes, 0, Size);
						MainDataString = "IMAGE";
					}

					OnMessageRecieved(this, MainDataBytes, MainDataString);
				}
				else 
				{
					Thread.Sleep(1000 / 60);
				}
			}
		}
		
		public bool Send(string Message)
		{
			if (!_Connected) { return false; }
			/* TryCatch is a must so that in the event that the stream has been closed within the same time period no errors will be thrown */
			try
			{
				byte[] DataBytes = Encoding.ASCII.GetBytes(Message);
				_Stream.Write(BitConverter.GetBytes(DataBytes.Length), 0, 4);
				_Stream.Write(DataBytes, 0, DataBytes.Length);	
				OnMessageSent(this, DataBytes, Message);
				return true;
			}
			catch { Disconnect(); return false; }
		}

		public bool SendImage(string ImagePath)
		{
			if (!_Connected) { return false; }
			try
			{
				byte[] ReadBytes = File.ReadAllBytes(ImagePath);
				Send("IMAGE");
				_Stream.Write(BitConverter.GetBytes(ReadBytes.Length), 0, 4);
				_Stream.Write(ReadBytes, 0, ReadBytes.Length);

				return false;
			}
			catch { Disconnect(); return false; }
		}

		public override string ToString()
		{
			return $"CLIENT:ID={_ID}:CONNECTED={_Connected}:HOSTNAME={_Hostname}:PORT={_Port}";
		}
	}
}
