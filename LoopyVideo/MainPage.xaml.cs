using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Composition;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Media.Playback;
using Windows.Media.Core;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace LoopyVideo
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        MediaPlayer _mediaPlayer;

        public MainPage()
        {
            this.InitializeComponent();
            _mediaPlayer = new MediaPlayer();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                _mediaPlayer.Source = MediaSource.CreateFromUri(new Uri("ms-appx:///Assets/COUNTDOWN_512kb.mp4"));
                _mediaPlayer.AudioCategory = MediaPlayerAudioCategory.Media;
                _mediaPlayer.IsLoopingEnabled = true;
                _mediaPlayer.IsMuted = true;


                _mediaPlayer.SetSurfaceSize(new Size(_playerSurface.ActualWidth, _playerSurface.ActualHeight));

                var compositor = Windows.UI.Xaml.Hosting.ElementCompositionPreview.GetElementVisual(this).Compositor;
                MediaPlayerSurface surface = _mediaPlayer.GetSurface(compositor);

                SpriteVisual spriteVisual = compositor.CreateSpriteVisual();
                spriteVisual.Size =
                    new System.Numerics.Vector2((float)_playerSurface.ActualWidth, (float)_playerSurface.ActualHeight);

                CompositionBrush brush = compositor.CreateSurfaceBrush(surface.CompositionSurface);
                spriteVisual.Brush = brush;

                ContainerVisual container = compositor.CreateContainerVisual();
                container.Children.InsertAtTop(spriteVisual);

                ElementCompositionPreview.SetElementChildVisual(_playerSurface, container);

                _mediaPlayer.Play();

            }
            catch(Exception ex)
            {
                MessageDialog errorDialog = new MessageDialog(ex.Message, "Error Creating MediaPlayer");
                await errorDialog.ShowAsync();
            }


        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            _mediaPlayer.Dispose();
        }

    }
}
