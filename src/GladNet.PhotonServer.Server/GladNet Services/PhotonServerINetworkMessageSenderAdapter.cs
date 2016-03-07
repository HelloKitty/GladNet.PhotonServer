using GladNet.Common;
using GladNet.Serializer;
using Photon.SocketServer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GladNet.PhotonServer.Server
{
	public class PhotonServerINetworkMessageSenderClientAdapter : INetworkMessageSender
	{
		private PeerBase photonPeer { get; }

		private ISerializerStrategy serializerStrategy;

		public PhotonServerINetworkMessageSenderClientAdapter(PeerBase peer, ISerializerStrategy serializer)
		{
			photonPeer = peer;
			serializerStrategy = serializer;
		}

		public bool CanSend(OperationType opType)
		{
			if (opType == OperationType.Event || opType == OperationType.Response)
				return true;
			else
				return false;
		}

		public GladNet.Common.SendResult TrySendMessage(OperationType opType, PacketPayload payload, DeliveryMethod deliveryMethod, bool encrypt = false, byte channel = 0)
		{
			Photon.SocketServer.SendResult result;

			//Depending on the operation type we'll need to call different methods on the peer to send
			switch (opType)
			{
				case OperationType.Event:
					result = SendEvent(payload, deliveryMethod.isReliable(), encrypt, channel);
					break;

				case OperationType.Response:
					result = SendResponse(payload, deliveryMethod.isReliable(), encrypt, channel);
					break;

				default:
					return GladNet.Common.SendResult.Invalid;
			}

			//Map the send result
			switch (result)
			{
				case Photon.SocketServer.SendResult.Ok:
					return GladNet.Common.SendResult.Sent;
				case Photon.SocketServer.SendResult.Disconnected:
					return GladNet.Common.SendResult.FailedNotConnected;
				case Photon.SocketServer.SendResult.SendBufferFull:
					return GladNet.Common.SendResult.Invalid;
				case Photon.SocketServer.SendResult.MessageToBig:
					return GladNet.Common.SendResult.Invalid;
				case Photon.SocketServer.SendResult.InvalidChannel:
					return GladNet.Common.SendResult.Invalid;
				case Photon.SocketServer.SendResult.Failed:
					return GladNet.Common.SendResult.Invalid;
				case Photon.SocketServer.SendResult.InvalidContentType:
					return GladNet.Common.SendResult.Invalid;
				case Photon.SocketServer.SendResult.EncryptionNotSupported:
					return GladNet.Common.SendResult.Invalid;
				default:
					return GladNet.Common.SendResult.Invalid;
			}
		}

		private Photon.SocketServer.SendResult SendEvent(PacketPayload payload, bool unreliable, bool encrypt, byte channel)
		{
			//Builds the message in a context that Photon understands (dictionary of objects)
			EventData data = new EventData(1, new Dictionary<byte, object>(1) { { 0, SerializePayload(payload) } });

			//Sends the event through Photon's transport layer.
			return photonPeer.SendEvent(data, new SendParameters() { ChannelId = channel, Encrypted = encrypt, Unreliable = unreliable });
		}

		private Photon.SocketServer.SendResult SendResponse(PacketPayload payload, bool unreliable, bool encrypt, byte channel)
		{
			//Builds the message in a context that Photon understands (dictionary of objects)
			OperationResponse data = new OperationResponse(1, new Dictionary<byte, object>(1) { { 0, SerializePayload(payload) } });

			//Sends the event through Photon's transport layer.
			return photonPeer.SendOperationResponse(data, new SendParameters() { ChannelId = channel, Encrypted = encrypt, Unreliable = unreliable });
		}

		private byte[] SerializePayload(PacketPayload payload)
		{
			return serializerStrategy.Serialize(payload);
		}

		public GladNet.Common.SendResult TrySendMessage<TPacketType>(OperationType opType, TPacketType payload) 
			where TPacketType : PacketPayload, IStaticPayloadParameters
		{
			return TrySendMessage(opType, payload, payload.DeliveryMethod, payload.Encrypted, payload.Channel);
		}
	}
}
