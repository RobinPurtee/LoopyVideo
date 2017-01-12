using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace WebAppServiceClient.Controls
{
    public sealed partial class IndicatorControl : UserControl
    {


        public string Label
        {
            get { return (string)GetValue(LabelProperty); }
            set { SetValue(LabelProperty, value); }
        }
        public static readonly DependencyProperty LabelProperty =
            DependencyProperty.Register(nameof(Label), typeof(string), typeof(IndicatorControl), new PropertyMetadata("Label"));



        public SolidColorBrush OnColor
        {
            get { return (SolidColorBrush)GetValue(OnColorProperty); }
            set { SetValue(OnColorProperty, value); }
        }
        public static readonly DependencyProperty OnColorProperty =
            DependencyProperty.Register(nameof(OnColor), typeof(SolidColorBrush), typeof(IndicatorControl), new PropertyMetadata(0));



        public SolidColorBrush OffColor
        {
            get { return (SolidColorBrush)GetValue(OffColorProperty); }
            set { SetValue(OffColorProperty, value); }
        }
        public static readonly DependencyProperty OffColorProperty =
            DependencyProperty.Register(nameof(OffColor), typeof(SolidColorBrush), typeof(IndicatorControl), new PropertyMetadata(0));



        public bool IsLit
        {
            get { return (bool)GetValue(IsLitProperty); }
            set { SetValue(IsLitProperty, value); }
        }
        public static readonly DependencyProperty IsLitProperty =
            DependencyProperty.Register(nameof(IsLit), typeof(bool), typeof(IndicatorControl), new PropertyMetadata(false));



        public IndicatorControl()
        {
            this.InitializeComponent();
            OnColor = Resources["LightOnColor"] as SolidColorBrush;
            OffColor = Resources["LightOffColor"] as SolidColorBrush;
        }
    }
}
