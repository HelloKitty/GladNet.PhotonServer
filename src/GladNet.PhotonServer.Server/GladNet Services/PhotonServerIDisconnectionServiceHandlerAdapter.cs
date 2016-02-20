using GladNet.Common;
using Photon.SocketServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GladNet.PhotonServer.Server
{
	public class PhotonServerIDisconnectionServiceHandlerAdapter : IDisconnectionServiceHandler
	{
		public bool isDisconnected { get; private set; }

		public event OnNetworkDisconnect DisconnectionEventHandler;

		public PhotonServerIDisconnectionServiceHandlerAdapter()
		{
			isDisconnected = false;
		}

		public void Disconnect()
		{
			isDisconnected = true;

			//Call subscribers for the disconnection event.
			if (DisconnectionEventHandler != null)
				DisconnectionEventHandler.Invoke();
		}
	}
}
