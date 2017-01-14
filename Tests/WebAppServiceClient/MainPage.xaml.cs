using System;
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
using WebAppServiceClient.Controls;
// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace WebAppServiceClient
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            if(PlayIndicator.Indicator == IndicatorControl.State.Off)
            {
                PlayIndicator.Indicator = IndicatorControl.State.On;
            }
            else
            {
                PlayIndicator.Indicator = IndicatorControl.State.Off;
            }

        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            if (StopIndicator.Indicator == IndicatorControl.State.Off)
            {
                StopIndicator.Indicator = IndicatorControl.State.On;
            }
            else
            {
                StopIndicator.Indicator = IndicatorControl.State.Off;
            }
        }
    }
}
