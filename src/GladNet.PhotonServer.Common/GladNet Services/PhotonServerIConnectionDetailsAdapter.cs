using GladNet.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace GladNet.PhotonServer.Server
{
	/// <summary>
	/// Adapter for the <see cref="IConnectionDetails"/> interface.
	/// </summary>
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

		/// <summary>
		/// Creates a new adapter for the <see cref="IConnectionDetails"/> interface.
		/// </summary>
		/// <param name="remoteIP">Remote IP address of the connection.</param>
		/// <param name="remotePort">Remote port of the connection.</param>
		/// <param name="localPort">Local port of the connection.</param>
		/// <param name="connectionID">Unique (port-wise) ID of the connection.</param>
		public PhotonServerIConnectionDetailsAdapter(string remoteIP, int remotePort, int localPort, int connectionID)
		{
			RemoteIP = IPAddress.Parse(remoteIP);
			RemotePort = remotePort;
			LocalPort = localPort;
			ConnectionID = connectionID;
		}
	}
}
