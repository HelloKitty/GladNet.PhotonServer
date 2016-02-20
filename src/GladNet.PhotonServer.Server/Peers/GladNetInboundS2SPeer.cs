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
	public class GladNetInboundS2SPeer : InboundS2SPeer
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

		public GladNetInboundS2SPeer(InitResponse response, INetworkMessageReceiver reciever, IDeserializerStrategy deserializationStrat, 
			IDisconnectionServiceHandler disconnectionService)
				: base(response)
		{
			response.ThrowIfNull(nameof(response));
			reciever.ThrowIfNull(nameof(reciever));
			deserializationStrat.ThrowIfNull(nameof(deserializationStrat));
			disconnectionService.ThrowIfNull(nameof(disconnectionService));

			disconnectionServiceHandler = disconnectionService;
			networkReciever = reciever;
			deserializer = deserializationStrat;
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
