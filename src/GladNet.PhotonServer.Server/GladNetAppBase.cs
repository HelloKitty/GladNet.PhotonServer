using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Photon.SocketServer;
using GladNet.Common;
using Common.Logging;
using GladNet.Serializer;
using Photon.SocketServer.ServerToServer;
using GladNet.Engine.Common;
using GladNet.Engine.Server;
using GladNet.Message;

namespace GladNet.PhotonServer.Server
{
	public abstract class GladNetAppBase : ApplicationBase
	{ 
		/// <summary>
		/// Application logger. Root logger for the <see cref="ApplicationBase"/>.
		/// </summary>
		protected abstract ILog AppLogger { get; set; }

		/// <summary>
		/// Provider for <see cref="ISerializerStrategy"/>s.
		/// </summary>
		public virtual ISerializerStrategy Serializer { get; }

		/// <summary>
		/// Provider for <see cref="IDeserializerStrategy"/>s.
		/// </summary>
		public virtual IDeserializerStrategy Deserializer { get; }

		public virtual ISerializerRegistry SerializerRegistry { get; }

		//These are marked internal because of the god damn PhotonServer implementation that offloads peer initialization to inside some stupid
		//callback method in the outgoing peer.
		internal AUIDServiceCollection<INetPeer> auidMapService { get; }

		internal DefaultNetworkMessageRouteBackService routebackService { get; }

		//prevents inherting from this class: http://stackoverflow.com/questions/1244953/internal-abstract-class-how-to-hide-usage-outside-assembly
		internal GladNetAppBase()
			: base()
		{
			//These new services are required for the GladNet2 2.x routeback feature
			auidMapService = new AUIDServiceCollection<INetPeer>(100);
			routebackService = new DefaultNetworkMessageRouteBackService(auidMapService, AppLogger);
		}

		/// <summary>
		/// Called internally by Photon when a peer is attempting to connect.
		/// Services the connection attempt.
		/// </summary>
		/// <param name="initRequest">Request details.</param>
		/// <returns></returns>
		protected override PeerBase CreatePeer(InitRequest initRequest)
		{
			//Create the details so that the consumer of this class, who extends it, can indicate if this is a request we should service
			//AKA should a peer be made
			IConnectionDetails details = new PhotonServerIConnectionDetailsAdapter(initRequest.RemoteIP, initRequest.RemotePort, initRequest.LocalPort);

			//If we should service the peer
			if (ShouldServiceIncomingPeerConnect(details))
			{
				//Unlike in PhotonServer we have the expectation that they WILL be creating a peer since they said they would
				//Because of this we'll be creating the actual PeerBase in advance.
				NetworkMessagePublisher publisher = new NetworkMessagePublisher();
				IDisconnectionServiceHandler disconnectionHandler = new PhotonServerIDisconnectionServiceHandlerAdapter();

				//Build the peer first since it's required for the network message sender
				GladNetClientPeer peerBase = new GladNetClientPeer(initRequest, publisher, Deserializer, disconnectionHandler);
				//We should make the ClientPeerSession now
				ClientPeerSession session = CreateClientSession(new PhotonServerINetworkMessageSenderClientAdapter(peerBase, Serializer), details, publisher, disconnectionHandler, routebackService);

				if (session == null)
				{
					peerBase.Disconnect();
					return null;
				}

				//Add the ID to the AUID map service and setup removal
				auidMapService.Add(details.ConnectionID, session);
				disconnectionHandler.DisconnectionEventHandler += () => auidMapService.Remove(details.ConnectionID);

				//This must be done to keep alive the reference of the session
				//Otherwise GC will clean it up (WARNING: This will create circular reference and cause a leak if you do not null the peer out eventually)
				peerBase.GladNetPeer = session;

				return peerBase;
			}
			else
			{
				//Disconnect the client if they're not going to have a peer serviced
				initRequest.PhotonPeer.DisconnectClient();

				return null;
			}
		}

		protected GladNetOutboundS2SPeer CreateOutBoundPeer()
		{
			//Services needed to have an outbound peer
			NetworkMessagePublisher publisher = new NetworkMessagePublisher();
			IDisconnectionServiceHandler disconnectionHandler = new PhotonServerIDisconnectionServiceHandlerAdapter();


			return new GladNetOutboundS2SPeer(this, publisher, this.Deserializer, disconnectionHandler);
		}

		/// <summary>
		/// Processes incoming connection details and decides if a connection should be established.
		/// </summary>
		/// <param name="details">Details of the connection.</param>
		/// <returns>Indicates if, based on the details, a connection should be serviced.</returns>
		protected abstract bool ShouldServiceIncomingPeerConnect(IConnectionDetails details);

		/// <summary>
		/// Creates a client session for the incoming connection request.
		/// </summary>
		/// <param name="sender">Message sending service.</param>
		/// <param name="details">Connection details.</param>
		/// <param name="subService">Subscription service for networked messages.</param>
		/// <param name="disconnectHandler">Disconnection handling service.</param>
		/// <returns>A new client session.</returns>
		protected abstract ClientPeerSession CreateClientSession(INetworkMessageRouterService sender, IConnectionDetails details, INetworkMessageSubscriptionService subService,
			IDisconnectionServiceHandler disconnectHandler, INetworkMessageRouteBackService routebackService);

		/// <summary>
		/// Creates a server client session (outbound) for the incoming connection request.
		/// </summary>
		/// <param name="sender">Message sending service.</param>
		/// <param name="details">Connection details.</param>
		/// <param name="subService">Subscription service for networked messages.</param>
		/// <param name="disconnectHandler">Disconnection handling service.</param>
		/// <returns>A new client session.</returns>
		public abstract GladNet.Engine.Common.ClientPeer CreateServerPeer(INetworkMessageRouterService sender, IConnectionDetails details, INetworkMessageSubscriptionService subService,
			IDisconnectionServiceHandler disconnectHandler, INetworkMessageRouteBackService routebackService);

		protected abstract void SetupSerializationRegistration(ISerializerRegistry serializationRegistry);

		/// <summary>
		/// Called internally by Photon when the application is just about to finish startup.
		/// </summary>
		protected sealed override void Setup()
		{
			//We utilize the internal Photon Setup() method
			//as a vector to provide various services to the consumer of this abstract class
			//Ways to register payload types and provide access to other internal services.
			//Not great design but it's better than exposing them as properties and relying on consumers
			//to deal with them there.

			//We also should handle registering network message types
			SerializerRegistry.Register(typeof(NetworkMessage));
			SerializerRegistry.Register(typeof(RequestMessage));
			SerializerRegistry.Register(typeof(ResponseMessage));
			SerializerRegistry.Register(typeof(EventMessage));
			SerializerRegistry.Register(typeof(StatusMessage));

			ServerSetup();
			SetupSerializationRegistration(SerializerRegistry);
		}

		protected abstract void ServerSetup();

		/// <summary>
		/// Called internally by Photon when the application is about to be torn down.
		/// </summary>
		protected override abstract void TearDown();
	}

	/// <summary>
	/// GladNet2 ApplicationBase for Photon applications.
	/// </summary>
	/// <typeparam name="TSerializationStrategy">Concrete serialization strategy.</typeparam>
	/// <typeparam name="TDeserializationStrategy">Concrete deserialization strategy.</typeparam>
	/// <typeparam name="TSerializerRegistry">Concrete serializer registry.</typeparam>
	public abstract class GladNetAppBase<TSerializationStrategy, TDeserializationStrategy, TSerializerRegistry> : GladNetAppBase
		where TSerializationStrategy : ISerializerStrategy, new() where TDeserializationStrategy : IDeserializerStrategy, new() where TSerializerRegistry : ISerializerRegistry, new()
	{	
		/// <summary>
		/// Provider for <see cref="ISerializerStrategy"/>s.
		/// </summary>
		public override ISerializerStrategy Serializer { get; } = new TSerializationStrategy(); //this instantiation is slow but we only do it once.

		/// <summary>
		/// Provider for <see cref="IDeserializerStrategy"/>s.
		/// </summary>
		public override IDeserializerStrategy Deserializer { get; } = new TDeserializationStrategy();  //this instantiation is slow but we only do it once.

		public override ISerializerRegistry SerializerRegistry { get; } = new TSerializerRegistry();
	}
}
