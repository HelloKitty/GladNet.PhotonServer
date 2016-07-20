using Photon.SocketServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PhotonHostRuntimeInterfaces;
using GladNet.Common;
using GladNet.Serializer;
using Photon.SocketServer.Rpc;
using GladNet.PhotonServer.Common;
using GladNet.Payload;
using GladNet.Message;
using GladNet.Engine.Common;
using Easyception;
using Photon.SocketServer.Rpc.Protocols;

namespace GladNet.PhotonServer.Server
{
	/// <summary>
	/// PeerBase for GladNet2 serversides. Handles message forwarding to <see cref="GladNetPeer"/>s and other network services
	/// as a proxy to the actual GladNet peer for Photon.
	/// </summary>
	public class GladNetClientPeer : Photon.SocketServer.ClientPeer, IPeerContainer
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

		public GladNetClientPeer(InitRequest request, 
			INetworkMessageReceiver reciever, IDeserializerStrategy deserializationStrat, IDisconnectionServiceHandler disconnectionService)
			: base(request)
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

		/// <summary>
		/// Called when photon internally recieves an operation request.
		/// </summary>
		/// <param name="operationRequest">The request.</param>
		/// <param name="sendParameters">The parameters with which the operation was sent.</param>
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
