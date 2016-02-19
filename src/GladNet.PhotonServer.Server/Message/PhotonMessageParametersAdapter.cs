using GladNet.Common;
using Photon.SocketServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GladNet.PhotonServer.Server
{
	/// <summary>
	/// Adapter for <see cref="IMessageParameters"/> interface that adapts <see cref="SendParameters"/>.
	/// </summary>
	public class PhotonMessageParametersAdapter : IMessageParameters
	{
		public byte Channel
		{
			get
			{
				return photonParameters.ChannelId;
			}
		}

		public DeliveryMethod DeliveryMethod
		{
			get
			{
				return photonParameters.Unreliable ? DeliveryMethod.UnreliableDiscardStale : DeliveryMethod.ReliableOrdered;
			}
		}

		public bool Encrypted
		{
			get
			{
				return photonParameters.Encrypted;
			}
		}

		private readonly SendParameters photonParameters;

		public PhotonMessageParametersAdapter(SendParameters parameters)
		{
			photonParameters = parameters;
		}
	}
}
