using GladNet.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GladNet.PhotonServer.Client
{
	public interface IClientNetworkMessageReciever
	{
		/// <summary>
		/// Called internally when a response is recieved.
		/// </summary>
		/// <param name="payload"><see cref="PacketPayload"/> sent by the peer.</param>
		/// <param name="parameters">Parameters the message was sent with.</param>
		void OnReceiveResponse(PacketPayload payload);

		/// <summary>
		/// Called internally when an event is recieved.
		/// </summary>
		/// <param name="payload"><see cref="PacketPayload"/> sent by the peer.</param>
		/// <param name="parameters">Parameters the message was sent with.</param>
		void OnReceiveEvent(PacketPayload payload);

		void OnStatusChanged(NetStatus status);
	}
}
