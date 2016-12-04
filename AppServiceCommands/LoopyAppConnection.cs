using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace LoopyVideo.Commands
{
    internal static class LoopyAppConnectionFactory
    {
        private static readonly Lazy<LoopyAppConnection> _instance = new Lazy<LoopyAppConnection>(() => new LoopyAppConnection());
        internal static LoopyAppConnection Instance
        {
            get { return _instance.Value; }
        }
        internal static bool IsValid
        {
            get { return _instance.IsValueCreated && _instance.Value.Connection != null; }
        }

    }

    /// <summary>
    /// The connection between LoopyVideo.AppService and  the apps being serviced
    /// </summary>
    internal class LoopyAppConnection : IDisposable
    {

        private readonly static string ServiceName = "net.manipulatormanor.LoopyWebServer";
        private readonly static string ServiceFamilyName = "LoopyVideo.AppService-uwp_n1q2psqd6svm2";


        public event EventHandler<LoopyCommand> ReceiveCommand;


        // The instance of this class

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
            return (null != Connection) && (Status == AppServiceConnectionStatus.Success);
        }


        /// <summary>
        /// Hidden default constructor
        /// </summary>
        public LoopyAppConnection()
        {
            _connection = null;
            Status = AppServiceConnectionStatus.Unknown;
        }

        ~LoopyAppConnection()
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

        public IAsyncOperation<bool> OpenConnectionAsync()
        {
            return Task<bool>.Run(async () =>
            {
                AppServiceConnection connection = new AppServiceConnection();

                // Here, we use the app service name defined in the app service provider's Package.appxmanifest file in the <Extension> section.
                connection.AppServiceName = ServiceName;

                // Use Windows.ApplicationModel.Package.Current.Id.FamilyName within the app service provider to get this value.
                connection.PackageFamilyName = ServiceFamilyName;

                Status = await connection.OpenAsync();
                bool bRet = Status == AppServiceConnectionStatus.Success;

                if (bRet)
                {
                    Connection = connection;
                }
                return bRet;
            }).AsAsyncOperation<bool>();
        }

        /// <summary>
        /// Send a Command message to the head applicaiton
        /// </summary>
        /// <param name="command">The command to send</param>
        /// <returns>The ValueSet containing the response</returns>
        public IAsyncOperation<ValueSet> SendCommandAsync(LoopyCommand command)
        {
            return Task<ValueSet>.Run(async () => { 
                ValueSet outValueSet = new ValueSet();
                ValueSet result;
                try
                {
                    LoopyCommandHelper.AddToValueSet(command, outValueSet);

                    AppServiceResponse response = await Connection.SendMessageAsync(outValueSet);
                    if (response.Status == AppServiceResponseStatus.Success)
                    {
                        result = response.Message;
                        Debug.WriteLine($"The client responded to the command with: {result.ToString()}");
                    }
                    else
                    {
                        result = new ValueSet();
                        result.Clear();
                    }

                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Exception Sending action {command.ToString()} : {ex.Message} ");
                    throw;
                }
                return result;
            }).AsAsyncOperation<ValueSet>();
        }


        /// <summary>
        /// Receive messages from the other app
        /// </summary>
        /// <param name="sender">The connection the message is from</param>
        /// <param name="args">The argurments fo the message</param>
        private void RequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            var requestDefferal = args.GetDeferral();
            Debug.WriteLine($"LoopyWebserver received the following message: {args.Request.Message.ToString()}");
            if (ReceiveCommand != null)
            {
                LoopyCommand lc = LoopyCommandHelper.FromValueSet(args.Request.Message);
                ReceiveCommand(this, lc);
            }

            requestDefferal.Complete();
        }

        private void ServiceClosed(AppServiceConnection sender, AppServiceClosedEventArgs args)
        {
            Debug.WriteLine($"AppService Connection has be closed by: {args.ToString()}");
            Connection = null;
            Status = AppServiceConnectionStatus.Unknown;
        }

    }
}
