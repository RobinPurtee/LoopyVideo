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

    internal class PlayerModel : BindableBase, IDisposable
    {
        public event EventHandler<PlayerModelErrorEventArgs> ErrorEvent;

        private const string settingsName = "MediaSource";

        private Uri _mediaUri;
        public Uri MediaUri
        {
            get
            {
                if (_mediaUri == null)
                {
                    Uri newValue;
                    string uriString = CurrentSetting;

                    if (string.IsNullOrEmpty(uriString)
                        || !Uri.TryCreate(uriString, UriKind.RelativeOrAbsolute, out newValue))
                    {
                        newValue = GetDefaultMediaUri();
                    }
                    _mediaUri = newValue;
                }
                _log.Information($"Media Uri: {_mediaUri.AbsolutePath}");
                return _mediaUri;
            }
            set
            {
                if (SetProperty(ref _mediaUri, value))
                { 
                    CurrentSetting = value.ToString();
                    UpdateMediaSource();
                }
            }
        }


        private string CurrentSetting
        {
            get { return (string)ApplicationData.Current.LocalSettings.Values[settingsName]; }
            set { ApplicationData.Current.LocalSettings.Values[settingsName] = value; }
        }


        /// <summary>
        /// Get the default media file object
        /// </summary>
        /// <returns></returns>
        private StorageFile GetDefaultMediaStorageFileAsync()
        {
            StorageLibrary lib = StorageLibrary.GetLibraryAsync(KnownLibraryId.Videos).GetAwaiter().GetResult();
            StorageFolder folder = lib.SaveFolder;
            // Get the files in the SaveFolder folder.
            IReadOnlyList<StorageFile> filesList = folder.GetFilesAsync().GetAwaiter().GetResult();
            if (filesList.Count == 0)
            {
                throw new FileNotFoundException("There are no files in the video library");
            }
            return filesList.First();
        }

        /// <summary>
        /// Get the default media uri
        /// </summary>
        /// <returns></returns>
        private Uri GetDefaultMediaUri()
        {
            return new Uri(GetDefaultMediaStorageFileAsync().Path);
        }


        /// <summary>
        /// The Player object that to control with this model
        /// </summary>
        private MediaPlayer _player = null;
        public MediaPlayer Player
        {
            get { return _player; }
            set
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
                    UpdateMediaSource();
                }
            }
        }

       
        /// <summary>
        /// Current state of the playback
        /// </summary>
        public MediaPlaybackState State
        {
            get
            {
                MediaPlaybackState ret = MediaPlaybackState.None;
                if (IsValid)
                {
                    ret = Player.PlaybackSession.PlaybackState;
                }
                return ret;
            }
        }

        /// <summary>
        /// default contructor
        /// </summary>
        public PlayerModel() : this(null, null) { }

        /// <summary>
        /// Initializing constructor
        /// </summary>
        /// <param name="player">The MediaPlayer to control with this object</param>
        public PlayerModel(MediaPlayer player) : this(player, null) { }

        /// <summary>
        /// Initializing constructor
        /// </summary>
        /// <param name="player">The MediaPlayer to control with this object</param>
        /// <param name="mediaLocation">The location of the media to play</param>
        public PlayerModel(MediaPlayer player, Uri mediaLocation)
        {
            Player = player;
            if (mediaLocation != null)
            {
                MediaUri = mediaLocation;
            }
        }

        /// <summary>
        /// Test is the player has been set yet
        /// </summary>
        public bool IsValid { get { return Player != null; } }


        /// <summary>
        /// Pauses media playback
        /// </summary>
        /// <returns>True if the video was paused</returns>
        public bool Pause()
        {
            bool bRet = Player.PlaybackSession.CanPause;
            if (bRet)
            {
                Player.Pause();
            }
            return bRet;
        }

        /// <summary>
        /// Play the media
        /// </summary>
        /// <returns>True if the video playback was started</returns>
        public bool Play()
        {
            bool bRet = State != MediaPlaybackState.Playing;
            if (bRet)
            {
                Player.Play();
            }
            return bRet;
        }

        /// <summary>
        /// Create and set a new MediaSource
        /// </summary>
        private void UpdateMediaSource()
        {
            MediaSource source = null;

            if (MediaUri != null)
            {
                if (MediaUri.IsFile)
                {
                    
                    StorageFile mediaFile = StorageFile.GetFileFromPathAsync(MediaUri.LocalPath).GetAwaiter().GetResult();
                    source = MediaSource.CreateFromStorageFile(mediaFile);
                }
                else
                {
                    source = MediaSource.CreateFromUri(MediaUri);

                }
            }

            if (Player.Source != null)
            {
                Pause();
                DisposeSource();
            }
            Player.Source = source;
        }


        #region MediaPlayback event handlers
        /// <summary>
        /// Handler for position changed 
        /// </summary>
        /// <remarks>limits logging to once a second</remarks>
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

        #endregion

        private Logger _log = new Logger("PlayerModel");

        #region IDisposable Support

        private void DisposeSource()
        {
            if (IsValid && Player.Source != null)
            {
                ((MediaSource)Player.Source).Dispose();
                Player.Source = null;
            }

        }

        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    DisposeSource();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~PlayerModel() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion

    }

    /// <summary>
    /// Error arguments for the PlayerModel
    /// </summary>
    public class PlayerModelErrorEventArgs : EventArgs
    {
        public string ErrorName { get; set; }

        public PlayerModelErrorEventArgs() { ErrorName = string.Empty; }
        public PlayerModelErrorEventArgs(string errorName) { ErrorName = errorName; }
    }
}
