using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;

namespace LoopVideo.AppService
{
    /// <summary>
    /// The singleton connection to the apps being serviced
    /// </summary>
    class AppConnection : IDisposable
    {
        // The instance of this class
        private static readonly Lazy<AppConnection> _instance = new Lazy<AppConnection>(() => new AppConnection());


        private AppServiceConnection _connection = null;

        public AppServiceConnection Connection
        {
            get { return _connection; }
            set
            {
                _connection = value;
                _connection.RequestReceived += RequestReceived;

            }
        }

        private AppConnection() { }

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
                if (_connection != null)
                    _connection.Dispose();
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


        public static AppConnection Instance
        {
            get { return _instance.Value; }
        }

        public static bool IsValid
        {
            get { return _instance.IsValueCreated && _instance.Value.Connection != null; }
        }

        /// <summary>
        /// Send a Command message to the head applicaiton
        /// </summary>
        /// <param name="command">The command to send</param>
        public async void SendCommand(LoopyCommand command)
        {
            var message = new ValueSet();
            try
            {
                LoopyCommandHelper.AddToValueSet(command, message);

                var response = await Connection.SendMessageAsync(message);
                if (response.Status == AppServiceResponseStatus.Success)
                {
                    var result = response.Message["Response"];
                    Debug.WriteLine($"The client responded to the command with: {result.ToString()}");
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception Sending action {command.ToString()} : {ex.Message} ");
                throw;
            }
        }

        /// <summary>
        /// Receive messages from the front-end app
        /// </summary>
        /// <param name="sender">The connection the message is from</param>
        /// <param name="args">The argurments fo the message</param>
        /// <remarks>
        /// Currently this method is not used 
        /// </remarks>
        private void RequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            //var requestDefferal = args.GetDeferral();
            Debug.WriteLine($"LoopyWebserver received the following message: {args.Request.Message.ToString()}");
            //requestDefferal.Complete();

        }


    }
}
