using System;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.ApplicationModel.AppService;
using LoopyVideo.Commands;
using LoopyVideo.Logging;

namespace LoopyAppServiceTest
{
    public sealed partial class MainPage : Page
    {
        private LoopyVideo.Logging.Logger _log = new LoopyVideo.Logging.Logger("LoopyAppServiceTest");

        public string ConnectionStatus
        {
            get { return (string)GetValue(ConnectionStatusProperty); }
            set
            {
                _log.Information($"Setting Connection Status {value.ToString()}");
                SetValue(ConnectionStatusProperty, value);
            }
        }

        // Using a DependencyProperty as the backing store for ConnectionStatus.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ConnectionStatusProperty =
            DependencyProperty.Register("ConnectionStatus", typeof(string), typeof(MainPage), new PropertyMetadata("Unknown"));



        public string PlaybackStatus
        {
            get { return (string)GetValue(PlaybackStatusProperty); }
            set
            {
                _log.Information($"Setting Playback Status {value.ToString()}");
                SetValue(PlaybackStatusProperty, value);
            }
        }

        // Using a DependencyProperty as the backing store for PlaybackStatus.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PlaybackStatusProperty =
            DependencyProperty.Register("PlaybackStatus", typeof(string), typeof(MainPage), new PropertyMetadata("Unknown"));




        private AppConnection _serviceConnection;

        private AppConnection ServiceConnection
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
            // TODO: implement state container
            return command;
        }

        public MainPage()
        {
            this.InitializeComponent();
            _log.Information("MainPage Created");
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            _log.Information("MainPage Loaded");
            DataContext = this;
            if (ServiceConnection == null)
            {
                ServiceConnection = new AppConnection("LoopyVideo.AppServiceTest.ServiceConnection");
            }

            ConnectionStatus = "The initial Status";
        }



        private async void StartService_Click(object sender, RoutedEventArgs e)
        {
            
            // Add the connection.
            if (ServiceConnection == null || !ServiceConnection.IsValid())
            {
                _log.Information("Starting the connection");
                //if (ServiceConnection == null)
                //{
                //    ServiceConnection = new AppConnection();
                //}
                ConnectionStatus = (await ServiceConnection.OpenConnectionAsync()).ToString();
            }
        }

        private async void SendPlaybackCommand(CommandType command, string param = "")
        {

            _log.Information("Opening the connection to the service");
            
            if (!ServiceConnection.IsValid())
            {
                _log.Error("Failed to connect to the service");
                return;
            }

            LoopyCommand lc = new LoopyCommand(command, param);
            try
            {
            
                _log.Information($"Sending Command {lc.ToString()}");
                LoopyCommand response = await ServiceConnection.SendCommandAsync(lc);
                PlaybackStatus = response.ToString();
            }
            catch (Exception ex)
            {
                PlaybackStatus = ex.Message;
            }
            _log.Information($"SendCommand exit with PlaybackStatus: {PlaybackStatus}");
        }

        private void Play_Click(object sender, RoutedEventArgs e)
        {
            SendPlaybackCommand(CommandType.Play);
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            SendPlaybackCommand(CommandType.Stop);
        }

    }
}
