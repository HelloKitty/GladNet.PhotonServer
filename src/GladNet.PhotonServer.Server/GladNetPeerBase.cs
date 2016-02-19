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
	public class GladNetPeerBase<TClientPeerSessionType> : PeerBase
		where TClientPeerSessionType : ClientPeerSession
	{
		private readonly TClientPeerSessionType gladNetSessionInstance;

		private readonly INetworkMessageReceiver networkReciever;

		private readonly IDeserializerStrategy deserializer;

		public GladNetPeerBase(IRpcProtocol protocol, IPhotonPeer unmanagedPeer, TClientPeerSessionType session, 
			INetworkMessageReceiver reciever, IDeserializerStrategy deserializationStrat) 
			: base(protocol, unmanagedPeer)
		{
			protocol.ThrowIfNull(nameof(protocol));
			unmanagedPeer.ThrowIfNull(nameof(unmanagedPeer));
			session.ThrowIfNull(nameof(session));
			reciever.ThrowIfNull(nameof(reciever));
			deserializationStrat.ThrowIfNull(nameof(deserializationStrat));

			gladNetSessionInstance = session;
			networkReciever = reciever;
			deserializer = deserializationStrat;
		}

		protected override void OnDisconnect(DisconnectReason reasonCode, string reasonDetail)
		{
			throw new NotImplementedException();
		}

		protected override void OnOperationRequest(OperationRequest operationRequest, SendParameters sendParameters)
		{
			//Make sure it's not null. I don't think it can be though
			operationRequest.Parameters.ThrowIfNull(nameof(operationRequest));

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
