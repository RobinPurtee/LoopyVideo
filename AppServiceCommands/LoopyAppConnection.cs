using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;
using Windows.Foundation;
using Windows.Foundation.Collections;
using LoopyVideo.Logging;
namespace LoopyVideo.Commands
{

    internal static class ValueSetOut
    {
        public static string ToString(ValueSet values)
        {
            StringBuilder valueBuilder = new StringBuilder();
            foreach(var pair in values)
            {
                if(valueBuilder.Length > 0)
                {
                    valueBuilder.Append(" | ");
                }
                valueBuilder.Append($"Key: {pair.Key} Value: {pair.Value}");
            }

            return valueBuilder.ToString();
        }
    }

    /// <summary>
    /// The connection between LoopyVideo.AppService and  the apps being serviced
    /// </summary>
    internal class AppConnection : IDisposable
    {
        private Logger _log;

        private readonly static string _serviceNameDefault = "net.manipulatormanor.LoopyWebServer";
        //private readonly static string _serviceFamilyNameDefault = "LoopyVideo.AppService-uwp_n1q2psqd6svm2";
        private readonly static string _serviceFamilyNameDefault = "18c4bd8e-3ea4-4e16-b996-b831b06ab7c9_n1q2psqd6svm2";

        public delegate ValueSet ReceiveMessage(ValueSet message);
        public event ReceiveMessage MessageReceived;


        // The instance of this class

        private string _serviceName;
        public string ServiceName
        {
            get
            {
                if(string.IsNullOrEmpty(_serviceName))
                {
                    _serviceName = _serviceNameDefault;
                }
                return _serviceName;
            }
            set { _serviceName = value; }
        }

        private string _familyName;
        public string ServiceFamilyName
        {
            get
            {
                if (string.IsNullOrEmpty(_familyName))
                {
                    _familyName = _serviceFamilyNameDefault;
                }
                return _familyName;
            }
            set { _familyName = value; }
        }

        private object _connectionLock = new object();    
        private AppServiceConnection _connection;
        public AppServiceConnection Connection
        {
            get { return _connection; }
            set
            {
                lock (_connectionLock)
                {
                    if(_connection != null)
                    {
                        _connection.RequestReceived -= RequestReceived;
                        _connection.ServiceClosed -= ServiceClosed;
                        _connection.Dispose();
                    }
                    _connection = value;
                    if (_connection != null)
                    {
                        _connection.RequestReceived += RequestReceived;
                        _connection.ServiceClosed += ServiceClosed;
                        Status = AppServiceConnectionStatus.Success;
                    }
                }
            }
        }

 
        public AppServiceConnectionStatus Status
        {
            get;
            private set;
        }

        public bool IsValid()
        {
            _log.Infomation($"AppConnection.IsValid: {((null != Connection) ? "is Connected" : "is Not connected}")} and Status : {Status.ToString()}");
            return (null != Connection) && (Status == AppServiceConnectionStatus.Success);
        }


        /// <summary>
        /// Default constructor
        /// </summary>
        public AppConnection() : this("AppConnection")
        {
        }

        public AppConnection(string logProviderName)
        {
            _log = new Logger(logProviderName);
            _connection = null;
            Status = AppServiceConnectionStatus.Unknown;
        }

        ~AppConnection()
        {
            Dispose(true);
        }

        #region IDisposable Members

        /// <summary>
        /// Internal variable which checks if Dispose has already been called
        /// </summary>
        private Boolean disposed;

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        private void Dispose(Boolean disposing)
        {
            if (disposed)
            {
                return;
            }

            if (disposing)
            {
                Connection = null;
                if (_log != null)
                {
                    _log.Dispose();
                }
            }

            disposed = true;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            // Call the private Dispose(bool) helper and indicate 
            // that we are explicitly disposing
            this.Dispose(true);

            // Tell the garbage collector that the object doesn't require any
            // cleanup when collected since Dispose was called explicitly.
            GC.SuppressFinalize(this);
        }

        #endregion

        public IAsyncOperation<AppServiceConnectionStatus> OpenConnectionAsync()
        {
            _log.Infomation("OpenConnectionAsync: called");
            return Task<AppServiceConnectionStatus>.Run(async () =>
            {
                _log.Infomation("OpenConnectionAsync: Creating App Service Connection");
                AppServiceConnection connection = new AppServiceConnection();

                // Here, we use the app service name defined in the app service provider's Package.appxmanifest file in the <Extension> section.
                connection.AppServiceName = ServiceName;

                // Use Windows.ApplicationModel.Package.Current.Id.FamilyName within the app service provider to get this value.
                connection.PackageFamilyName = ServiceFamilyName;

                Status = await connection.OpenAsync();
                bool bRet = Status == AppServiceConnectionStatus.Success;
                _log.Infomation($"OpenConnectionAsync: Connection Status = {Status.ToString()}");

                if (bRet)
                {
                    Connection = connection;
                }
                return Status;
            }).AsAsyncOperation<AppServiceConnectionStatus>();
        }

        /// <summary>
        /// Send a Command message to the head applicaiton
        /// </summary>
        /// <param name="command">The command to send</param>
        /// <returns>The ValueSet containing the response</returns>
        public IAsyncOperation<AppServiceResponse> SendCommandAsync(ValueSet command)
        {
            IAsyncOperation<AppServiceResponse> ret = null;
            if (IsValid())
            {
                _log.Infomation($"SendCommandAsync: Sending {ValueSetOut.ToString(command)}");
                ret = Connection.SendMessageAsync(command);
            }
            else
            {
                _log.Infomation("SendCommandAsync: called before the connection is valid");
                throw new InvalidOperationException("Cannot send a command until connection is opened");
            }
            return ret;
        }


        /// <summary>
        /// Receive messages from the other app
        /// </summary>
        /// <param name="sender">The connection the message is from</param>
        /// <param name="args">The argurments fo the message</param>
        private async void RequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            if(!IsValid())
            {
                _log.Infomation("SendCommandAsync: called before the connection is valid");
                throw new InvalidOperationException("Message recieved before connection is opened");
            }
            var requestDefferal = args.GetDeferral();
            try
            {

                _log.Infomation($"AppConnection.RequestReceived: received the following message: {ValueSetOut.ToString(args.Request.Message)}");
                AppServiceResponseStatus status = AppServiceResponseStatus.Unknown;
                if (MessageReceived != null)
                {
                    status = await args.Request.SendResponseAsync(MessageReceived(args.Request.Message));
                }

                _log.Infomation($"Sending Response to Request returned: {status.ToString()}");
            }
            finally
            {
                requestDefferal.Complete();
            }
        }

        private void ServiceClosed(AppServiceConnection sender, AppServiceClosedEventArgs args)
        {
            _log.Infomation($"AppService Connection has be closed by: {args.Status.ToString()}");
            Connection = null;
            Status = AppServiceConnectionStatus.Unknown;
        }

    }
}
