using Easyception;
using GladNet.Common;
using GladNet.Engine.Common;
using GladNet.Message;
using GladNet.Payload;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GladNet.PhotonServer.Client
{
	/// <summary>
	/// Adapter that adapts the <typeparamref name="TSendingService"/> to fit the
	/// <see cref="INetworkMessageRouterService"/> interface.
	/// </summary>
	public class UnityClientPeerNetworkMessageSenderAdapter<TSendingService> : INetworkMessageRouterService
		where TSendingService : IClientPeerNetworkMessageRouter, IClientPeerPayloadSender
	{
		/// <summary>
		/// Adaptee object.
		/// </summary>
		private TSendingService messageSendingService { get; }

		/// <summary>
		/// Creates a new adapter that adapts the <see cref="IClientPeerNetworkMessageSender"/> interface to fit the
		/// <see cref="INetworkMessageSender"/> interface.
		/// </summary>
		/// <param name="messageRouterService">Client message sending object/service.</param>
		public UnityClientPeerNetworkMessageSenderAdapter(TSendingService messageRouterService)
		{
			messageSendingService = messageRouterService;
		}
		
		/// <summary>
		/// Indicates if the <see cref="OperationType"/> can be sent.
		/// </summary>
		/// <param name="opType">Operation type.</param>
		/// <returns>True if the operation can be sent (This particular sender can only send Requests)</returns>
		public bool CanSend(OperationType opType)
		{
			return opType == OperationType.Request;
		}

		public SendResult TrySendMessage(OperationType opType, PacketPayload payload, DeliveryMethod deliveryMethod, bool encrypt = false, byte channel = 0)
		{
			if (opType == OperationType.Request)
				return messageSendingService.SendRequest(payload, deliveryMethod, encrypt, channel);
			else
				throw new ArgumentException($"Cannot send OperationType: {opType} from client peers.", nameof(opType));
		}

		public SendResult TrySendMessage<TPacketType>(OperationType opType, TPacketType payload) where TPacketType : PacketPayload, IStaticPayloadParameters
		{
			if (opType == OperationType.Request)
				return messageSendingService.SendRequest(payload);
			else
				throw new ArgumentException($"Cannot send OperationType: {opType} from client peers.", nameof(opType));
		}

		/// <summary>
		/// Tries to send the <typeparamref name="TMessageType"/> message without routing semantics.
		/// </summary>
		/// <typeparam name="TMessageType">A <see cref="INetworkMessage"/> type that implements <see cref="IRoutableMessage"/>.</typeparam>
		/// <param name="message"><typeparamref name="TMessageType"/> to be sent.</param>
		/// <param name="deliveryMethod">The deseried <see cref="DeliveryMethod"/> of the message.</param>
		/// <param name="encrypt">Indicates if the message should be encrypted.</param>
		/// <param name="channel">Indicates the channel for this message to be sent over.</param>
		/// <exception cref="InvalidOperationException">Throws this if the <see cref="OperationType"/> is not a Request.</exception>
		/// <returns>Indication of the message send state.</returns>
		public SendResult TryRouteMessage<TMessageType>(TMessageType message, DeliveryMethod deliveryMethod, bool encrypt = false, byte channel = 0) 
			where TMessageType : INetworkMessage, IRoutableMessage, IOperationTypeMappable
		{
			//We can only route requests on clients
			switch (message.OperationTypeMappedValue)
			{
				case OperationType.Request:
					return messageSendingService.RouteRequest((IRequestMessage)message, deliveryMethod, encrypt, channel);
				case OperationType.Event:
				case OperationType.Response:
				default:
					Throw<ArgumentException>.If.Now($"Provided {nameof(TMessageType)} is invalid with OpType: {message.OperationTypeMappedValue}.", nameof(message));
					return SendResult.Invalid;
			}
		}
	}
}
