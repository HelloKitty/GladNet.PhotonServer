using Easyception;
using GladNet.Common;
using GladNet.Engine.Common;
using GladNet.Message;
using GladNet.Payload;
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
	/// <summary>
	/// Adapter for the <see cref="INetworkMessageRouterService"/> interface.
	/// </summary>
	public class PhotonServerINetworkMessageSenderClientAdapter : INetworkMessageRouterService
	{
		/// <summary>
		/// Internal Photon <see cref="PeerBase"/>.
		/// </summary>
		private PeerBase photonPeer { get; }

		/// <summary>
		/// Serailization strategy to use for outgoing messages.
		/// </summary>
		private ISerializerStrategy serializerStrategy { get; }

		/// <summary>
		/// Creates a new Photon compatible <see cref="INetworkMessageRouterService"/>.
		/// </summary>
		/// <param name="peer"><see cref="PeerBase"/> to route messages to.</param>
		/// <param name="serializer">Serialization strategy to use for outgoing messages.</param>
		public PhotonServerINetworkMessageSenderClientAdapter(PeerBase peer, ISerializerStrategy serializer)
		{
			Throw<ArgumentNullException>.If.IsNull(peer)?.Now(nameof(peer));
			Throw<ArgumentNullException>.If.IsNull(serializer)?.Now(nameof(serializer));

			photonPeer = peer;
			serializerStrategy = serializer;
		}

		/// <summary>
		/// Indicates if the <see cref="OperationType"/> can be sent.
		/// </summary>
		/// <param name="opType">Operation type.</param>
		/// <returns>True if the <paramref name="opType"/> can be sent.</returns>
		public bool CanSend(OperationType opType)
		{
			return opType == OperationType.Event || opType == OperationType.Response;
		}

		/// <summary>
		/// Attempts to send a message using the provided <see cref="PacketPayload"/>.
		/// </summary>
		/// <param name="opType">The outgoing <see cref="OperationType"/>.</param>
		/// <param name="payload">The <see cref="PacketPayload"/> to send with the message.</param>
		/// <param name="deliveryMethod">Delivery method to use for the message.</param>
		/// <param name="encrypt">Indicates if the message should be encrypted.</param>
		/// <param name="channel">Channel to send on.</param>
		/// <returns>Indicates the status of the outgoing message attempt.</returns>
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

		/// <summary>
		/// Sends a Photon event message with the provided parameters
		/// </summary>
		/// <param name="payload">Payload to send with the message.</param>
		/// <param name="unreliable">Indicates if the message should be sent unreliabily.</param>
		/// <param name="encrypt">Indicates if the message should be encrypted.</param>
		/// <param name="channel">Channel to send the message on.</param>
		/// <returns>Indicates the result of sending the message.</returns>
		private Photon.SocketServer.SendResult SendEvent(PacketPayload payload, bool unreliable, bool encrypt, byte channel)
		{
			//Builds the message in a context that Photon understands (dictionary of objects)
			EventData data = new EventData(1, new Dictionary<byte, object>(1) { { 0, SerializePayload(payload) } });

			//Sends the event through Photon's transport layer.
			return photonPeer.SendEvent(data, new SendParameters() { ChannelId = channel, Encrypted = encrypt, Unreliable = unreliable });
		}

		/// <summary>
		/// Sends a Photon response message with the provided parameters
		/// </summary>
		/// <param name="payload">Payload to send with the message.</param>
		/// <param name="unreliable">Indicates if the message should be sent unreliabily.</param>
		/// <param name="encrypt">Indicates if the message should be encrypted.</param>
		/// <param name="channel">Channel to send the message on.</param>
		/// <returns>Indicates the result of sending the message.</returns>
		private Photon.SocketServer.SendResult SendResponse(PacketPayload payload, bool unreliable, bool encrypt, byte channel)
		{
			//Builds the message in a context that Photon understands (dictionary of objects)
			OperationResponse data = new OperationResponse(1, new Dictionary<byte, object>(1) { { 0, SerializePayload(payload) } });

			//Sends the event through Photon's transport layer.
			return photonPeer.SendOperationResponse(data, new SendParameters() { ChannelId = channel, Encrypted = encrypt, Unreliable = unreliable });
		}

		/// <summary>
		/// Serializes the <see cref="PacketPayload"/> provided.
		/// </summary>
		/// <param name="payload">Payload to serialize.</param>
		/// <returns>The serialized packet payload.</returns>
		private byte[] SerializePayload(PacketPayload payload)
		{
			return serializerStrategy.Serialize(payload);
		}

		/// <summary>
		/// Attempts to send a message using the provided <see cref="PacketPayload"/>.
		/// </summary>
		/// <param name="opType">The outgoing <see cref="OperationType"/>.</param>
		/// <param name="payload">The <see cref="PacketPayload"/> to send with the message.</param>
		/// <returns>Indicates the status of the outgoing message attempt.</returns>
		public GladNet.Common.SendResult TrySendMessage<TPacketType>(OperationType opType, TPacketType payload) 
			where TPacketType : PacketPayload, IStaticPayloadParameters
		{
			return TrySendMessage(opType, payload, payload.DeliveryMethod, payload.Encrypted, payload.Channel);
		}

		public GladNet.Common.SendResult TryRouteMessage(IResponseMessage message, DeliveryMethod deliveryMethod, bool encrypt = false, byte channel = 0)
		{
			throw new NotImplementedException();
		}

		public GladNet.Common.SendResult TryRouteMessage(IRequestMessage message, DeliveryMethod deliveryMethod, bool encrypt = false, byte channel = 0)
		{
			throw new NotImplementedException();
		}

		public GladNet.Common.SendResult TryRouteMessage<TMessageType>(TMessageType message, DeliveryMethod deliveryMethod, bool encrypt = false, byte channel = 0) where TMessageType : INetworkMessage, IRoutableMessage, IOperationTypeMappable
		{
			throw new NotImplementedException();
		}
	}
}
