using GladNet.Common;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GladNet.PhotonServer.Server.UnitTests
{
	[TestFixture]
	public static class PhotonMessageParametersAdapterTests
	{
		[Test]
		public static void Test_Constructor_Doesnt_Throw()
		{
			//assert
			Assert.DoesNotThrow(() => new PhotonMessageParametersAdapter(new Photon.SocketServer.SendParameters()));
		}

		[Test]
		[TestCase(true, DeliveryMethod.UnreliableDiscardStale)]
		[TestCase(false, DeliveryMethod.ReliableOrdered)]
		public static void Test_DeliveryMethod_Is_As_Expected(bool isUnreliable, DeliveryMethod expectedMethod)
		{
			//arrange
			PhotonMessageParametersAdapter parameters = new PhotonMessageParametersAdapter(new Photon.SocketServer.SendParameters() { Unreliable = isUnreliable });

			//assert
			Assert.AreEqual(expectedMethod, parameters.DeliveryMethod);
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public static void Test_Encrypted_Is_Encrypted_After_Adapting(bool isEncrypted)
		{
			//arrange
			PhotonMessageParametersAdapter parameters = new PhotonMessageParametersAdapter(new Photon.SocketServer.SendParameters() { Encrypted = isEncrypted });

			//assert
			Assert.IsTrue(isEncrypted == parameters.Encrypted);
		}
	}
}
