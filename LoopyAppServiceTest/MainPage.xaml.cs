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
                _log.Infomation($"Setting Connection Status {value.ToString()}");
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
                _log.Infomation($"Setting Playback Status {value.ToString()}");
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
                    _log.Infomation("Disposing of old connection");
                    _serviceConnection.MessageReceived -= CommandReceived;
                    _serviceConnection.Dispose();
                }
                _serviceConnection = value;
                if (_serviceConnection != null)
                {
                    _log.Infomation("new connection set");
                    _serviceConnection.MessageReceived += CommandReceived;
                }
            }
        }

        private ValueSet CommandReceived(ValueSet set)
        {
            _log.Infomation($"Command received: {set.ToString()}");
            ValueSet retset = new ValueSet();
            foreach(var pair in set)
            {
                retset.Add(pair);
            }

            return set;
        }

        public MainPage()
        {
            this.InitializeComponent();
            _log.Infomation("MainPage Created");
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            _log.Infomation("MainPage Loaded");
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
                _log.Infomation("Starting the connection");
                //if (ServiceConnection == null)
                //{
                //    ServiceConnection = new AppConnection();
                //}
                ConnectionStatus = (await ServiceConnection.OpenConnectionAsync()).ToString();
            }
        }

        private async void SendPlaybackCommand(LoopyCommand.CommandType command, string param = "")
        {

            _log.Infomation("Opening the connection to the service");
            
            if (!ServiceConnection.IsValid())
            {
                _log.Error("Failed to connect to the service");
                return;
            }

            LoopyCommand lc = new LoopyCommand(command, param);
            try
            {
                var messageSet = lc.ToValueSet();

                _log.Infomation($"Sending Value Set {ValueSetOut.ToString(messageSet)}");
                AppServiceResponse response = await ServiceConnection.SendCommandAsync(messageSet);
                if (response.Status == AppServiceResponseStatus.Success)
                {
                    PlaybackStatus = ValueSetOut.ToString(response.Message);
                }
                else
                {
                    PlaybackStatus = $"Sending Command Failed: {response.Status.ToString()}";
                }
            }
            catch (Exception ex)
            {
                PlaybackStatus = ex.Message;
            }
            _log.Infomation($"SendCommand exit with PlaybackStatus: {PlaybackStatus}");
        }

        private void Play_Click(object sender, RoutedEventArgs e)
        {
            SendPlaybackCommand(LoopyCommand.CommandType.Play);
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            SendPlaybackCommand(LoopyCommand.CommandType.Stop);
        }

    }
}
