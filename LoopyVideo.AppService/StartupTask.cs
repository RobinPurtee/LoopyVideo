using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppService;
using Windows.ApplicationModel.Background;
using Restup.Webserver.Rest;
using Restup.Webserver.Http;

// The Background Application template is documented at http://go.microsoft.com/fwlink/?LinkID=533884&clcid=0x409

namespace LoopyVideo.AppService
{
    public sealed class StartupTask : IBackgroundTask
    {

        private BackgroundTaskDeferral _defferral = null;
        private HttpServer _webServer = null;


        public void Run(IBackgroundTaskInstance taskInstance)
        {
            Debug.WriteLine($"The packagename is: {Windows.ApplicationModel.Package.Current.Id.FamilyName}");
            // save the defferal to keep the server running until the instance is Canceled
            _defferral = taskInstance.GetDeferral();
            taskInstance.Canceled += Server_Canceled;

            // setup the AppService Connection
            var serviceTrigger = taskInstance.TriggerDetails as AppServiceTriggerDetails;
            //            LoopyAppConnection.Instance.Connection = serviceTrigger.AppServiceConnection;

            // setup the the web server
            var restRouteHandler = new RestRouteHandler();
            restRouteHandler.RegisterController<LoopyCommandController>();
            var configuration = new HttpServerConfiguration()
              .ListenOnPort(8800)
              .RegisterRoute("loopy", restRouteHandler)
              .EnableCors();
            try
            {
                var _webServer = new HttpServer(configuration);
                Task serverTask = Task.Run(_webServer.StartServerAsync);
                serverTask.Wait();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(string.Format("Web Server Exception: {0}", ex.Message));
            }
        }



        /// <summary>
        /// Receive request from LoopyVideo front-end
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void AppConnection__RequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
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
