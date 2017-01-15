using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LoopyVideo.Commands;
using Windows.ApplicationModel.Background;


namespace LoopyVideo
{
    internal class WebServerManager : IDisposable
    {
        private static string serverName = "LoopyVideo.WebServer";
        private static string serverEntryPoint = "LoopyVideo.WebServer.Server";
        private static Logging.Logger _log = new Logging.Logger("WebServerManager");


        private BackgroundTaskRegistration _server = null;
        private bool _registered = false;

        public bool IsRegistered
        {
            get
            {

                if (_server == null)
                {
                    foreach (var task in BackgroundTaskRegistration.AllTasks)
                    {
                        if (task.Value.Name == serverName)
                        {
                            _server = (BackgroundTaskRegistration)task.Value;
                            _registered = false;
                            break;
                        }
                    }

                }
                return _server != null;
            }
        }

        public WebServerManager()
        {

        }

        ~WebServerManager()
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
                End();
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


        public async void StartAsync()
        {
            bool accessAllowed = false;
            BackgroundAccessStatus bgAccess = await BackgroundExecutionManager.RequestAccessAsync();
            switch(bgAccess)
            {
                case BackgroundAccessStatus.DeniedBySystemPolicy:
                    break;
                case BackgroundAccessStatus.DeniedByUser:
                    break;
                //case BackgroundAccessStatus.AllowedSubjectToSystemPolicy:
                //case BackgroundAccessStatus.AlwaysAllowed:
                default:
                    accessAllowed = true;
                    break;
            }
            if(accessAllowed && !IsRegistered)
            {
                BackgroundTaskBuilder builder = new BackgroundTaskBuilder();
                builder.Name = serverName;
                builder.TaskEntryPoint = serverEntryPoint;
                builder.IsNetworkRequested = true;
                builder.SetTrigger(new ApplicationTrigger());
                builder.AddCondition(new SystemCondition(SystemConditionType.FreeNetworkAvailable));
                try
                {
                    _server = builder.Register();
                    _registered = true;
                    _server.Completed += OnServerExit;
                }
                catch (Exception ex)
                {
                    _log.Error($"Background Registration Exception: {ex.Message}");
                    throw;
                }              
            }
        }

        private void OnServerExit(IBackgroundTaskRegistration sender, BackgroundTaskCompletedEventArgs args)
        {
            _log.Infomation("Backgroud service exited ");
            if (_registered)
            {
                _server.Unregister(false);
            }
            _registered = false;
        }

        public void End()
        {
            if (_registered)
            {
                _server.Unregister(false);
                _registered = false;
            }
        }

    }
}
