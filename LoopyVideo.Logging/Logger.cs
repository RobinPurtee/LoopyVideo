using System;
using System.Text;
using Windows.Foundation.Diagnostics;
using Windows.Foundation.Collections;

namespace LoopyVideo.Logging
{
    public class Logger : IDisposable
    {
        private LoggingChannel _logChannel;
        

        public Logger(string providerName)
        {
            _logChannel = new LoggingChannel(providerName, null, new Guid("4bd2826e-54a1-4ba9-bf63-92b73ea1ac4a"));
        }

        #region IDisposable Members

        /// <summary>
        /// Internal variable which checks if Dispose has already been called
        /// </summary>
        private Boolean disposed;

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        private void Dispose(Boolean disposing)
        {
            if (disposed)
            {
                return;
            }

            if (disposing)
            {
                if(_logChannel != null)
                {
                    _logChannel.Dispose();
                    _logChannel = null;
                }

            }
 
            disposed = true;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            // Call the private Dispose(bool) helper and indicate 
            // that we are explicitly disposing
            this.Dispose(true);

            // Tell the garbage collector that the object doesn't require any
            // cleanup when collected since Dispose was called explicitly.
            GC.SuppressFinalize(this);
        }

        #endregion


        public void Information(string message)
        {
            _logChannel.LogMessage(message, LoggingLevel.Information);
        }

        public void Error(string message)
        {
            _logChannel.LogMessage(message, LoggingLevel.Error);
        }
    }

    public static class ValueSetOut
    {
        public static string ToString(ValueSet values)
        {
            StringBuilder valueBuilder = new StringBuilder();
            foreach (var pair in values)
            {
                if (valueBuilder.Length > 0)
                {
                    valueBuilder.Append(" | ");
                }
                valueBuilder.Append($"Key: {pair.Key} Value: {pair.Value}");
            }

            return valueBuilder.ToString();
        }
    }


}
