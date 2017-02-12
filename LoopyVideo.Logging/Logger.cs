//  ---------------------------------------------------------------------------------
//  Copyright (c)  Rick Purtee.  All rights reserved.
// 
//  The MIT License (MIT)
// 
//  Permission is hereby granted, free of charge, to any person obtaining a copy
//  of this software and associated documentation files (the "Software"), to deal
//  in the Software without restriction, including without limitation the rights
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//  copies of the Software, and to permit persons to whom the Software is
//  furnished to do so, subject to the following conditions:
// 
//  The above copyright notice and this permission notice shall be included in
//  all copies or substantial portions of the Software.
// 
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//  THE SOFTWARE.
//  ---------------------------------------------------------------------------------
using System;
using System.Text;
using System.Diagnostics;
using Windows.Foundation.Diagnostics;
using Windows.Foundation.Collections;

namespace LoopyVideo.Logging
{
    public class Logger : IDisposable
    {
        private LoggingChannel _logChannel;
        private string _providerName;
        /// <summary>
        /// Method that actually writes the message
        /// </summary>
        /// <param name="message">The message to log</param>
        /// <param name="level">The diagnostic level of the message</param>
        /// <remarks>This method logs to bot the Debug console and the ETW for the system</remarks>
        private void LogMessage(string message, LoggingLevel level)
        {
            _logChannel.LogMessage(message, LoggingLevel.Information);
            Debug.WriteLine($"[{level.ToString()}] ({_providerName}) {message}");
            
        }

        /// <summary>
        /// Logger initializing constructor
        /// </summary>
        /// <param name="providerName">The name of the message provider</param>
        public Logger(string providerName)
        {
            // setup a log channel to the Microsoft-Windows-Diagnostic-LoggingChannel channel
            _logChannel = new LoggingChannel(providerName, null, new Guid("4bd2826e-54a1-4ba9-bf63-92b73ea1ac4a"));
            _providerName = providerName;
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

        /// <summary>
        /// Log an informational message
        /// </summary>
        /// <param name="message">The message to log</param>
        public void Information(string message)
        {
            LogMessage(message, LoggingLevel.Information);
        }

        /// <summary>
        /// Log an error message
        /// </summary>
        /// <param name="message">The message to log</param>
        public void Error(string message)
        {
            LogMessage(message, LoggingLevel.Error);
        }
    }

    /// <summary>
    /// Helper class to output a ValueSet has a JSON string
    /// </summary>
    public static class ValueSetOut
    {
        /// <summary>
        /// Create a JSON string from a ValueSet
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static string ToString(ValueSet values)
        {
            if(null == values)
            {
                return string.Empty;
            }
            StringBuilder valueBuilder = new StringBuilder("{ ");
            foreach (var pair in values)
            {
                if (valueBuilder.Length > 2)
                {
                    valueBuilder.Append(", ");
                }
                valueBuilder.Append($"{pair.Key} : {pair.Value} ");
            }
            valueBuilder.Append("}");

            return valueBuilder.ToString();
        }
    }


}
