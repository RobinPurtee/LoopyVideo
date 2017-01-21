using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ComponentModel;
using System.Threading.Tasks;
using Windows.Storage;
using LoopyVideo.Logging;

namespace LoopyVideo
{
    class MediaSourceUri : INotifyPropertyChanged
    {
        private Logger _log = new Logger("MediaSourceUri");

        public const string MediaUriName = "MediaUri";
        private const string settingsName = "MediaSource";

        #region Singleton implementation
        private Uri _mediaUri;
        private static readonly Lazy<MediaSourceUri> _instance = new Lazy<MediaSourceUri>(() => new MediaSourceUri());

        /// <summary>
        /// The property factory
        /// </summary>
        public static MediaSourceUri Instance
        {
            get { return _instance.Value; }
        }

        /// <summary>
        /// Private constructor to force singleton use
        /// </summary>
        private MediaSourceUri() { }
        #endregion

        public event PropertyChangedEventHandler PropertyChanged;


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
            StorageLibrary lib = StorageLibrary.GetLibraryAsync(KnownLibraryId.Videos).AsTask<StorageLibrary>().Result;
            StorageFolder folder = lib.SaveFolder;
            // Get the files in the SaveFolder folder.
            IReadOnlyList<StorageFile> filesList = folder.GetFilesAsync().AsTask<IReadOnlyList<StorageFile>>().Result;;
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
        public Uri GetDefaultMediaUri()
        {
            return new Uri(GetDefaultMediaStorageFileAsync().Path);
        }

        /// <summary>
        /// Get the current MediaUri for the application
        /// </summary>
        public Uri Get()
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

        /// <summary>
        /// Set the medai Uri to the give value
        /// </summary>
        /// <param name="value">the value to set the Property too</param>
        /// <returns>True if the given value is a new one</returns>
        public bool Set(Uri value)
        {
            bool hasChanged = _mediaUri != null && !value.Equals(_mediaUri);
            if (hasChanged)
            {
                _mediaUri = value;
                CurrentSetting = value.ToString();
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(MediaUriName));
            }
            return hasChanged;
        }



    }
}
