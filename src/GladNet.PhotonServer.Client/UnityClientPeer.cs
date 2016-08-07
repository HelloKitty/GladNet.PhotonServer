using ExitGames.Client.Photon;
using GladNet.Common;
using GladNet.Engine.Common;
using GladNet.PhotonServer.Server;
using GladNet.Serializer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using UnityEngine;
using GladNet.Message;
using GladNet.Payload;

namespace GladNet.PhotonServer.Client
{
	//TODO: Stop leaking PhotonServer IPhotonPeerListener interface to consumers of this library.
	/// <summary>
	/// Unity3D Component that acts as a network peer.
	/// </summary>
	/// <typeparam name="TSerializationStrategy"></typeparam>
	/// <typeparam name="TDeserializationStrategy"></typeparam>
	/// <typeparam name="TSerializerRegistry"></typeparam>
	public abstract class UnityClientPeer<TSerializationStrategy, TDeserializationStrategy, TSerializerRegistry> : MonoBehaviour, IPhotonPeerListener, IClientPeerNetworkMessageRouter, IClientPeerPayloadSender, IClientNetworkMessageReciever, INetPeer
		where TSerializationStrategy : ISerializerStrategy, new() where TDeserializationStrategy : IDeserializerStrategy, new() where TSerializerRegistry : ISerializerRegistry, new()
	{
		//Contraining new() for generic type params in .Net 3.5 is very slow
		//This object should rarely be created. If in the future you must fix this slowness, which compiled to Activator, then
		//you should use a compliled lambda expression to create the object I think.

		/// <summary>
		/// Deserializer capable of deserializing incoming messages of the expected format.
		/// </summary>
		private IDeserializerStrategy deserializer { get; } = new TDeserializationStrategy();

		/// <summary>
		/// Serializer capable of serializing outgoing messages of the designated format.
		/// </summary>
		private ISerializerStrategy serializer { get; } = new TSerializationStrategy();

		//Dont assume calls to this service register types for all serializers.
		//Though that probably is the case.
		/// <summary>
		/// Serialization registry service that provides simple type registeration services to make aware specified types
		/// to the serializer service called <see cref="serializer"/> within this class.
		/// </summary>
		private ISerializerRegistry serializerRegiter { get; } = new TSerializerRegistry();

		/// <summary>
		/// Internally managed <see cref="PhotonPeer"/> which this class wraps around building the GladNet2 API on.
		/// It is the network peer for the client in PhotonServer Unity products.
		/// </summary>
		private PhotonPeer peer { get; set; }

		/// <summary>
		/// Indicates the current status of the network peer.
		/// </summary>
		public NetStatus Status { get; private set; } = NetStatus.Disconnected; //default should be disconnected.

		/// <summary>
		/// Exposes information about the Peer's connection.
		/// Null if no connection was attempted.
		/// </summary>
		public IConnectionDetails PeerDetails { get; private set; }

		/// <summary>
		/// Service provides network message routing and sending
		/// Service will be null and unavailable until a connection attempt.
		/// </summary>
		public INetworkMessageRouterService NetworkSendService { get; private set; }

		/// <summary>
		/// Provides textual descriptions for various error conditions and noteworthy situations. In cases where the application needs to react, a call to OnStatusChanged is used. 
		/// OnStatusChanged gives "feedback" to the game, DebugReturn provies human readable messages on the background.
		/// </summary>
		/// <param name="level">DebugLevel (severity) of the message.</param>
		/// <param name="message">Debug text. Print to System.Console or screen.</param>
		void IPhotonPeerListener.DebugReturn(DebugLevel level, string message)
		{
			//Do nothing
		}

		/// <summary>
		/// Attempts to connect to the specified <paramref name="serverAddress"/> and <paramref name="appName"/>.
		/// Application Name is used to plex between multiple apps on the given address for certain implementations.
		/// </summary>
		/// <param name="serverAddress">Endpoint IP/Domain for the server.</param>
		/// <param name="appName">Application name of the server application at the endpoint.</param>
		/// <returns></returns>
		public bool Connect(string serverAddress, string appName)
		{
			//Register these so the user doesnt have to
			this.serializerRegiter.Register(typeof(NetworkMessage));
			this.serializerRegiter.Register(typeof(RequestMessage));
			this.serializerRegiter.Register(typeof(StatusMessage));
			this.serializerRegiter.Register(typeof(ResponseMessage));
			this.serializerRegiter.Register(typeof(EventMessage));

			//We need to register the payload types before we can even send
			//anything. Otherwise we can't serialize.
			RegisterPayloadTypes(this.serializerRegiter);

			//We simply create a new PhotonPeer which would generally be created by users
			//who normally use Photon but we store it and provide the GladNet API on top of it through this class.
			peer = new PhotonPeer(this, ConnectionProtocol.Udp);

			//This indicates if it's a valid connection attempt, not if we're actually connected.
			//Connection is NOT established after this line. It's no syncronous.
			bool isConnecting = peer.Connect(serverAddress, appName);

			//We can't really give accurate data. Photon doesn't expose it.
			PeerDetails = new PhotonServerIConnectionDetailsAdapter(serverAddress.Split(':').First(), Int32.Parse(serverAddress.Split(':').Last()), -1);
			NetworkSendService = new UnityClientPeerNetworkMessageSenderAdapter<UnityClientPeer<TSerializationStrategy, TDeserializationStrategy, TSerializerRegistry>>(this);

			if (!isConnecting)
				return isConnecting;

			//This is thread save because Unity coroutines occur on the same thread.
			//Also, this will prevent multiple poll routines.
			if(!isPollRunning)
				//Start the polling process
				StartCoroutine(BeginPoll());

			return true;
		}

		/// <summary>
		/// Attempts to establish a secure channel with which to communicate with the
		/// remote-host with.
		/// </summary>
		public void EstablishEncryption()
		{
			peer.EstablishEncryption();
		}

		/// <summary>
		/// Indicates if the polling system is still running
		/// </summary>
		private bool isPollRunning = false;

		/// <summary>
		/// A cached <see cref="WaitForSeconds"/> to be yielded on for the coroutine.
		/// </summary>
		private readonly WaitForSeconds waitTime = new WaitForSeconds(0.1f); //TODO: Expose this to the user.

		/// <summary>
		/// Begins the internal polling mechanism by attaching a Unity3D coroutine to this
		/// <see cref="MonoBehaviour"/>.
		/// </summary>
		/// <returns>The coroutine.</returns>
		private IEnumerator BeginPoll()
		{
			isPollRunning = true;
			while (peer != null)
			{
				peer.Service();
				yield return waitTime;
			}

			isPollRunning = false;
		}

		/// <summary>
		/// PhotonServer's <see cref="IPhotonPeerListener"/> OnEvent implementation.
		/// This should not be called. It is an artifact of efficently wrapping the class.
		/// </summary>
		/// <param name="eventData">Internal PhotonServer event data.</param>
		void IPhotonPeerListener.OnEvent(EventData eventData)
		{
			Debug.Log("Recieved event");

			EventMessage message = StripMessage(eventData);

			if (message == null)
			{
				Debug.LogWarning("Event was empty");
				return;
			}

			//PhotonServer does not provide information about recieved messages.
			this.OnReceiveEvent(message, null);
		}

		/// <summary>
		/// PhotonServer's <see cref="IPhotonPeerListener"/> OnOperationResponse implementation.
		/// This should not be called. It is an artifact of efficently wrapping the class.
		/// </summary>
		/// <param name="eventData">Internal PhotonServer response data.</param>
		void IPhotonPeerListener.OnOperationResponse(OperationResponse operationResponse)
		{
			ResponseMessage message = StripMessage(operationResponse);

			if (message == null)
				return;

			//PhotonServer does not provide information about recieved messages.
			this.OnReceiveResponse(message, null);
		}

		/// <summary>
		/// PhotonServer's <see cref="IPhotonPeerListener"/> OnOperationResponse implementation.
		/// This should not be called. It is an artifact of efficently wrapping the class.
		/// </summary>
		/// <param name="eventData">Internal PhotonServer response code.</param>
		void IPhotonPeerListener.OnStatusChanged(StatusCode statusCode)
		{
			NetStatus? status = statusCode.ToGladNet();

			if (status.HasValue)
			{
				//We should set the INetPeer status with the new status
				Status = status.Value;

				this.OnStatusChanged(status.Value);
			}
		}

		//We cannot make these generic because PhotonServer failed to create interfaces
		//for the incoming message data.

		/// <summary>
		/// Strips the <see cref="EventMessage"/> from the <see cref="EventData"/>.
		/// </summary>
		/// <param name="data">Incoming <see cref="EventData"/>.</param>
		/// <returns>The <see cref="EventMessage"/> from the data or null.</returns>
		private EventMessage StripMessage(EventData data)
		{
			//Try to get the only parameter
			//Should be the Message
			KeyValuePair<byte, object> objectPair = data.Parameters.FirstOrDefault(x => x.Value != null);

			if (objectPair.Value == null)
				return null;

			return deserializer.Deserialize<EventMessage>(objectPair.Value as byte[]);
		}

		/// <summary>
		/// Strips the <see cref="ResponseMessage"/> from the <see cref="OperationResponse"/>.
		/// </summary>
		/// <param name="data">Incoming <see cref="OperationResponse"/>.</param>
		/// <returns>The <see cref="ResponseMessage"/> from the data or null.</returns>
		private ResponseMessage StripMessage(OperationResponse data)
		{
			//Try to get the only parameter
			//Should be the Message
			KeyValuePair<byte, object> objectPair = data.Parameters.FirstOrDefault(x => x.Value != null);

			if (objectPair.Value == null)
				return null;

			return deserializer.Deserialize<ResponseMessage>(objectPair.Value as byte[]);
		}

		/// <summary>
		/// Sends a networked request.
		/// </summary>
		/// <param name="payload"><see cref="PacketPayload"/> for the desired network request message.</param>
		/// <param name="deliveryMethod">Desired <see cref="DeliveryMethod"/> for the request. See documentation for more information.</param>
		/// <param name="encrypt">Optional: Indicates if the message should be encrypted. Default: false</param>
		/// <param name="channel">Optional: Inidicates the channel the network message should be sent on. Default: 0</param>
		/// <returns>Indication of the message send state.</returns>
		[SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
		public SendResult SendRequest(PacketPayload payload, DeliveryMethod deliveryMethod, bool encrypt = false, byte channel = 0)
		{
			//We send messages now and not payloads
			RequestMessage message = new RequestMessage(payload);

			//Serialize the internal payload
			//We need to do this manually
			message.Payload.Serialize(serializer);

			//WARNING: Make sure to send encrypted parameter. There was a fault where we didn't. We cannot unit test it as it's within a MonoBehaviour
			return peer.OpCustom(1, new Dictionary<byte, object>() { { 1, serializer.Serialize(message) } }, deliveryMethod.isReliable(), channel, encrypt) ? SendResult.Sent : SendResult.Invalid;
		}

		/// <summary>
		/// Sends a networked request.
		/// Additionally this message/payloadtype is known to have static send parameters and those will be used in transit.
		/// </summary>
		/// <typeparam name="TPacketType">Type of the packet payload.</typeparam>
		/// <param name="payload">Payload instance to be sent in the message that contains static message parameters.</param>
		/// <returns>Indication of the message send state.</returns>
		public SendResult SendRequest<TPacketType>(TPacketType payload)
			where TPacketType : PacketPayload, IStaticPayloadParameters
		{
			//We send messages now and not payloads
			RequestMessage message = new RequestMessage(payload);

			//Serialize the internal payload
			//We need to do this manually
			message.Payload.Serialize(serializer);

			//WARNING: Make sure to send encrypted parameter. There was a fault where we didn't. We cannot unit test it as it's within a MonoBehaviour
			return peer.OpCustom(1, new Dictionary<byte, object>() { { 1, serializer.Serialize(message) } }, payload.DeliveryMethod.isReliable(), payload.Channel, payload.Encrypted) ? SendResult.Sent : SendResult.Invalid;
		}

		/// <summary>
		/// Called internally by Unity3D when the application is terminating.
		/// Overriders MUST call base.
		/// </summary>
		protected virtual void OnApplicationQuit()
		{
			if (peer != null)
				peer.Disconnect();
		}

		/// <summary>
		/// Handles a <see cref="PacketPayload"/> sent as a response.
		/// </summary>
		/// <param name="payload">Response payload data from the network.</param>
		public abstract void OnReceiveResponse(IResponseMessage message, IMessageParameters parameters);

		/// <summary>
		/// Handles a <see cref="PacketPayload"/> sent as an event.
		/// </summary>
		/// <param name="payload">Event payload data from the network.</param>
		public abstract void OnReceiveEvent(IEventMessage message, IMessageParameters parameters);

		public abstract void RegisterPayloadTypes(ISerializerRegistry registry);

		/// <summary>
		/// Handles a changed <see cref="NetStatus"/> stat from either local events or network events.
		/// </summary>
		/// <param name="status">Current status.</param>
		public abstract void OnStatusChanged(NetStatus status);

		public bool CanSend(OperationType opType)
		{
			//Clients can only send requests.
			return opType == OperationType.Request;
		}

		public SendResult RouteRequest(IRequestMessage message, DeliveryMethod deliveryMethod, bool encrypt = false, byte channel = 0)
		{
			//WARNING: Make sure to send encrypted parameter. There was a fault where we didn't. We cannot unit test it as it's within a MonoBehaviour
			return peer.OpCustom(1, new Dictionary<byte, object>() { { 1, message.SerializeWithVisitor(serializer) } }, deliveryMethod.isReliable(), channel, encrypt) ? SendResult.Sent : SendResult.Invalid;
		}
	}
}
