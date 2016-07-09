using GladNet.Common;
using GladNet.Engine.Common;
using Photon.SocketServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GladNet.PhotonServer.Server
{
	/// <summary>
	/// Adapter for the <see cref="IDisconnectionServiceHandler"/> interface.
	/// </summary>
	public class PhotonServerIDisconnectionServiceHandlerAdapter : IDisconnectionServiceHandler
	{
		/// <summary>
		/// Indicates if the connection is disconnected.
		/// </summary>
		public bool isDisconnected { get; private set; }

		/// <summary>
		/// Publisher of <see cref="OnNetworkDisconnect"/> events to subscribers.
		/// </summary>
		public event OnNetworkDisconnect DisconnectionEventHandler;

		/// <summary>
		/// Creates a new adapter for the <see cref="IDisconnectionServiceHandler"/> interface.
		/// </summary>
		public PhotonServerIDisconnectionServiceHandlerAdapter()
		{
			isDisconnected = false;
		}

		/// <summary>
		/// Disconnects the connection.
		/// </summary>
		public void Disconnect()
		{
			isDisconnected = true;

			//Call subscribers for the disconnection event.
			if (DisconnectionEventHandler != null)
				DisconnectionEventHandler.Invoke();
		}
	}
}
