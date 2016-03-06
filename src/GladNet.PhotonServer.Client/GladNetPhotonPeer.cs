using ExitGames.Client.Photon;
using GladNet.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common.Logging;
using GladNet.PhotonServer.Common;
using GladNet.Serializer;

namespace GladNet.PhotonServer.Client
{
	public class GladNetPhotonPeer : PhotonPeer, IPhotonPeerListener, IClientPeerNetworkMessageSender
	{
		private IClientNetworkMessageReciever callbackMessageReciever { get; }

		private IDeserializerStrategy deserializer { get; }

		private ISerializerStrategy serializer { get; }

		public GladNetPhotonPeer(ConnectionProtocol protocolType, IDeserializerStrategy deserializationStrat, ISerializerStrategy serializationStrat, IClientNetworkMessageReciever reciever)
			: base(protocolType)
		{
			Listener = this;
			deserializer = deserializationStrat;
			serializer = serializationStrat;
			callbackMessageReciever = reciever;
		}

		public override void Service()
		{
			base.Service();
		}

		public void DebugReturn(DebugLevel level, string message)
		{
			//We don't do anything in GladNet with this
		}

		public void OnOperationResponse(OperationResponse operationResponse)
		{
			PacketPayload payload = StripPayload(operationResponse.Parameters);

			if (payload == null)
				return;

			callbackMessageReciever.OnReceiveResponse(payload);
		}

		public void OnStatusChanged(StatusCode statusCode)
		{
			NetStatus? status = statusCode.ToGladNet();

			if (status.HasValue)
				this.callbackMessageReciever.OnStatusChanged(status.Value);
		}

		public void OnEvent(EventData eventData)
		{
			PacketPayload payload = StripPayload(eventData.Parameters);

			if (payload == null)
				return;

			callbackMessageReciever.OnReceiveEvent(payload);
		}

		private PacketPayload StripPayload(Dictionary<byte, object> parameters)
		{
			//Try to get the only parameter
			//Should be the PacketPayload
			KeyValuePair<byte, object> objectPair = parameters.FirstOrDefault();

			if (objectPair.Value == null)
				return null;

			return deserializer.Deserialize<PacketPayload>(objectPair.Value as byte[]);
		}

		public SendResult SendRequest(PacketPayload payload, DeliveryMethod deliveryMethod, bool encrypt = false, byte channel = 0)
		{
			byte[] payloadBytes = serializer.Serialize(payload);

			return OpCustom(1, new Dictionary<byte, object>() { { 1, payloadBytes } }, deliveryMethod.isReliable(), channel) ? SendResult.Sent : SendResult.Invalid;
		}

		public SendResult SendRequest<TPacketType>(TPacketType payload) 
			where TPacketType : PacketPayload, IStaticPayloadParameters
		{
			byte[] payloadBytes = serializer.Serialize(payload);

			return OpCustom(1, new Dictionary<byte, object>() { { 1, payloadBytes } }, payload.DeliveryMethod.isReliable(), payload.Channel) ? SendResult.Sent : SendResult.Invalid;
		}
	}
}
