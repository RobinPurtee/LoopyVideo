using System;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Media.Playback;
using Windows.Media.Core;
using Windows.Storage;
using LoopyVideo.Logging;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LoopyVideo
{
    public class PlayerModelErrorEventArgs : EventArgs
    {
        public string ErrorName { get; set; }

        public PlayerModelErrorEventArgs() { ErrorName = string.Empty; }
        public PlayerModelErrorEventArgs(string errorName) { ErrorName = errorName; }
    }

    internal class PlayerModel
    {
        private Logger _log = new Logger("PlayerModel");

        public event EventHandler<PlayerModelErrorEventArgs> ErrorEvent;


        private MediaPlayer _player;
        public MediaPlayer Player
        {
            get { return _player; }
            private set
            {
                _player = value;
                if (_player != null)
                {
                    _player.RealTimePlayback = true;
                    _player.PlaybackSession.PlaybackRate = 1.0;
                    _player.AutoPlay = true;
                    //hook up event handlers
                    _player.MediaEnded += Player_MediaEnded;
                    _player.MediaFailed += Player_MediaFailed;
                    _player.MediaOpened += Player_MediaOpened;
                    _player.SourceChanged += Player_SourceChanged;
                    _player.PlaybackSession.BufferingEnded += Player_BufferingEnded;
                    _player.PlaybackSession.BufferingProgressChanged += Player_BufferingProgressChanged;
                    _player.PlaybackSession.BufferingStarted += Player_BufferingStarted;
                    _player.PlaybackSession.DownloadProgressChanged += Player_DownloadProgressChanged;
                    _player.PlaybackSession.NaturalDurationChanged += Player_NaturalDurationChanged;
                    _player.PlaybackSession.NaturalVideoSizeChanged += Player_NaturalVideoSizeChanged;
                    _player.PlaybackSession.PlaybackRateChanged += Player_PlaybackRateChanged;
                    _player.PlaybackSession.PlaybackStateChanged += Player_PlaybackStateChanged;
                    _player.PlaybackSession.PositionChanged += Player_PositionChanged;
                    _player.PlaybackSession.SeekCompleted += Player_SeekCompleted;

                }
            }
        }

        private Uri _mediaUri;
        public Uri MediaUri
        {
            get
            {
                string uriStr = (string)ApplicationData.Current.LocalSettings.Values["mediaSource"];
                if( string.IsNullOrEmpty(uriStr) 
                    || !Uri.IsWellFormedUriString(uriStr, UriKind.RelativeOrAbsolute)    
                    )
                {
                    uriStr = GetDefaultMediaUriString();
                }
                if (_mediaUri.ToString() != uriStr)
                {
                    _mediaUri = new Uri(uriStr);
                }

                return _mediaUri;
            }
            set
            {
                if(_mediaUri != null && value != _mediaUri)
                {
                    ApplicationData.Current.LocalSettings.Values["mediaSource"] = value.ToString();
                }
                _mediaUri = value;
                if(_mediaUri != null && Player != null)
                {
                    Player.Source =  Task.Run(GetMediaSource).Result;
                }
            }
        }



        public PlayerModel(MediaPlayer player, Uri mediaUri)
        {
            Player = player;
            MediaUri = mediaUri;
        }

        /// <summary>
        /// Get the base folder (default to the Video library)
        /// </summary>
        /// <returns>The base folder to use </returns>
        private async Task<StorageFolder> GetBaseFolderAsync()
        {
            StorageLibrary lib = await StorageLibrary.GetLibraryAsync(KnownLibraryId.Videos);
            StorageFolder folder = lib.SaveFolder;
            _log.Information(string.Format("The video library path is: {0}", folder.Path));
            return folder;
        }

        /// <summary>
        /// Get the default media file object
        /// </summary>
        /// <returns></returns>
        private async Task<StorageFile> GetDefaultMediaStorageFileAsync()
        {
            StorageFolder folder = await GetBaseFolderAsync();
            // Get the files in the SaveFolder folder.
            IReadOnlyList<StorageFile> filesList = await folder.GetFilesAsync();
            if (filesList.Count == 0)
            {
                throw new FileNotFoundException("There are no files in the video library");
            }
            _log.Information(string.Format("The default video file is: {0}", filesList.First().Path));
            return filesList.First();
        }

        /// <summary>
        /// Get the default media uri
        /// </summary>
        /// <returns></returns>
        private string GetDefaultMediaUriString()
        {
            return (GetDefaultMediaStorageFileAsync().Result).Path;
        }

        /// <summary>
        /// Get the MediaSource 
        /// </summary>
        private async Task<MediaSource> GetMediaSource()
        {
            MediaSource ret = null;


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



        private TimeSpan updateTime = new TimeSpan();
        private void Player_PositionChanged(MediaPlaybackSession sender, object args)
        {
            if (sender.Position > updateTime)
            {
                _log.Information($"PlayBack Session Position Changed to: {sender.Position.ToString()}");
                updateTime += TimeSpan.FromSeconds(1.0);
            }
        }
        /// <summary>
        /// Playback State Changed
        /// </summary>
        private void Player_PlaybackStateChanged(MediaPlaybackSession sender, object args)
        {
            _log.Information($"PlayBack Session State Changed to: {sender.PlaybackState.ToString()}");
        }

        /// <summary>
        /// Natural VideoSize Changed handle
        /// </summary>
        /// <param name="sender">the MediaPlaybackSession that changed </param>
        /// <param name="args"></param>
        private void Player_NaturalVideoSizeChanged(MediaPlaybackSession sender, object args)
        {
            Size size = new Windows.Foundation.Size(sender.NaturalVideoWidth, sender.NaturalVideoHeight);
            _log.Information($"Natural Video Size Changed to: {size.ToString()}");
        }

        /// <summary>
        /// Download progress handler
        /// </summary>
        /// <param name="sender">the MediaPlaybackSession that changed </param>
        /// <param name="args"></param>
        private void Player_DownloadProgressChanged(MediaPlaybackSession sender, object args)
        {
            _log.Information($"Current DownloadProgress: {sender.DownloadProgress.ToString()}");
        }

        /// <summary>
        /// Buffering Progress handler
        /// </summary>
        /// <param name="sender">the MediaPlaybackSession that changed </param>
        /// <param name="args"></param>
        private void Player_BufferingProgressChanged(MediaPlaybackSession sender, object args)
        {
            _log.Information($"Buffer Progress is currently: {sender.BufferingProgress.ToString()}");

        }

        private void Player_SourceChanged(MediaPlayer sender, object args)
        {
            _log.Information("Player Source changed");
        }

        private void Player_PlaybackRateChanged(MediaPlaybackSession sender, object args)
        {
            _log.Information($"Playback Rate is currently : {sender.PlaybackRate.ToString()}");
        }

        private void Player_NaturalDurationChanged(MediaPlaybackSession sender, object args)
        {
            _log.Information($"Natural Video Duration is : {sender.NaturalDuration.ToString()}");
        }

        private void Player_MediaOpened(MediaPlayer sender, object args)
        {
            _log.Information("The Player opened :" + sender.Source.ToString());
        }

        /// <summary>
        /// Media playback error handler
        /// </summary>
        /// <param name="sender">the MediaPlayer that sent the error;</param>
        /// <param name="args"></param>
        private void Player_MediaFailed(MediaPlayer sender, MediaPlayerFailedEventArgs args)
        {
            PlayerModelErrorEventArgs errorArgs = new PlayerModelErrorEventArgs($"MediaPlayerError_{args.Error.ToString()}");
            if(ErrorEvent != null)
            {
                ErrorEvent(this, errorArgs);
            }
        }

        private void Player_BufferingStarted(MediaPlaybackSession sender, object args)
        {
            _log.Information("Buffer has started");
        }

        private void Player_BufferingEnded(MediaPlaybackSession sender, object args)
        {
            _log.Information("Buffer has started");
        }

        /// <summary>
        /// Seek has completed, restart playback
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void Player_SeekCompleted(MediaPlaybackSession sender, object args)
        {
            sender.MediaPlayer.Play();
        }

        private void Player_MediaEnded(MediaPlayer sender, object args)
        {
            sender.PlaybackSession.Position = TimeSpan.Zero;
            updateTime = TimeSpan.Zero;
        }


        public bool Pause()
        {
            bool bRet = Player.PlaybackSession.CanPause;
            if(bRet)
            {
                Player.Pause();
            }
            return bRet;
        }

        public bool Play()
        {
            bool bRet = Player.PlaybackSession.PlaybackState == MediaPlaybackState.Paused;
            if(bRet)
            {
                Player.Play();
            }
            return bRet;
        }

    }
}
