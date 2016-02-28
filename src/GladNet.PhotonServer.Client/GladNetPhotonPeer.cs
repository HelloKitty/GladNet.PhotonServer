using ExitGames.Client.Photon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GladNet.PhotonServer.Client
{
	public class GladNetPhotonPeer : PhotonPeer
	{
		public GladNetPhotonPeer(IPhotonPeerListener listener, ConnectionProtocol protocolType) 
			: base(listener, protocolType)
		{

		}
	}
}
