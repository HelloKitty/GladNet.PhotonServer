using GladNet.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using GladNet.Engine.Common;
using System.Threading;

namespace GladNet.PhotonServer.Server
{
	/// <summary>
	/// Adapter for the <see cref="IConnectionDetails"/> interface.
	/// </summary>
	public class PhotonServerIConnectionDetailsAdapter : IConnectionDetails
	{
		//We need AUIDs because of GladNet2 routing specification: https://github.com/HelloKitty/GladNet2.Specifications/blob/master/Routing/RoutingSpecification.md
		/// <summary>
		/// Represents static AUID counter value for connections.
		/// </summary>
		private static int internalAUIDCounter = 0;

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
		/// AUID of the peer. (Application-wide unique)
		/// </summary>
		public int ConnectionID { get; }

		/// <summary>
		/// Creates a new adapter for the <see cref="IConnectionDetails"/> interface.
		/// </summary>
		/// <param name="remoteIP">Remote IP address of the connection.</param>
		/// <param name="remotePort">Remote port of the connection.</param>
		/// <param name="localPort">Local port of the connection.</param>
		/// <param name="connectionID">Unique (port-wise) ID of the connection.</param>
		public PhotonServerIConnectionDetailsAdapter(string remoteIP, int remotePort, int localPort)
		{
			RemoteIP = IPAddress.Parse(remoteIP);
			RemotePort = remotePort;
			LocalPort = localPort;

			//We need to manually control AUID assingment in Photon because PhotonServer creates UIDs for connections
			//unique PER PORT when we need PER APPLICATION.
			//We need AUIDs because of GladNet2 routing specification: https://github.com/HelloKitty/GladNet2.Specifications/blob/master/Routing/RoutingSpecification.md
			ConnectionID = Interlocked.Increment(ref internalAUIDCounter);
		}
	}
}
