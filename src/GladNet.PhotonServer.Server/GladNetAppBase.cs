using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Photon.SocketServer;
using GladNet.Common;
using Logging.Services;
using GladNet.Server.Common;

namespace GladNet.PhotonServer.Server
{
	public abstract class GladNetAppBase : Photon.SocketServer.ApplicationBase
	{
		protected abstract ILogger AppLogger { get; set; }

		protected override PeerBase CreatePeer(InitRequest initRequest)
		{
			throw new NotImplementedException();
		}

		protected abstract bool ShouldIncomingPeerShouldConnect(INetworkMessageSender sender, IConnectionDetails details, INetworkMessageSubscriptionService subService,
			IDisconnectionServiceHandler disconnectHandler);

		protected override void Setup()
		{
			throw new NotImplementedException();
		}

		protected override void TearDown()
		{
			throw new NotImplementedException();
		}
	}
}
