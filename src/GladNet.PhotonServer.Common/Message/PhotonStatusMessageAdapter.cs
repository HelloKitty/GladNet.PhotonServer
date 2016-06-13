using GladNet.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GladNet.PhotonServer.Common
{
	/// <summary>
	/// Adapter for the <see cref="IStatusM"/> interface.
	/// </summary>
	public class PhotonStatusMessageAdapter : IStatusMessage
	{
		/// <summary>
		/// Null/Unused payload in Photon. Always null.
		/// </summary>
		NetSendable<PacketPayload> INetworkMessage.Payload { get; } = null;

		/// <summary>
		/// The <see cref="NetStatus"/> value.
		/// </summary>
		public NetStatus Status { get; }

		/// <summary>
		/// Creates a new adapter for the <see cref="IStatusMessage"/> interface.
		/// </summary>
		/// <param name="payload">Payload to adapt.</param>
		public PhotonStatusMessageAdapter(NetStatus status)
		{
			Status = status;
		}
	}
}
