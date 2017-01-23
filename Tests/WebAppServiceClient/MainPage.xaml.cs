//  ---------------------------------------------------------------------------------
//  Copyright (c)  Rick Purtee.  All rights reserved.
// 
//  The MIT License (MIT)
// 
//  Permission is hereby granted, free of charge, to any person obtaining a copy
//  of this software and associated documentation files (the "Software"), to deal
//  in the Software without restriction, including without limitation the rights
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//  copies of the Software, and to permit persons to whom the Software is
//  furnished to do so, subject to the following conditions:
// 
//  The above copyright notice and this permission notice shall be included in
//  all copies or substantial portions of the Software.
// 
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//  THE SOFTWARE.
//  ---------------------------------------------------------------------------------
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using WebAppServiceClient.Controls;
using LoopyVideo.Commands;
using LoopyVideo.Logging;
using System;
// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace WebAppServiceClient
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private Logger _log = new Logger("WebAppServiceClient");

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
            LoopyCommand retCommand = new LoopyCommand(LoopyCommand.CommandType.Error, $"Unsupported command type {command.Command.ToString()}");
            Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                switch (command.Command)
                {
                    case LoopyCommand.CommandType.Play:
                        TogglePlayState();
                        retCommand.Copy(command);
                        break;
                    case LoopyCommand.CommandType.Stop:
                        ToggleStopState();
                        retCommand.Copy(command);
                        break;
                    default:
                        break;

                }
            }).AsTask().Wait();
            return retCommand;

        }

        public MainPage()
        {
            this.InitializeComponent();
        }


        private void TogglePlayState()
        {

            PlayIndicator.Indicator = (PlayIndicator.Indicator == IndicatorControl.State.Off) 
                ? IndicatorControl.State.On
                : IndicatorControl.State.Off;
        }
        
        private void ToggleStopState()
        {
            StopIndicator.Indicator = (StopIndicator.Indicator == IndicatorControl.State.Off)
                ? IndicatorControl.State.On
                : IndicatorControl.State.Off;

        }




        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            SendPlaybackCommand(LoopyCommand.CommandType.Play);
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleStopState();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            _log.Information("MainPage Loaded");
            DataContext = this;
            if (ServiceConnection == null)
            {
                ServiceConnection = new AppConnection("WebAppServiceClient.ServiceConnection");
            }
            await ServiceConnection.OpenConnectionAsync();
        }




        private async void SendPlaybackCommand(LoopyCommand.CommandType command, string param = "")
        {

            _log.Information("Opening the connection to the service");

            if (!ServiceConnection.IsValid())
            {
                _log.Error("Failed to connect to the service");
                errorText.Text = "Service connection is not valid";
                return;
            }

            LoopyCommand lc = new LoopyCommand(command, param);
            try
            {

                _log.Information($"Sending Command {lc.ToString()}");
                LoopyCommand response = await ServiceConnection.SendCommandAsync(lc);
                statusText.Text = response.ToString();
            }
            catch (Exception ex)
            {
                errorText.Text = $"Exception while Sending Play command: {ex.Message}";
            }
            _log.Information($"SendCommand exit with PlaybackStatus: {statusText.Text}");
        }

    }
}
