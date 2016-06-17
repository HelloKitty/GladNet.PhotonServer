using GladNet.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GladNet.PhotonServer.Client
{
	/// <summary>
	/// Adapter that adapts the<see cref="IClientPeerNetworkMessageSender"/> interface to fit the
	/// <see cref="INetworkMessageSender"/> interface.
	/// </summary>
	public class UnityClientPeerNetworkMessageSenderAdapter : INetworkMessageSender
	{
		/// <summary>
		/// Adaptee object.
		/// </summary>
		private IClientPeerNetworkMessageSender sender { get; }

		/// <summary>
		/// Creates a new adapter that adapts the <see cref="IClientPeerNetworkMessageSender"/> interface to fit the
		/// <see cref="INetworkMessageSender"/> interface.
		/// </summary>
		/// <param name="clientMessageSender">Client message sending object/service.</param>
		public UnityClientPeerNetworkMessageSenderAdapter(IClientPeerNetworkMessageSender clientMessageSender)
		{
			sender = clientMessageSender;
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
				return sender.SendRequest(payload, deliveryMethod, encrypt, channel);
			else
				throw new ArgumentException($"Cannot send OperationType: {opType} from client peers.", nameof(opType));
		}

		public SendResult TrySendMessage<TPacketType>(OperationType opType, TPacketType payload) where TPacketType : PacketPayload, IStaticPayloadParameters
		{
			if (opType == OperationType.Request)
				return sender.SendRequest(payload);
			else
				throw new ArgumentException($"Cannot send OperationType: {opType} from client peers.", nameof(opType));
		}
	}
}
