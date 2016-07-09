using GladNet.Common;
using GladNet.Message;
using Photon.SocketServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GladNet.PhotonServer.Server
{
	/// <summary>
	/// Adapter for <see cref="IMessageParameters"/> interface that adapts <see cref="SendParameters"/>.
	/// </summary>
	public class PhotonMessageParametersAdapter : IMessageParameters
	{
		/// <summary>
		/// Indicates the channel of the message.
		/// </summary>
		public byte Channel { get { return photonParameters.ChannelId; } }

		/// <summary>
		/// Indicates the <see cref="DeliveryMethod"/> method of the message. Can/should be used to verify correct channel usage.
		/// </summary>
		public DeliveryMethod DeliveryMethod
		{
			get
			{
				return photonParameters.Unreliable ? DeliveryMethod.UnreliableDiscardStale : DeliveryMethod.ReliableOrdered;
			}
		}

		/// <summary>
		/// Indicates if the messge is/was encrypted depending on context.
		/// </summary>
		public bool Encrypted { get { return photonParameters.Encrypted; } }

		/// <summary>
		/// Sendparameters beind adapted to <see cref="IMessageParameters"/>.
		/// </summary>
		private SendParameters photonParameters { get; }

		/// <summary>
		/// Creates a new adapter for the <see cref="IMessageParameters"/> interface.
		/// </summary>
		/// <param name="parameters">SendParameters to adapt.</param>
		public PhotonMessageParametersAdapter(SendParameters parameters)
		{
			photonParameters = parameters;
		}
	}
}
