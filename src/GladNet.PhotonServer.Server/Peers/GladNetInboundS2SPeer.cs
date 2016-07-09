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
using GladNet.Message;
using GladNet.Engine.Common;
using Easyception;

namespace GladNet.PhotonServer.Server
{
	public class GladNetInboundS2SPeer : InboundS2SPeer, IPeerContainer
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
		public GladNet.Engine.Common.Peer GladNetPeer { get; set; }

		public GladNetInboundS2SPeer(InitResponse response, INetworkMessageReceiver reciever, IDeserializerStrategy deserializationStrat, 
			IDisconnectionServiceHandler disconnectionService)
				: base(response)
		{
			Throw<ArgumentNullException>.If.IsNull(reciever)?.Now(nameof(reciever));
			Throw<ArgumentNullException>.If.IsNull(deserializationStrat)?.Now(nameof(deserializationStrat));
			Throw<ArgumentNullException>.If.IsNull(disconnectionService)?.Now(nameof(disconnectionService));

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
			GladNetPeer = null;

			//Disconnects the peer
			disconnectionServiceHandler.Disconnect();

			networkReciever.OnNetworkMessageReceive(new PhotonStatusMessageAdapter(NetStatus.Disconnected), null);
		}

		protected override void OnEvent(IEventData eventData, SendParameters sendParameters)
		{
			//TODO: Logging, we shouldn't recieve events.
		}

		protected override void OnOperationResponse(OperationResponse operationResponse, SendParameters sendParameters)
		{
			//TODO: Logging, we shouldn't recieve responses.
		}

		protected override void OnOperationRequest(OperationRequest operationRequest, SendParameters sendParameters)
		{
			//Try to get the only parameter
			//Should be the RequestMessage
			KeyValuePair<byte, object> objectPair = operationRequest.Parameters.FirstOrDefault();

			//TODO: Easyception should offer Now() ctors
			Throw<InvalidOperationException>.If.IsTrue(objectPair.Value == null)?.Now();

			RequestMessage message = deserializer.Deserialize<RequestMessage>(objectPair.Value as byte[]);

			//TODO: Easyception should offer Now() ctors
			Throw<InvalidOperationException>.If.IsTrue(message == null)?.Now();

			networkReciever.OnNetworkMessageReceive(message, new PhotonMessageParametersAdapter(sendParameters));
		}
	}
}
