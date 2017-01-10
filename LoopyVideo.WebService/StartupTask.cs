using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;
using Restup.Webserver.Http;
using Restup.Webserver.Rest;
using Restup.Webserver.File;

// The Background Application template is documented at http://go.microsoft.com/fwlink/?LinkID=533884&clcid=0x409

namespace LoopyVideo.WebService
{
    public sealed class StartupTask : IBackgroundTask
    {
        private LoopyVideo.Logging.Logger _log = new LoopyVideo.Logging.Logger("WebServiceProvider");

        private BackgroundTaskDeferral _deferral = null;
        private HttpServer _webServer = null;


        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            _log.Infomation($"WebService.Run Starting");
            // save the deferral to keep the server running until the instance is Canceled
            _deferral = taskInstance.GetDeferral();
            taskInstance.Canceled += Server_Canceled;

            // setup the AppService Connection
            var serviceTrigger = taskInstance.TriggerDetails as AppServiceTriggerDetails;
            AppConnectionFactory.Instance.Connection = serviceTrigger.AppServiceConnection;
            AppConnectionFactory.Instance.MessageReceived += ReceiveAppCommand; ;


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
                _log.Infomation("Creating Web Server");
                var _webServer = new HttpServer(configuration);
                await _webServer.StartServerAsync();
                _log.Infomation("Web Server Task ended");
            }
            catch (Exception ex)
            {
                _log.Infomation($"Web Server Exception: {ex.Message}");
            }
        }

        private ValueSet ReceiveAppCommand(ValueSet command)
        {
            _log.Infomation($"Received {LoopyVideo.Commands.ValueSetOut.ToString(command)} command from the Appication");

            //isBusy = command["Command"].ToString() != "Exit";


            // echo the command back
            ValueSet retset = new ValueSet();
            foreach (var pair in command)
            {
                retset.Add(pair);
            }
            _log.Infomation($"Echo response is: {LoopyVideo.Commands.ValueSetOut.ToString(retset)}");
            return retset;
        }

        private void Server_Canceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {

            _log.Infomation($"Server_Canceled called with reason: {reason.ToString()}");
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
                _log.Infomation($"Server_Canceled: Task complete");
                _deferral.Complete();
            }

        }
    }
}
