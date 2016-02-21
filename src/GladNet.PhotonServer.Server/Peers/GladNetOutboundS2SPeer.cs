using Photon.SocketServer.ServerToServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Photon.SocketServer;
using PhotonHostRuntimeInterfaces;
using GladNet.Serializer;
using GladNet.Common;
using GladNet.PhotonServer.Common;

namespace GladNet.PhotonServer.Server
{
	public class GladNetOutboundS2SPeer : OutboundS2SPeer, IPeerContainer
	{
		/// <summary>
		/// Reciever to push messages through.
		/// </summary>
		private INetworkMessageReceiver networkReciever { get; }

		/// <summary>
		/// Deserialization strategy for incoming payloads.
		/// </summary>
		private IDeserializerStrategy deserializer { get; }

		/// <summary>
		/// Service for handling disconnections.
		/// </summary>
		private IDisconnectionServiceHandler disconnectionServiceHandler;

		//Used only to keep a reference to the Peer object so that GC doesn't clean it up
		public GladNet.Common.Peer GladNetPeer { get; set; }

		public GladNetOutboundS2SPeer(GladNetAppBase appBase, INetworkMessageReceiver reciever, IDeserializerStrategy deserializationStrat, 
			IDisconnectionServiceHandler disconnectionService)
				: base(appBase)
		{
			appBase.ThrowIfNull(nameof(appBase));
			reciever.ThrowIfNull(nameof(reciever));
			deserializationStrat.ThrowIfNull(nameof(deserializationStrat));
			disconnectionService.ThrowIfNull(nameof(disconnectionService));

			disconnectionServiceHandler = disconnectionService;
			networkReciever = reciever;
			deserializer = deserializationStrat;

			//Publish that we are connecting
			networkReciever.OnNetworkMessageReceive(new PhotonStatusMessageAdapter(NetStatus.Connecting), null);
		}

		protected override void OnInitializeEcryptionCompleted(short resultCode, string debugMessage)
		{
			base.OnInitializeEcryptionCompleted(resultCode, debugMessage);
		}

		/// <summary>
		/// Called when Photon internally disconnects the peer.
		/// </summary>
		/// <param name="reasonCode">Reason for disconnecting.</param>
		/// <param name="reasonDetail">Detailed reason string.</param>
		protected override void OnDisconnect(DisconnectReason reasonCode, string reasonDetail)
		{
			//Null the peer out otherwise we will leak. Trust me.
			GladNetPeer = null;

			//Disconnects the peer
			disconnectionServiceHandler.Disconnect();

			networkReciever.OnNetworkMessageReceive(new PhotonStatusMessageAdapter(NetStatus.Disconnected), null);
		}

		protected override void OnConnectionEstablished(object responseObject)
		{
			//We connected so we should publish that fact
			IConnectionDetails details = new PhotonServerIConnectionDetailsAdapter(this.RemoteIP, this.RemotePort, this.LocalPort, this.ConnectionId);

			//This is a horrible way to do it but I did not expect this sort of change in Photon 4.
			GladNetPeer = ((GladNetAppBase)GladNetAppBase.Instance).CreateServerPeer(new PhotonServerINetworkMessageSenderClientAdapter(this, ((GladNetAppBase)GladNetAppBase.Instance).Serializer),
				details, (INetworkMessageSubscriptionService)networkReciever, disconnectionServiceHandler);

			//If we failed to generate a peer
			if(GladNetPeer == null)
			{
				this.Disconnect();
				return;
			}

			networkReciever.OnNetworkMessageReceive(new PhotonStatusMessageAdapter(NetStatus.Connected), null);
		}

		protected override void OnConnectionFailed(int errorCode, string errorMessage)
		{
			Disconnect();
		}

		protected override void OnEvent(IEventData eventData, SendParameters sendParameters)
		{
			//Try to get the only parameter
			//Should be the PacketPayload
			KeyValuePair<byte, object> objectPair = eventData.Parameters.FirstOrDefault();

			if (objectPair.Value == null)
				return;

			PacketPayload payload = deserializer.Deserialize<PacketPayload>(objectPair.Value as byte[]);

			if (payload == null)
				return;

			networkReciever.OnNetworkMessageReceive(new PhotonEventMessageAdapter(payload), new PhotonMessageParametersAdapter(sendParameters));
		}

		protected override void OnOperationResponse(OperationResponse operationResponse, SendParameters sendParameters)
		{
			//Try to get the only parameter
			//Should be the PacketPayload
			KeyValuePair<byte, object> objectPair = operationResponse.Parameters.FirstOrDefault();

			if (objectPair.Value == null)
				return;

			PacketPayload payload = deserializer.Deserialize<PacketPayload>(objectPair.Value as byte[]);

			if (payload == null)
				return;

			networkReciever.OnNetworkMessageReceive(new PhotonResponseMessageAdapter(payload), new PhotonMessageParametersAdapter(sendParameters));
		}

		protected override void OnOperationRequest(OperationRequest operationRequest, SendParameters sendParameters)
		{
			//TODO: Logging, we shouldn't recieve requests.
		}
	}
}
