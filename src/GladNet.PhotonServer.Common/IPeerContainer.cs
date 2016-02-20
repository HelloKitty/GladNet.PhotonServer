using GladNet.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GladNet.PhotonServer.Common
{
	public interface IPeerContainer
	{
		Peer GladNetPeer { get; }
	}
}
