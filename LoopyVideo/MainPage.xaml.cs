using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Media.Playback;
using Windows.Media.Core;
using LoopyVideo.Logging;


namespace LoopyVideo
{
    public sealed partial class MainPage : Page
    {
        private static Logger _log = new Logger("LoopyVideoMain");

        public Uri MediaUri
        {
            get { return (Uri)GetValue(MediaUriProperty); }
            set { SetValue(MediaUriProperty, value); }
        }

        private static readonly DependencyProperty MediaUriProperty =
            DependencyProperty.Register(
                "MediaUri",
                typeof(string),
                typeof(LoopyVideo.MainPage),
                new PropertyMetadata(null)
                );

        private PlayerModel _playerModel;

    
        /// <summary>
        /// Initialize the MediaPlayer of the Player control
        /// </summary>
        private void InitMediaPlayer()
        {
            _playerModel = new PlayerModel(_playerElement.MediaPlayer, MediaUri);
            _playerModel.ErrorEvent += PlayerModel_ErrorEvent;
        }

        /// <summary>
        /// Handler for errors reported by the PlayerModel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PlayerModel_ErrorEvent(object sender, PlayerModelErrorEventArgs e)
        {
            try
            {
                var ignored = this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Low, async () => {
                    string errorMessage;
                    var loader = new Windows.ApplicationModel.Resources.ResourceLoader();
                    errorMessage = loader.GetString(e.ErrorName);

                    _log.Information($"The media source has failed : {errorMessage}");
                    MessageDialog dialog = new MessageDialog(errorMessage);
                    await dialog.ShowAsync();

                });
            }
            catch (Exception ex)
            {
                _log.Information($"Error getting error message: {ex.Message}");
            }

        }

        /// <summary>
        /// Constructor
        /// </summary>
        public MainPage()
        {
            this.InitializeComponent();
            this.DataContext = this;
            MediaUri = MediaSourceUri.Instance.GetDefaultMediaUri();
        }

        /// <summary>
        /// Handler for the page being loaded
        /// </summary>
        /// <param name="sender">the frame that loaded it</param>
        /// <param name="e">N/A</param>
        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {

                _playerElement.IsFullWindow = false;
                InitMediaPlayer();
                _playerModel.Play();
                //_playerElement.MediaPlayer.Play();

            }
            catch(Exception ex)
            {
                MessageDialog dialog = new MessageDialog(ex.Message);
                await dialog.ShowAsync();
            }

        }

        
        /// <summary>
        ///  Page Unload handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            _playerModel?.Dispose();  
        }

        /// <summary>
        /// change the media source
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SetUriButton_Click(object sender, RoutedEventArgs e)
        {
            _playerModel.MediaUri = MediaUri;
            _playerModel?.Play();
        }

        /// <summary>
        /// File picker
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void filePick_Click(object sender, RoutedEventArgs e)
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation =  Windows.Storage.Pickers.PickerLocationId.VideosLibrary;
            picker.FileTypeFilter.Add(".mp4");
            picker.FileTypeFilter.Add(".mkv");
            picker.FileTypeFilter.Add(".wmv");
            picker.FileTypeFilter.Add(".avi");

            Windows.Storage.StorageFile file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                // Application now has read/write access to the picked file
                MediaUri = new Uri(file.Path);
            }
 
        }
    }
}
