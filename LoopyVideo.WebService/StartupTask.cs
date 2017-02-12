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
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.AppService;
using Restup.Webserver.Http;
using Restup.Webserver.Rest;
using Restup.Webserver.File;
using LoopyVideo.Logging;
using LoopyVideo.Commands;


// The Background Application template is documented at http://go.microsoft.com/fwlink/?LinkID=533884&clcid=0x409

namespace LoopyVideo.WebService
{
    public sealed class StartupTask : IBackgroundTask
    {
        private Logger _log = new Logger("WebServiceProvider");

        private BackgroundTaskDeferral _deferral = null;
        private HttpServer _webServer = null;

        /// <summary>
        /// AppService entry point 
        /// </summary>
        /// <param name="taskInstance">The instance of the AppService</param>
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            _log.Information($"WebService.Run Starting");
            // save the deferral to keep the server running until the instance is Canceled
            _deferral = taskInstance.GetDeferral();
            taskInstance.Canceled += WebService_Canceled;

            Task webServerTask = null;

            // setup the web server
            var restRouteHandler = new RestRouteHandler();
            restRouteHandler.RegisterController<LoopyCommandController>();

            var configuration = new HttpServerConfiguration()
                .ListenOnPort(8800)
                .RegisterRoute("loopy", restRouteHandler)
                .RegisterRoute(new StaticFileRouteHandler(@"Web"))
                .EnableCors();
            try
            {
                _log.Information("Creating Web Server");
                _webServer = new HttpServer(configuration);
                webServerTask = _webServer.StartServerAsync();
                _log.Information("Web Server Started");
            }
            catch (Exception ex)
            {
                _log.Information($"Web Server Exception: {ex.Message}");
            }

            // setup the AppService Connection
            // Setting the AppConnectionFactory.Connection will cause any pending
            // request received events to fire. Therefore, setting the connection
            // is the last thing to be done.    
            try
            {
                var serviceTrigger = taskInstance.TriggerDetails as AppServiceTriggerDetails;
                AppConnectionFactory.Instance.MessageReceived += ReceiveAppCommand; ;
                AppConnectionFactory.Instance.Connection = serviceTrigger.AppServiceConnection;
            }
            catch (Exception ex)
            {
                _log.Error($"Error starting App Service connection: {ex.Message}");
            }

            if (webServerTask != null)
            {
                _log.Information("Waiting on Web server");
                webServerTask.Wait();
                _log.Information("Web server stopped");
            }
            if (_deferral != null)
            {
                _log.Information($"Task complete");
                _deferral.Complete();
            }
        }

        private LoopyCommand ReceiveAppCommand(LoopyCommand command)
        {
            _log.Information($"Received {command.ToString()} command from the Application");

            // echo the command back


            return command;
        }


        private void WebService_Canceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {

            _log.Information($"WebService_Canceled called with reason: {reason.ToString()}");
            if (_webServer != null)
            {
                _log.Information($"WebService_Canceled stopping Web Server");
                _webServer.StopServer();
                _webServer = null;
            }
            if (AppConnectionFactory.IsValid)
            {
                _log.Information($"Server_Canceled disposing AppService connection");
                AppConnectionFactory.Instance.Dispose();
            }


        }
    }
}
