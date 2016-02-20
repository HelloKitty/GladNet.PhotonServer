using GladNet.Common;
using GladNet.Serializer;
using GladNet.Server.Common;
using Logging.Services;
using Moq;
using NUnit.Framework;
using Photon.SocketServer;
using PhotonHostRuntimeInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GladNet.PhotonServer.Server.Tests.UnitTests
{
	[TestFixture]
	public static class GladNetPeerBaseTests
	{
		[Test]
		public static void Test_Ctor_Doesnt_Throw()
		{
			//arrange
			GladNetPeerBase peer = new GladNetPeerBase(Mock.Of<IRpcProtocol>(), Mock.Of<IPhotonPeer>(), Mock.Of<INetworkMessageReceiver>(), Mock.Of<IDeserializerStrategy>(), Mock.Of<IDisconnectionServiceHandler>());
		}

		[Test]
		public static void Test_OnRequest_Forwards_To_Reciever()
		{
			//arrange
			Mock<INetworkMessageReceiver> reciever = new Mock<INetworkMessageReceiver>();
			reciever.Setup(x => x.OnNetworkMessageReceive(It.IsAny<IRequestMessage>(), It.IsAny<IMessageParameters>()));

			Mock<IDeserializerStrategy> deserializer = new Mock<IDeserializerStrategy>();

			deserializer.Setup(x => x.Deserialize<PacketPayload>(It.IsAny<byte[]>()))
				.Returns(Mock.Of<PacketPayload>());

			GladNetPeerBase peer = new GladNetPeerBase(Mock.Of<IRpcProtocol>(), Mock.Of<IPhotonPeer>(), reciever.Object, deserializer.Object, Mock.Of<IDisconnectionServiceHandler>());

			OperationRequest request = new OperationRequest() { Parameters = new Dictionary<byte, object>() { { 1, new byte[2] } } };

			//assert
			typeof(GladNetPeerBase)
				.GetMethod("OnOperationRequest", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
				.Invoke(peer, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic, null, new object[] { request, new SendParameters() }, null);

			reciever.Verify(x => x.OnNetworkMessageReceive(It.IsAny<IRequestMessage>(), It.IsAny<IMessageParameters>()), Times.Once());
		}

		[Test]
		public static void Test_Disconnect_Calls_Disconnection_Events()
		{
			//arrange
			Mock<IDisconnectionServiceHandler> disconnectionHandler = new Mock<IDisconnectionServiceHandler>();
			GladNetPeerBase peer = new GladNetPeerBase(Mock.Of<IRpcProtocol>(), Mock.Of<IPhotonPeer>(), Mock.Of<INetworkMessageReceiver>(), Mock.Of<IDeserializerStrategy>(), disconnectionHandler.Object);

			//act
			peer.GetType().GetMethod("OnDisconnect", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
				.Invoke(peer, new object[2] { DisconnectReason.ClientDisconnect, "" } );//disconnection (and hopefully calls disconnect on the service)

			//assert
			disconnectionHandler.Verify(x => x.Disconnect(), Times.Once());
		}
	}
}
