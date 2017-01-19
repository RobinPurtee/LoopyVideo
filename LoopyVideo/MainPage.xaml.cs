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
                PropertyMetadata.Create(
                    () =>
                    {
                        Uri ret = null;
                        string uriStr = (string)ApplicationData.Current.LocalSettings.Values["mediaSource"];
                        if (string.IsNullOrEmpty(uriStr))
                        {
                            ret = Task.Run(MainPage.GetDefaultMediaUriAsync).Result;
                        }
                        else
                        {
                            ret = new Uri(uriStr);
                        }
                        return ret;
                    },
                    (o, e) =>
                    {
                        Uri oldValue = e.OldValue as Uri;
                        Uri newValue = e.NewValue as Uri;
                        if (newValue != oldValue)
                        {
                            ApplicationData.Current.LocalSettings.Values["mediaSource"] = newValue.ToString();
                        }
                    }
                )
            );

        private PlayerModel _playerModel;

        private MediaSource _media;
        public MediaSource Source
        {
            get { return _media; }
            private set { _media = value; }
        }

        /// <summary>
        /// Get the base folder (default to the Video library)
        /// </summary>
        /// <returns>The base folder to use </returns>
        private static async Task<StorageFolder> GetBaseFolderAsync()
        {
            StorageLibrary lib = await StorageLibrary.GetLibraryAsync(KnownLibraryId.Videos);
            StorageFolder folder = lib.SaveFolder;
            Debug.WriteLine(string.Format("The video library path is: {0}", folder.Path));
            return folder;
        }

        /// <summary>
        /// Get the default media file object
        /// </summary>
        /// <returns></returns>
        private static async Task<StorageFile> GetDefaultMediaStorageFileAsync()
        {
            StorageFolder folder = await GetBaseFolderAsync();
            // Get the files in the SaveFolder folder.
            IReadOnlyList<StorageFile> filesList = await folder.GetFilesAsync();
            if (filesList.Count == 0)
            {
                throw new FileNotFoundException("There are no files in the video library");
            }
            Debug.WriteLine(string.Format("The default video file is: {0}", filesList.First().Path));
            return filesList.First();
        }

        /// <summary>
        /// Get the default media uri
        /// </summary>
        /// <returns></returns>
        private static async Task<Uri> GetDefaultMediaUriAsync()
        {           
            return new Uri((await GetDefaultMediaStorageFileAsync()).Path);
        }

        /// <summary>
        /// Get the MediaSource 
        /// </summary>
        private async Task<MediaSource> GetMediaSource()
        {
            MediaSource ret = null;
            ApplicationDataContainer settings = ApplicationData.Current.LocalSettings;


            if (MediaUri.IsFile)
            {
                StorageFile mediaFile = await StorageFile.GetFileFromPathAsync(MediaUri.LocalPath);
                ret = MediaSource.CreateFromStorageFile(mediaFile);
            }
            else
            {
                ret = MediaSource.CreateFromUri(MediaUri);

            }
            return ret;
        }

        /// <summary>
        /// Initialize the MediaPlayer of the Player control
        /// </summary>
        private void InitMediaPlayer()
        {
            _playerModel = new PlayerModel(_playerElement.MediaPlayer, MediaUri);
            _playerModel.ErrorEvent += PlayerModel_ErrorEvent;
        }

        private void PlayerModel_ErrorEvent(object sender, PlayerModelErrorEventArgs e)
        {
            try
            {
                var ignored = this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Low, async () => {
                    string errorMessage;
                    //errorMessage = (string)Application.Current.Resources[errorName];
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
            ((MediaSource)_playerElement.MediaPlayer.Source).Dispose();  
        }

        /// <summary>
        /// change the media source
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void SetUriButton_Click(object sender, RoutedEventArgs e)
        {

            MediaPlayer player = _playerElement.MediaPlayer;
            if(player.PlaybackSession.CanPause)
            {
                player.Pause();
            }
            player.Source = await GetMediaSource();
            player.Play();
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
