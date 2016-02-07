using GladNet.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GladNet.PhotonServer.Server
{
	public class PhotonRequestMessageAdapter : IRequestMessage
	{
		public NetSendable<PacketPayload> Payload { get; private set; }

		public PhotonRequestMessageAdapter(PacketPayload payload)
		{
			//Not great but in Production we won't use Photon.
			Payload = new NetSendable<PacketPayload>(payload);
		}
	}
}
