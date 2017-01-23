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
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.AppService;
using Restup.Webserver.Http;
using Restup.Webserver.Rest;
using Restup.Webserver.File;
using LoopyVideo.Logging;


// The Background Application template is documented at http://go.microsoft.com/fwlink/?LinkID=533884&clcid=0x409

namespace LoopyVideo.WebService
{
    public sealed class StartupTask : IBackgroundTask
    {
        private Logger _log = new Logger("WebServiceProvider");

        private BackgroundTaskDeferral _deferral = null;
        private HttpServer _webServer = null;


        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            _log.Information($"WebService.Run Starting");
            // save the deferral to keep the server running until the instance is Canceled
            _deferral = taskInstance.GetDeferral();
            taskInstance.Canceled += Server_Canceled;
            // setup the AppService Connection
            try
            {
                var serviceTrigger = taskInstance.TriggerDetails as AppServiceTriggerDetails;
                AppConnectionFactory.Instance.Connection = serviceTrigger.AppServiceConnection;
            }
            catch(Exception ex)
            {
                _log.Error($"Error starting App Service connection: {ex.Message}");
            }

            if (_webServer == null)
            {
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
                    await _webServer.StartServerAsync();
                    _log.Information("Web Server Started");
                }
                catch (Exception ex)
                {
                    _log.Information($"Web Server Exception: {ex.Message}");
                }
            }
        }


        private void Server_Canceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {

            _log.Information($"Server_Canceled called with reason: {reason.ToString()}");
            if (_webServer != null)
            {
                _webServer.StopServer();
            }
            if (AppConnectionFactory.IsValid)
            {
                AppConnectionFactory.Instance.Dispose();
            }

            if (_deferral != null)
            {
                _log.Information($"Server_Canceled: Task complete");
                _deferral.Complete();
            }

        }
    }
}
