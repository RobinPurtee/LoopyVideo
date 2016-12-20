using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.ApplicationModel.AppService;
using LoopyVideo.Commands;
using System.Text;

namespace LoopyAppServiceTest
{
    public sealed partial class MainPage : Page
    {

        public string ConnectionStatus
        {
            get { return (string)GetValue(ConnectionStatusProperty); }
            set { SetValue(ConnectionStatusProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ConnectionStatus.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ConnectionStatusProperty =
            DependencyProperty.Register("ConnectionStatus", typeof(string), typeof(MainPage), new PropertyMetadata("Unknown"));



        public string PlaybackStatus
        {
            get { return (string)GetValue(PlaybackStatusProperty); }
            set { SetValue(PlaybackStatusProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PlaybackStatus.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PlaybackStatusProperty =
            DependencyProperty.Register("PlaybackStatus", typeof(string), typeof(MainPage), new PropertyMetadata("Unknown"));




        private LoopyAppConnection _serviceConnection;

        private LoopyAppConnection ServiceConnection
        {
            get { return _serviceConnection; }
            set
            {
                if (_serviceConnection != null)
                {
                    _serviceConnection.MessageReceived -= CommandReceived;
                    _serviceConnection.Dispose();
                }
                _serviceConnection = value;
                if (_serviceConnection != null)
                {
                    _serviceConnection.MessageReceived += CommandReceived; ;
                }
            }
        }

        private ValueSet CommandReceived(ValueSet set)
        {
            Debug.WriteLine($"Command received: {set.ToString()}");
            return set;
        }

        public MainPage()
        {
            this.InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            DataContext = this;
            if (ServiceConnection == null)
            {
                ServiceConnection = new LoopyAppConnection();
            }

            ConnectionStatus = "The initial Status";
        }



        private async void StartService_Click(object sender, RoutedEventArgs e)
        {
            
            // Add the connection.
            if (ServiceConnection == null || !ServiceConnection.IsValid())
            {
                //if (ServiceConnection == null)
                //{
                //    ServiceConnection = new LoopyAppConnection();
                //}
                ConnectionStatus = (await ServiceConnection.OpenConnectionAsync()).ToString();
            }
        }

        private async void SendPlaybackCommand(LoopyCommand.CommandType command, string param = "")
        {
            LoopyCommand lc = new LoopyCommand(command, param);
            if (ServiceConnection.IsValid())
            {
                try
                {
                    ValueSet response = await ServiceConnection.SendCommandAsync(lc);
                    if (response.Any())
                    {
                        StringBuilder responseMessageBuilder = new StringBuilder();
                        foreach(var pair in response)
                        {
                            responseMessageBuilder.Append($"Key: {pair.Key} Value: {pair.Value} | ");
                        } 
                        PlaybackStatus = response.ToString();
                    }
                    else
                    {
                        PlaybackStatus = "Sending Command failed";
                    }
                }
                catch (Exception ex)
                {
                    PlaybackStatus = ex.Message;
                }
            }
            else
            {
                PlaybackStatus = $"Service Connection is not valid";
            }
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
