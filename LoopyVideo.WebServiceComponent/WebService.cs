using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;
using Restup.Webserver.Http;
using Restup.Webserver.Rest;




namespace LoopyVideo.WebServiceComponent
{
    public sealed class WebService : IBackgroundTask
    {
        private LoopyVideo.Logging.Logger _log = new LoopyVideo.Logging.Logger("WebServiceProvider");

        private BackgroundTaskDeferral _defferral = null;
        private HttpServer _webServer = null;


        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            _log.Infomation($"WebService.Run Starting");
            // save the defferal to keep the server running until the instance is Canceled
            _defferral = taskInstance.GetDeferral();
            taskInstance.Canceled += Server_Canceled;

            // setup the AppService Connection
            var serviceTrigger = taskInstance.TriggerDetails as AppServiceTriggerDetails;
            AppConnectionFactory.Instance.Connection = serviceTrigger.AppServiceConnection;
            AppConnectionFactory.Instance.MessageReceived += ReceiveAppCommand; ;


            // setup the the web server
            //var restRouteHandler = new RestRouteHandler();
            //restRouteHandler.RegisterController<LoopyCommandController>();
            //var configuration = new HttpServerConfiguration()
            //  .ListenOnPort(8800)
            //  .RegisterRoute("loopy", restRouteHandler)
            //  .EnableCors();
            //try
            //{
            //    _log.Infomation("Creating Web Server");
            //    var _webServer = new HttpServer(configuration);
            //    Task serverTask = Task.Run(_webServer.StartServerAsync);
            //    serverTask.Wait();
            //    _log.Infomation("Web Server Task ended");
            //}
            //catch (Exception ex)
            //{
            //    _log.Infomation($"Web Server Exception: {ex.Message}");
            //}
        }

 
        private ValueSet ReceiveAppCommand(ValueSet command)
        {
            _log.Infomation($"Received {LoopyVideo.Commands.ValueSetOut.ToString(command)} command from the Appication");

            //isBusy = command["Command"].ToString() != "Exit";


            // echo the command back
            ValueSet retset = new ValueSet();
            foreach(var pair in command)
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
            if(AppConnectionFactory.IsValid)
            {
                AppConnectionFactory.Instance.Dispose();
            }

            if (_defferral != null)
            {
                _log.Infomation($"Server_Canceled: Task complete");
                _defferral.Complete();
            }

        }
    }
}
