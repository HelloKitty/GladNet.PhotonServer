using GladNet.Common;
using GladNet.Message;
using GladNet.Payload;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GladNet.Serializer;

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
		NetSendable<PacketPayload> IPayloadContainer.Payload { get; } = null;

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

		//This shouldn't be called
		public byte[] SerializeWithVisitor(ISerializerStrategy serializer)
		{
			throw new NotImplementedException($"{nameof(PhotonStatusMessageAdapter)} doesn't need to serialize the fake status message.");
		}
	}
}