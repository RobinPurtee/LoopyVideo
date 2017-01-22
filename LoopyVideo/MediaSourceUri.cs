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
    class MediaSourceUri
    {
        private Logger _log = new Logger("MediaSourceUri");

        public const string MediaUriName = "MediaUri";
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
                if (_mediaUri != null && !value.Equals(_mediaUri))
                {
                    _mediaUri = value;
                    CurrentSetting = value.ToString();
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
        public Uri GetDefaultMediaUri()
        {
            return new Uri(GetDefaultMediaStorageFileAsync().Path);
        }


    }
}
