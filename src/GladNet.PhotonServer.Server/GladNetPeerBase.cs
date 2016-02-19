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
		private readonly INetworkMessageReceiver networkReciever;

		private readonly IDeserializerStrategy deserializer;

		//A peer instance should be injected 
		public Peer GladnetPeer { get; set; }

		public GladNetPeerBase(IRpcProtocol protocol, IPhotonPeer unmanagedPeer, 
			INetworkMessageReceiver reciever, IDeserializerStrategy deserializationStrat)
			: base(protocol, unmanagedPeer)
		{
			protocol.ThrowIfNull(nameof(protocol));
			unmanagedPeer.ThrowIfNull(nameof(unmanagedPeer));
			reciever.ThrowIfNull(nameof(reciever));
			deserializationStrat.ThrowIfNull(nameof(deserializationStrat));

			networkReciever = reciever;
			deserializer = deserializationStrat;
		}

		protected override void OnDisconnect(DisconnectReason reasonCode, string reasonDetail)
		{
			if (GladnetPeer == null)
				throw new InvalidOperationException("The " + nameof(GladnetPeer) + " was never init.");

			//Disconnects the peer
			GladnetPeer.Disconnect();
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
