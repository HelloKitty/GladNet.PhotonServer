using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Photon.SocketServer;
using GladNet.Common;
using Logging.Services;
using GladNet.Server.Common;
using GladNet.Serializer;

namespace GladNet.PhotonServer.Server
{
	public abstract class GladNetAppBase : Photon.SocketServer.ApplicationBase
	{
		protected abstract ILogger AppLogger { get; set; }

		protected abstract ISerializerStrategy Serializer { get; set; }

		protected abstract IDeserializerStrategy Deserializer { get; set; }

		//TODO: Right now there is no reference to the session so it will be cleaned up by GC. Store it somewhere.
		protected override PeerBase CreatePeer(InitRequest initRequest)
		{
			//Create the details so that the consumer of this class, who extends it, can indicate if this is a request we should service
			//AKA should a peer be made
			IConnectionDetails details = new PhotonServerIConnectionDetailsAdapter(initRequest.RemoteIP, initRequest.RemotePort, initRequest.LocalPort, initRequest.ConnectionId);

			//If we should service the peer
			if (ShouldServiceIncomingPeerConnect(details))
			{
				//Unlike in PhotonServer we have the expectation that they WILL be creating a peer since they said they would
				//Because of this we'll be creating the actual PeerBase in advance.
				NetworkMessagePublisher publisher = new NetworkMessagePublisher();
				IDisconnectionServiceHandler disconnectionHandler = new PhotonServerIDisconnectionServiceHandlerAdapter();

				//Build the peer first since it's required for the network message sender
				GladNetPeerBase peerBase = new GladNetPeerBase(initRequest.Protocol, initRequest.PhotonPeer, publisher, Deserializer, disconnectionHandler);
				//We should make the ClientPeerSession now
				ClientPeerSession session = CreateClientSession(new PhotonServerINetworkMessageSenderClientAdapter(peerBase, Serializer), details, publisher, disconnectionHandler);

				if (session == null)
				{
					peerBase.Disconnect();

					return null;
				}
				
				//This must be done to keep alive the reference of the session
				//Otherwise GC will clean it up (WARNING: This will create circular reference and cause a leak if you do not null the peer out eventually)
				peerBase.Peer = session;

				return peerBase;
			}
			else
			{
				//Disconnect the client if they're not going to have a peer serviced
				initRequest.PhotonPeer.DisconnectClient();

				return null;
			}
		}

		protected abstract bool ShouldServiceIncomingPeerConnect(IConnectionDetails details);

		protected abstract ClientPeerSession CreateClientSession(INetworkMessageSender sender, IConnectionDetails details, INetworkMessageSubscriptionService subService,
			IDisconnectionServiceHandler disconnectHandler);

		protected override abstract void Setup();

		protected override abstract void TearDown();
	}
}
