using ExitGames.Client.Photon;
using GladNet.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GladNet.PhotonServer.Client
{
	public static class StatusCodeExtensions
	{
		public static NetStatus? ToGladNet(this StatusCode status)
		{
			switch (status)
			{
				case StatusCode.Connect:
					return NetStatus.Connected;
				case StatusCode.Disconnect:
					return NetStatus.Disconnected;
				case StatusCode.DisconnectByServer:
					return NetStatus.Disconnected;
				case StatusCode.DisconnectByServerUserLimit:
					return NetStatus.Disconnected;
				case StatusCode.DisconnectByServerLogic:
					return NetStatus.Disconnected;
				case StatusCode.EncryptionEstablished:
					return NetStatus.EncryptionEstablished;

				case StatusCode.Exception:
				case StatusCode.EncryptionFailedToEstablish:
				case StatusCode.ExceptionOnConnect:
				case StatusCode.SecurityExceptionOnConnect:
				case StatusCode.QueueOutgoingReliableWarning:
				case StatusCode.QueueOutgoingUnreliableWarning:
				case StatusCode.SendError:
				case StatusCode.QueueOutgoingAcksWarning:
				case StatusCode.QueueIncomingReliableWarning:
				case StatusCode.QueueIncomingUnreliableWarning:
				case StatusCode.QueueSentWarning:
				case StatusCode.ExceptionOnReceive:
				case StatusCode.TimeoutDisconnect:
				default:
					return null;
			}
		}
	}
}
