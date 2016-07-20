using GladNet.Common;
using GladNet.Engine.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GladNet.PhotonServer.Common
{
	/// <summary>
	/// Contract for objects that contain related <see cref="Peer"/>s.
	/// </summary>
	public interface IPeerContainer
	{
		/// <summary>
		/// <see cref="Peer"/> instance being contained.
		/// </summary>
		Peer GladNetPeer { get; }
	}
}
