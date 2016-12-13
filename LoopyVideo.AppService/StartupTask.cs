using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;
using Windows.ApplicationModel.Background;
using Windows.Foundation.Collections;
using Restup.Webserver.Rest;
using Restup.Webserver.Http;

// The Background Application template is documented at http://go.microsoft.com/fwlink/?LinkID=533884&clcid=0x409

namespace LoopyVideo.AppService
{
    public sealed class StartupTask : IBackgroundTask
    {

        private BackgroundTaskDeferral _defferral = null;
        private HttpServer _webServer = null;


        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            Debug.WriteLine($"The packagename is: {Windows.ApplicationModel.Package.Current.Id.FamilyName}");
            // save the defferal to keep the server running until the instance is Canceled
            _defferral = taskInstance.GetDeferral();
            taskInstance.Canceled += Server_Canceled;

            // setup the AppService Connection
            var serviceTrigger = taskInstance.TriggerDetails as AppServiceTriggerDetails;
            AppConnectionFactory.Instance.Connection = serviceTrigger.AppServiceConnection;

            await AppConnectionFactory.Instance.OpenConnectionAsync();
            if (AppConnectionFactory.Instance.Status == AppServiceConnectionStatus.Success )
            {
                AppConnectionFactory.Instance.MessageReceived += ReceiveAppCommand;
            }


            // setup the the web server
            //var restRouteHandler = new RestRouteHandler();
            //restRouteHandler.RegisterController<LoopyCommandController>();
            //var configuration = new HttpServerConfiguration()
            //  .ListenOnPort(8800)
            //  .RegisterRoute("loopy", restRouteHandler)
            //  .EnableCors();
            //try
            //{
            //    var _webServer = new HttpServer(configuration);
            //    Task serverTask = Task.Run(_webServer.StartServerAsync);
            //    serverTask.Wait();
            //}
            //catch (Exception ex)
            //{
            //    Debug.WriteLine($"Web Server Exception: {ex.Message}");
            //}
        }

        private ValueSet ReceiveAppCommand(ValueSet command)
        {
            Debug.WriteLine($"Received {command.ToString()} command from the Appication");

            // echo the command back
            ValueSet retset = command;
            Debug.WriteLine($"Echo response is: {retset.ToString()}");
            return retset;
        }

        private void Server_Canceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            if (_webServer != null)
            {
                _webServer.StopServer();
            }
            if (_defferral != null)
            {
                _defferral.Complete();
            }

        }
    }
}
