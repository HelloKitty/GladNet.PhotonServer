using GladNet.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace GladNet.PhotonServer.Server
{
	public class PhotonServerIConnectionDetailsAdapter : IConnectionDetails
	{
		/// <summary>
		/// IPAddress of the remote peer.
		/// </summary>
		public IPAddress RemoteIP { get; }

		/// <summary>
		/// Remote port of the peer.
		/// </summary>
		public int RemotePort { get; }

		/// <summary>
		/// Local port the peer is connecting on.
		/// </summary>
		public int LocalPort { get; }

		/// <summary>
		/// Connection ID of the peer. (unique per port)
		/// </summary>
		public int ConnectionID { get; }

		public PhotonServerIConnectionDetailsAdapter(string ip, int remotePort, int localPort, int connectionID)
		{
			RemoteIP = IPAddress.Parse(ip);
			RemotePort = remotePort;
			LocalPort = localPort;
			ConnectionID = connectionID;
		}
	}
}
