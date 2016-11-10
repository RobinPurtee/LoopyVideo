using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Composition;
using Windows.Storage;
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


        public string MediaUri
        {
            get { return (string)GetValue(MediaUriProperty); }
            set { SetValue(MediaUriProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MediaUri.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MediaUriProperty =
            DependencyProperty.Register("MediaUri", typeof(string), typeof(LoopyVideo.MainPage), new PropertyMetadata(0));



        private MediaSource _media;
        public MediaSource Source
        {
            get { return _media; }
            private set { _media = value; }
        }


        private async Task<StorageFolder> GetBaseFolderAsync()
        {
            StorageLibrary lib = await StorageLibrary.GetLibraryAsync(KnownLibraryId.Videos);
            StorageFolder folder = lib.SaveFolder;
            Debug.WriteLine(string.Format("The video library path is: {0}", folder.Path));
            return folder;
        }

        private async Task<StorageFile> GetDefaultMediaStorageFileAsync()
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

        private async Task<Uri> GetDefaultMediaUriAsync()
        {           
            return new Uri((await GetDefaultMediaStorageFileAsync()).Path);
        }

        private async Task<MediaSource> GetMediaSource()
        {
            MediaSource ret = null;
            ApplicationDataContainer settings = ApplicationData.Current.LocalSettings;

            string uriString = (string)settings.Values["mediaSource"];
            if(null == uriString)
            {
                //uriString = (await GetDefaultMediaUriAsync()).ToString();
                //settings.Values["mediaSource"] = uriString.ToString();
            }
            MediaUri = uriString;
            Uri mediaUri = new Uri(uriString);


            //ret = MediaSource.CreateFromUri(mediaUri);
            StorageFile mediaFile = await StorageFile.GetFileFromPathAsync(mediaUri.LocalPath);
            ret = MediaSource.CreateFromStorageFile(mediaFile);
            return ret;
        }

        private async Task<MediaPlayer> CreateMediaPlayer()
        {
            MediaPlayer player = _playerElement.MediaPlayer;
            player.Source = await GetMediaSource();
            player.RealTimePlayback = true;
            player.PlaybackSession.PlaybackRate = 1.0;
            player.AutoPlay = true;
            //hook up event handlers
            player.MediaEnded += Player_MediaEnded;
            player.MediaFailed += Player_MediaFailed;
            player.MediaOpened += Player_MediaOpened;
            player.SourceChanged += Player_SourceChanged;
            player.PlaybackSession.BufferingEnded += Player_BufferingEnded;
            player.PlaybackSession.BufferingProgressChanged += Player_BufferingProgressChanged;
            player.PlaybackSession.BufferingStarted += Player_BufferingStarted;
            player.PlaybackSession.DownloadProgressChanged += Player_DownloadProgressChanged;
            player.PlaybackSession.NaturalDurationChanged += Player_NaturalDurationChanged;
            player.PlaybackSession.NaturalVideoSizeChanged += Player_NaturalVideoSizeChanged;
            player.PlaybackSession.PlaybackRateChanged += Player_PlaybackRateChanged;
            player.PlaybackSession.PlaybackStateChanged += Player_PlaybackStateChanged;
            player.PlaybackSession.PositionChanged += Player_PositionChanged;
            player.PlaybackSession.SeekCompleted += Player_SeekCompleted;

            return player;
        }



        public MainPage()
        {
            this.InitializeComponent();
            this.DataContext = this;
            MediaUri = string.Empty;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {

                _playerElement.IsFullWindow = false;

                await CreateMediaPlayer();


            }
            catch(Exception ex)
            {
                MessageDialog dialog = new MessageDialog(ex.Message);
                dialog.ShowAsync();
            }

        }

        private TimeSpan updateTime = new TimeSpan();
        private void Player_PositionChanged(MediaPlaybackSession sender, object args)
        {
            if(sender.Position > updateTime)
            {
                Debug.WriteLine("PlayBack Session Position Changed to: " + sender.Position.ToString());
                updateTime += TimeSpan.FromSeconds(1.0);
            }
        }
        /// <summary>
        /// Playback State Changed
        /// </summary>
        private void Player_PlaybackStateChanged(MediaPlaybackSession sender, object args)
        {
            Debug.WriteLine("PlayBack Session State Changed to: " + sender.PlaybackState.ToString());
        }

        /// <summary>
        /// Natural VideoSize Changed
        /// </summary>
        private void Player_NaturalVideoSizeChanged(MediaPlaybackSession sender, object args)
        {
            Size size = new Windows.Foundation.Size(sender.NaturalVideoWidth, sender.NaturalVideoHeight);
            Debug.WriteLine("Natural Video Size Changed to: " + size.ToString());
        }

        private void Player_DownloadProgressChanged(MediaPlaybackSession sender, object args)
        {
            Debug.WriteLine("Current DownloadProgress: " + sender.DownloadProgress.ToString());
        }

        private void Player_BufferingProgressChanged(MediaPlaybackSession sender, object args)
        {
            Debug.WriteLine("Buffer Progress is currently: " + sender.BufferingProgress.ToString());

        }

        private void Player_SourceChanged(MediaPlayer sender, object args)
        {
            Debug.WriteLine("Player Source changed");
        }

        private void Player_PlaybackRateChanged(MediaPlaybackSession sender, object args)
        {
            Debug.WriteLine("Playback Rate is currently :" + sender.PlaybackRate.ToString());
        }

        private void Player_NaturalDurationChanged(MediaPlaybackSession sender, object args)
        {
            Debug.WriteLine("Natural Video Duration is :" + sender.NaturalDuration.ToString());
        }

        private void Player_MediaOpened(MediaPlayer sender, object args)
        {
            Debug.WriteLine("The Player opened :" + sender.Source.ToString());
        }

        private void Player_MediaFailed(MediaPlayer sender, MediaPlayerFailedEventArgs args)
        {
            string errorName = string.Format("MediaPlayerError_{0}", args.Error.ToString());
            try
            {
                var ignored = this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Low, () => {
                    string errorMessage; 
                    //errorMessage = (string)Application.Current.Resources[errorName];
                    var loader = new Windows.ApplicationModel.Resources.ResourceLoader();
                    errorMessage = loader.GetString(errorName);

                    Debug.WriteLine("The media source has failed : " + errorMessage);
                    MessageDialog dialog = new MessageDialog(errorMessage);
                    dialog.ShowAsync();

                });
            }
            catch(Exception ex)
            {
                Debug.WriteLine("Error getting error message: {0}", ex.Message);
            }
        }

        private void Player_BufferingStarted(MediaPlaybackSession sender, object args)
        {
            Debug.WriteLine("Buffer has started");
        }

        private void Player_BufferingEnded(MediaPlaybackSession sender, object args)
        {
            Debug.WriteLine("Buffer has started");
        }

        private void Player_SeekCompleted(MediaPlaybackSession sender, object args)
        {
            sender.MediaPlayer.Play();
        }

        private void Player_MediaEnded(MediaPlayer sender, object args)
        {
            sender.PlaybackSession.Position = TimeSpan.Zero;
            updateTime = TimeSpan.Zero;
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            _playerElement.MediaPlayer.Dispose();  
        }

        private async void SetUriButton_Click(object sender, RoutedEventArgs e)
        {
            ApplicationData.Current.LocalSettings.Values["mediaSource"] = MediaUri;

            MediaPlayer player = _playerElement.MediaPlayer;
            if(player.PlaybackSession.CanPause)
            {
                player.Pause();
            }
            player.Source = await GetMediaSource();
            player.Play();
        }

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
                MediaUri = new Uri(file.Path).ToString();
                ApplicationData.Current.LocalSettings.Values["mediaSource"] = MediaUri;
 

            }
 
        }
    }
}
