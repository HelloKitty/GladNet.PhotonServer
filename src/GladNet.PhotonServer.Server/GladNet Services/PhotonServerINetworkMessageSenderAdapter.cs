using GladNet.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GladNet.PhotonServer.Server
{
	public class PhotonServerINetworkMessageSenderAdapter : INetworkMessageSender
	{
		public PhotonServerINetworkMessageSenderAdapter()
		{

		}

		public bool CanSend(OperationType opType)
		{
			throw new NotImplementedException();
		}

		public SendResult TrySendMessage(OperationType opType, PacketPayload payload, DeliveryMethod deliveryMethod, bool encrypt = false, byte channel = 0)
		{
			throw new NotImplementedException();
		}

		public SendResult TrySendMessage<TPacketType>(OperationType opType, TPacketType payload) where TPacketType : PacketPayload, IStaticPayloadParameters
		{
			throw new NotImplementedException();
		}
	}
}
