using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibNetworking
{
	public delegate void MessageEvent(TcpClient Client, byte[] RawData, string MessageData);
	public delegate void ConnectionEvent(TcpClient Client);
}
