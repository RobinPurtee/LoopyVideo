using System;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using LoopyVideo.Logging;
using LoopyVideo.Commands;

namespace LoopyVideo
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {

        Logger _log = new Logger("LoopyVideo.App");


        private PlayerModel _playerModel = new PlayerModel();

        public PlayerModel Player
        {
            get { return _playerModel; }
            set { _playerModel = value; }
        }

        /// <summary>
        /// The connection to the LoopyVideo.Webservice
        /// </summary>
        private AppConnection _serviceConnection;
        public AppConnection ServiceConnection
        {
            get { return _serviceConnection; }
            set
            {
                if (_serviceConnection != null)
                {
                    _log.Information("Disposing of old connection");
                    _serviceConnection.MessageReceived -= CommandReceived;
                    _serviceConnection.Dispose();
                }
                _serviceConnection = value;
                if (_serviceConnection != null)
                {
                    _log.Information("new connection set");
                    _serviceConnection.MessageReceived += CommandReceived;
                }
            }
        }

        private LoopyCommand CommandReceived(LoopyCommand command)
        {
            _log.Information($"Command received: {command.ToString()}");
            LoopyCommand retCommand = new LoopyCommand(LoopyCommand.CommandType.Error, $"Unsupported command type {command.Command.ToString()}");
            switch (command.Command)
            {
                case LoopyCommand.CommandType.Play:
                    if(Player.IsValid)
                    {
                        Player.Play();
                        retCommand.Copy(command);
                    }
                    else
                    {
                        retCommand.Param = "";
                    }
                    break;
                case LoopyCommand.CommandType.Stop:
                    if (Player.IsValid)
                    {
                        Player.Pause();
                        retCommand.Copy(command);
                    }
                    else
                    {
                        retCommand.Param = "";
                    }
                    break;
                case LoopyCommand.CommandType.Media:

                default:
                    break;

            }
            return retCommand;
        }


        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
        }

        public string GetErrorString(string errorName)
        {
            var loader = new Windows.ApplicationModel.Resources.ResourceLoader();
            return loader.GetString(errorName);

        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            //#if DEBUG
            //            if (System.Diagnostics.Debugger.IsAttached)
            //            {
            //                this.DebugSettings.EnableFrameRateCounter = true;
            //            }
            //#endif

            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    // When the navigation stack isn't restored navigate to the first page,
                    // configuring the new page by passing required information as a navigation
                    // parameter
                    rootFrame.Navigate(typeof(MainPage), e.Arguments);
                }
                // Ensure the current window is active
                Window.Current.Activate();

                if (ServiceConnection == null)
                {
                    ServiceConnection = new AppConnection("WebAppServiceClient.ServiceConnection");
                }
                // has no messages are sent to service from this application and connection
                // status can be determined from the connection object if needed.
                // is is not necessory to wait for the connection process to complete here
                #pragma warning disable 4014
                ServiceConnection.OpenConnectionAsync();
                #pragma warning restore 4014
            }
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }
    }
}
