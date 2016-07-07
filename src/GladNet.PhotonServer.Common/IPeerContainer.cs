using GladNet.Common;
using GladNet.Engine.Common;
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
