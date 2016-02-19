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
		public IPAddress RemoteIP { get; private set; }

		/// <summary>
		/// Remote port of the peer.
		/// </summary>
		public int RemotePort { get; private set; }

		/// <summary>
		/// Local port the peer is connecting on.
		/// </summary>
		public int LocalPort { get; private set; }

		/// <summary>
		/// Connection ID of the peer. (unique per port)
		/// </summary>
		public int ConnectionID { get; private set; }
	}
}
