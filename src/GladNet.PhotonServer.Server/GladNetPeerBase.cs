using Photon.SocketServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PhotonHostRuntimeInterfaces;
using GladNet.Server.Common;
using GladNet.Common;
using GladNet.Serializer;

namespace GladNet.PhotonServer.Server
{
	public class GladNetPeerBase : PeerBase
	{
		private INetworkMessageReceiver networkReciever { get; }

		private IDeserializerStrategy deserializer { get; }

		private IDisconnectionServiceHandler disconnectionServiceHandler;

		//Used only to keep a reference to the Peer object so that GC doesn't clean it up
		public Peer Peer { get; set; }

		public GladNetPeerBase(IRpcProtocol protocol, IPhotonPeer unmanagedPeer, 
			INetworkMessageReceiver reciever, IDeserializerStrategy deserializationStrat, IDisconnectionServiceHandler disconnectionService)
			: base(protocol, unmanagedPeer)
		{
			protocol.ThrowIfNull(nameof(protocol));
			unmanagedPeer.ThrowIfNull(nameof(unmanagedPeer));
			reciever.ThrowIfNull(nameof(reciever));
			deserializationStrat.ThrowIfNull(nameof(deserializationStrat));
			disconnectionService.ThrowIfNull(nameof(disconnectionService));

			disconnectionServiceHandler = disconnectionService;
			networkReciever = reciever;
			deserializer = deserializationStrat;
		}

		protected override void OnDisconnect(DisconnectReason reasonCode, string reasonDetail)
		{
			//Null the peer out otherwise we will leak. Trust me.
			Peer = null;

			//Disconnects the peer
			disconnectionServiceHandler.Disconnect();
		}

		protected override void OnOperationRequest(OperationRequest operationRequest, SendParameters sendParameters)
		{
			//Try to get the only parameter
			//Should be the PacketPayload
			KeyValuePair<byte, object> objectPair = operationRequest.Parameters.FirstOrDefault();

			if (objectPair.Value == null)
				return;

			PacketPayload payload = deserializer.Deserialize<PacketPayload>(objectPair.Value as byte[]);

			if (payload == null)
				return;

			networkReciever.OnNetworkMessageReceive(new PhotonRequestMessageAdapter(payload), new PhotonMessageParametersAdapter(sendParameters)); 
		}
	}
}
