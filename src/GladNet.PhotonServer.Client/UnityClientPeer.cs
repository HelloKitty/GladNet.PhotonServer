using ExitGames.Client.Photon;
using GladNet.Common;
using GladNet.PhotonServer.Server;
using GladNet.Serializer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using UnityEngine;

namespace GladNet.PhotonServer.Client
{
	//TODO: Stop leaking PhotonServer IPhotonPeerListener interface to consumers of this library.
	/// <summary>
	/// 
	/// </summary>
	/// <typeparam name="TSerializationStrategy"></typeparam>
	/// <typeparam name="TDeserializationStrategy"></typeparam>
	/// <typeparam name="TSerializerRegistry"></typeparam>
	public abstract class UnityClientPeer<TSerializationStrategy, TDeserializationStrategy, TSerializerRegistry> : MonoBehaviour, IPhotonPeerListener, IClientPeerNetworkMessageSender, IClientNetworkMessageReciever
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

			//We simply create a new PhotonPeer which would generally be created by users
			//who normally use Photon but we store it and provide the GladNet API on top of it through this class.
			peer = new PhotonPeer(this, ConnectionProtocol.Udp);

			//This indicates if it's a valid connection attempt, not if we're actually connected.
			//Connection is NOT established after this line. It's no syncronous.
			bool isConnecting = peer.Connect(serverAddress, appName);

			if (!isConnecting)
				return isConnecting;

			//This is thread save because Unity coroutines occur on the same thread.
			//Also, this will prevent multiple poll routines.
			if(!isPollRunning)
				//Start the polling process
				StartCoroutine(Poll());

			return true;
		}

		private bool isPollRunning = false;
		private readonly WaitForSeconds waitTime = new WaitForSeconds(0.1f); //TODO: Expose this to the user.
		private IEnumerator Poll()
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

			PacketPayload payload = StripPayload(eventData.Parameters);

			if (payload == null)
			{
				Debug.LogWarning("Event was empty");
				return;
			}	

			this.OnReceiveEvent(payload);
		}

		/// <summary>
		/// PhotonServer's <see cref="IPhotonPeerListener"/> OnOperationResponse implementation.
		/// This should not be called. It is an artifact of efficently wrapping the class.
		/// </summary>
		/// <param name="eventData">Internal PhotonServer response data.</param>
		void IPhotonPeerListener.OnOperationResponse(OperationResponse operationResponse)
		{
			PacketPayload payload = StripPayload(operationResponse.Parameters);

			if (payload == null)
				return;

			this.OnReceiveResponse(payload);
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
				this.OnStatusChanged(status.Value);
		}

		private PacketPayload StripPayload(Dictionary<byte, object> parameters)
		{
			//Try to get the only parameter
			//Should be the PacketPayload
			KeyValuePair<byte, object> objectPair = parameters.FirstOrDefault(x => x.Value != null);

			if (objectPair.Value == null)
				return null;

			return deserializer.Deserialize<PacketPayload>(objectPair.Value as byte[]);
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
			byte[] payloadBytes = serializer.Serialize(payload);

			return peer.OpCustom(1, new Dictionary<byte, object>() { { 1, payloadBytes } }, deliveryMethod.isReliable(), channel) ? SendResult.Sent : SendResult.Invalid;
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
			byte[] payloadBytes = serializer.Serialize(payload);

			return peer.OpCustom(1, new Dictionary<byte, object>() { { 1, payloadBytes } }, payload.DeliveryMethod.isReliable(), payload.Channel) ? SendResult.Sent : SendResult.Invalid;
		}

		protected virtual void OnApplicationQuit()
		{
			if (peer != null)
				peer.Disconnect();
		}

		/// <summary>
		/// Handles a <see cref="PacketPayload"/> sent as a response.
		/// </summary>
		/// <param name="payload">Response payload data from the network.</param>
		public abstract void OnReceiveResponse(PacketPayload payload);

		/// <summary>
		/// Handles a <see cref="PacketPayload"/> sent as an event.
		/// </summary>
		/// <param name="payload">Event payload data from the network.</param>
		public abstract void OnReceiveEvent(PacketPayload payload);

		/// <summary>
		/// Handles a changed <see cref="NetStatus"/> stat from either local events or network events.
		/// </summary>
		/// <param name="status">Current status.</param>
		public abstract void OnStatusChanged(NetStatus status);
	}
}
