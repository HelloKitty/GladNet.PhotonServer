using Common.Logging;
using Common.Logging.Factory;
using ExitGames.Logging;
using ExitGames.Logging.Log4Net;
using log4net;
using log4net.Config;
using log4net.Repository.Hierarchy;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GladNet.PhotonServer.Server
{
	/// <summary>
	/// Adapter for the PhotonServer Log4Net logging service to Common.Logging interface.
	/// </summary>
	public class PhotonServerLog4NetCommonLoggingILogAdapter : AbstractLogger
	{
		/// <summary>
		/// Creates an internal Photon log4net logger for the adapter.
		/// </summary>
		private readonly ILogger internalPhotonLogger = ExitGames.Logging.LogManager.GetCurrentClassLogger();

		/// <summary>
		/// Creates a new Photon Log4Net Common.Logging adapter for logging.
		/// </summary>
		public PhotonServerLog4NetCommonLoggingILogAdapter(string applicationRootPath, string applicationName, string binaryPath)
		{
			ExitGames.Logging.LogManager.SetLoggerFactory(Log4NetLoggerFactory.Instance);
			GlobalContext.Properties["Photon:ApplicationLogPath"] = Path.Combine(applicationRootPath, "log");
			GlobalContext.Properties["LogFileName"] = applicationName;
			XmlConfigurator.ConfigureAndWatch(new FileInfo(Path.Combine(binaryPath, "log4net.config")));
		}

		#region AbstractLogger implementation

		public override bool IsDebugEnabled { get { return internalPhotonLogger.IsDebugEnabled; } }

		public override bool IsErrorEnabled { get { return internalPhotonLogger.IsErrorEnabled; } }

		public override bool IsFatalEnabled { get { return internalPhotonLogger.IsFatalEnabled; } }

		public override bool IsInfoEnabled { get { return internalPhotonLogger.IsInfoEnabled; } }

		public override bool IsTraceEnabled { get { return internalPhotonLogger.IsDebugEnabled; } }

		public override bool IsWarnEnabled { get { return internalPhotonLogger.IsWarnEnabled; } }

		protected override void WriteInternal(LogLevel level, object message, Exception exception)
		{
			switch (level)
			{
				case LogLevel.All:
					internalPhotonLogger.Error(message, exception);
					break;
				case LogLevel.Trace:
					internalPhotonLogger.Debug(message, exception);
					break;
				case LogLevel.Debug:
					internalPhotonLogger.Debug(message, exception);
					break;
				case LogLevel.Info:
					internalPhotonLogger.Info(message, exception);
					break;
				case LogLevel.Warn:
					internalPhotonLogger.Warn(message, exception);
					break;
				case LogLevel.Error:
					internalPhotonLogger.Error(message, exception);
					break;
				case LogLevel.Fatal:
					internalPhotonLogger.Fatal(message, exception);
					break;
				case LogLevel.Off:
					//If logging is off then we don't need to do anything.
					break;
				default:
					throw new InvalidOperationException("Failed to determine log level of message.");
			}
		}

		#endregion
	}
}
