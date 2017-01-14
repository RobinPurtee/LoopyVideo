using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

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
            DependencyProperty.Register(nameof(Label), typeof(string), typeof(IndicatorControl), new PropertyMetadata("Fred"));



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


        public enum State { Off, On }

        private State _indicator = State.Off;
        public State Indicator
        {
            get { return _indicator; }
            set
            {
                _indicator = value;
                if (value == State.On)
                {
                    VisualStateManager.GoToState(this, "LightOn", false);
                }
                else
                {
                    VisualStateManager.GoToState(this, "LightOff", false);
                }

            }
        }


        public IndicatorControl()
        {
            this.InitializeComponent();
            OnColor = Resources["LightOnColor"] as SolidColorBrush;
            OffColor = Resources["LightOffColor"] as SolidColorBrush;
        }
    }
}
