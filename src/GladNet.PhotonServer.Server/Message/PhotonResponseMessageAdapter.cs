using GladNet.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GladNet.PhotonServer.Server
{
	/// <summary>
	/// Adapter for the <see cref="IResponseMessage"/> interface.
	/// </summary>
	public class PhotonResponseMessageAdapter : IRequestMessage
	{
		/// <summary>
		/// The payload of a <see cref="INetworkMessage"/>. Can be sent accross a network.
		/// <see cref="NetSendable"/> enforces its wire readyness.
		/// In Photon we just have to push a payload into it to fit the interface.
		/// </summary>
		public NetSendable<PacketPayload> Payload { get; }

		/// <summary>
		/// Creates a new adapter for the <see cref="IResponseMessage"/> interface.
		/// </summary>
		/// <param name="payload">Payload to adapt.</param>
		public PhotonResponseMessageAdapter(PacketPayload payload)
		{
			//Not great but in Production we won't use Photon.
			Payload = new NetSendable<PacketPayload>(payload);
		}
	}
}
