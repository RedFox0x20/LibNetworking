using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LibNetworking;

namespace Testing
{
	class Program
	{
		TcpServer Sv;
		TcpClient Cl, Cl2;

		void Run()
		{
			Sv = new TcpServer(6667);
			Sv.OnClientConnect += Sv_OnClientConnect;
			Sv.OnClientDisconnect += Sv_OnClientDisconnect;
			Sv.OnMessageRecieved += Sv_OnMessageRecieved;
			Sv.OnMessageSent += Sv_OnMessageSent;
			Sv.Start();

			Cl = new TcpClient("127.0.0.1", 6667);
			Cl.OnConnect += Cl_OnConnect;
			Cl.OnDisconnect += Cl_OnDisconnect;
			Cl.OnMessageRecieved += Cl_OnMessageRecieved;
			Cl.OnMessageSent += Cl_OnMessageSent;

			Cl2 = new TcpClient("127.0.0.1", 6667);
			Cl2.OnConnect += Cl_OnConnect;
			Cl2.OnDisconnect += Cl_OnDisconnect;
			Cl2.OnMessageRecieved += Cl_OnMessageRecieved;
			Cl2.OnMessageSent += Cl_OnMessageSent;

			Cl.Connect();
			Cl.Send("Pie");
			Sv.SendMessageToAllClients("No pie!");

			Cl2.Connect();
			Cl2.Send("Pie2");
			Sv.SendMessageToAllClients("No pie2!");

			Cl.Disconnect();
			Sv.SendMessageToAllClients("Aft DC");
			Cl2.Disconnect();
			Sv.Stop();
			Console.ReadKey();
		}

		private void Cl_OnMessageSent(TcpClient Client, byte[] RawData, string MessageData)
		{
			Console.WriteLine("[CLIENT] Sent> " + MessageData);
		}

		private void Cl_OnMessageRecieved(TcpClient Client, byte[] RawData, string MessageData)
		{
			Console.WriteLine("[CLIENT] Recieved> " + MessageData);
		}

		private void Cl_OnDisconnect(TcpClient Client)
		{
			Console.WriteLine("[CLIENT] Disconnected");
		}

		private void Cl_OnConnect(TcpClient Client)
		{
			Console.WriteLine("[CLIENT] Connected");
		}

		private void Sv_OnMessageSent(TcpClient Client, byte[] RawData, string MessageData)
		{
			Console.WriteLine("[SERVER] Message sent> " + MessageData);
		}

		private void Sv_OnMessageRecieved(TcpClient Client, byte[] RawData, string MessageData)
		{
			Console.WriteLine("[SERVER] Recieved> " + MessageData);
		}

		private void Sv_OnClientDisconnect(TcpClient Client)
		{
			Console.WriteLine("[SERVER] Client disconnected!");
		}

		private void Sv_OnClientConnect(TcpClient Client)
		{
			Console.WriteLine("[SERVER] Client connected!");
		}

		static void Main(string[] args) => new Program().Run();
	}
}
