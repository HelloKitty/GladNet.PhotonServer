using ExitGames.Client.Photon;
using GladNet.Common;
using GladNet.PhotonServer.Server;
using GladNet.Serializer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace GladNet.PhotonServer.Client
{
	public abstract class UnityClientPeer : MonoBehaviour, IPhotonPeerListener, IClientPeerNetworkMessageSender, IClientNetworkMessageReciever
	{
		protected abstract IDeserializerStrategy deserializer { get; }

		protected abstract ISerializerStrategy serializer { get; }

		private PhotonPeer peer { get; set; }

		void IPhotonPeerListener.DebugReturn(DebugLevel level, string message)
		{
			//Do nothing
		}

		public bool Connect(string serverAddress, string appName)
		{
			peer = new PhotonPeer(this, ConnectionProtocol.Udp);

			bool result = peer.Connect(serverAddress, appName);

			if (!result)
				return result;

			if(!isPollRunning)
				//Start the polling process
				StartCoroutine(Poll());

			return true;
		}

		private bool isPollRunning = false;
		private readonly WaitForSeconds waitTime = new WaitForSeconds(0.1f);
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

		void IPhotonPeerListener.OnOperationResponse(OperationResponse operationResponse)
		{
			PacketPayload payload = StripPayload(operationResponse.Parameters);

			if (payload == null)
				return;

			this.OnReceiveResponse(payload);
		}

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

		public SendResult SendRequest(PacketPayload payload, DeliveryMethod deliveryMethod, bool encrypt = false, byte channel = 0)
		{
			byte[] payloadBytes = serializer.Serialize(payload);

			return peer.OpCustom(1, new Dictionary<byte, object>() { { 1, payloadBytes } }, deliveryMethod.isReliable(), channel) ? SendResult.Sent : SendResult.Invalid;
		}

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

		public abstract void OnReceiveResponse(PacketPayload payload);
		public abstract void OnReceiveEvent(PacketPayload payload);
		public abstract void OnStatusChanged(NetStatus status);

		public bool CanSend(OperationType opType)
		{
			throw new NotImplementedException();
		}
	}
}
