using GladNet.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GladNet.PhotonServer.Common
{
	public class PhotonStatusMessageAdapter : IStatusMessage
	{
		public NetSendable<PacketPayload> Payload { get; }

		public NetStatus Status { get; }

		public PhotonStatusMessageAdapter(NetStatus status)
		{
			Status = status;
		}
	}
}
