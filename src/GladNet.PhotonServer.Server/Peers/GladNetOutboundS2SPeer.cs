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

namespace GladNet.PhotonServer.Server
{
	public class GladNetOutboundS2SPeer : OutboundS2SPeer
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
		public Peer Peer { get; set; }

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

		/// <summary>
		/// Called when Photon internally disconnects the peer.
		/// </summary>
		/// <param name="reasonCode">Reason for disconnecting.</param>
		/// <param name="reasonDetail">Detailed reason string.</param>
		protected override void OnDisconnect(DisconnectReason reasonCode, string reasonDetail)
		{
			//Null the peer out otherwise we will leak. Trust me.
			Peer = null;

			//Disconnects the peer
			disconnectionServiceHandler.Disconnect();
		}

		protected override void OnConnectionEstablished(object responseObject)
		{
			//We connected so we should publish that fact
			IConnectionDetails details = new PhotonServerIConnectionDetailsAdapter(this.RemoteIP, this.RemotePort, this.LocalPort, this.ConnectionId);

			//This is a horrible way to do it but I did not expect this sort of change in Photon 4.
			Peer = ((GladNetAppBase)GladNetAppBase.Instance).CreateServerPeer(new PhotonServerINetworkMessageSenderClientAdapter(this, ((GladNetAppBase)GladNetAppBase.Instance).Serializer),
				details, (INetworkMessageSubscriptionService)networkReciever, disconnectionServiceHandler);

			//If we failed to generate a peer
			if(Peer == null)
			{
				this.Disconnect();
				return;
			}

			networkReciever.OnNetworkMessageReceive(new PhotonStatusMessageAdapter(NetStatus.Connected), null);
		}

		protected override void OnConnectionFailed(int errorCode, string errorMessage)
		{
			//Do nothing I guess.
		}

		protected override void OnEvent(IEventData eventData, SendParameters sendParameters)
		{
			throw new NotImplementedException();
		}

		protected override void OnOperationResponse(OperationResponse operationResponse, SendParameters sendParameters)
		{
			throw new NotImplementedException();
		}

		protected override void OnOperationRequest(OperationRequest operationRequest, SendParameters sendParameters)
		{
			throw new NotImplementedException();
		}
	}
}
