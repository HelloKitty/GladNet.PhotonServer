using GladNet.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

//Use the GladNet.Common namespace for thie extension to work anywhere without reference
namespace GladNet.Common
{
	public static class DeliveryMethodExtensions
	{
		/// <summary>
		/// Indicates if the <see cref="DeliveryMethod"/> value is a reliable method.
		/// </summary>
		/// <param name="method">The delivery method value.</param>
		/// <returns>True if the method is reliable otherwise false.</returns>
		public static bool isReliable(this DeliveryMethod method)
		{
			switch (method)
			{
				case DeliveryMethod.Unknown:
					return false;
				case DeliveryMethod.UnreliableAcceptDuplicate:
					return false;
				case DeliveryMethod.UnreliableDiscardStale:
					return false;
				case DeliveryMethod.ReliableUnordered:
					return true;
				case DeliveryMethod.ReliableDiscardStale:
					return true;
				case DeliveryMethod.ReliableOrdered:
					return true;
				default:
					return false;
			}
		}
	}
}
