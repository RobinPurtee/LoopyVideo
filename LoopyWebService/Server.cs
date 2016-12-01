using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using Windows.ApplicationModel.AppService;
using Windows.ApplicationModel.Background;
using Windows.Foundation.Collections;
using Restup.Webserver.Rest;
using Restup.Webserver.Attributes;
using Restup.Webserver.Http;
using System.Threading.Tasks;

// The Background Application template is documented at http://go.microsoft.com/fwlink/?LinkID=533884&clcid=0x409

namespace LoopyVideo.WebServer
{
    public sealed class Server : IBackgroundTask
    {
        private Task serverTask_ = null;
        private BackgroundTaskDeferral deferral_ = null;
        private AppServiceConnection appConnection_ = null;


        public void Run(IBackgroundTaskInstance taskInstance)
        {
            // save the defferal to keep the server running until the instance is Canceled
            deferral_ = taskInstance.GetDeferral();
            taskInstance.Canceled += Server_Canceled;

            // setup the AppService Connection
            var serviceTrigger = taskInstance.TriggerDetails as AppServiceTriggerDetails;
            appConnection_ = serviceTrigger.AppServiceConnection;
            appConnection_.RequestReceived += AppConnection__RequestReceived;

            // setup the the web server
            var restRouteHandler = new RestRouteHandler();
            restRouteHandler.RegisterController<PlaybackController>();
            var configuration = new HttpServerConfiguration()
              .ListenOnPort(8800)
              .RegisterRoute("loopy", restRouteHandler)
              .EnableCors();
            try
            {
                var httpServer = new HttpServer(configuration);
                serverTask_ = Task.Run( httpServer.StartServerAsync );
                serverTask_.Wait();
            }
            catch(Exception ex)
            {
                Debug.WriteLine(string.Format("Web Server Exception: {0}", ex.Message));
            }
        }

        public async void SendPlaybackActionToApp(PlaybackAction action)
        {
            var message = new ValueSet();
            try
            {
                message.Add(action.Type.ToString(), action);

                var response = await appConnection_.SendMessageAsync(message);
                if (response.Status == AppServiceResponseStatus.Success)
                {
                    var result = response.Message["Response"];
                    Debug.WriteLine($"The client responded to the command with: {result.ToString()}");
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception Sending action {action.Type.ToString()} : {ex.Message} ");
                throw;
            }
        }


        /// <summary>
        /// Receive request from LoopyVideo front-end
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void AppConnection__RequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            var requestDefferal = args.GetDeferral();

            requestDefferal.Complete();
        }

        private void Server_Canceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            if(deferral_ != null)
            {
                deferral_.Complete();
            }

        }
    }





    [RestController(Restup.Webserver.Models.Schemas.InstanceCreationType.Singleton)]
    public sealed class PlaybackController
    {
        internal class CommandReceived
        {
            public string CommandName { get; set; }
        }

        [UriFormat("/Play")]
        public Restup.Webserver.Models.Contracts.IGetResponse PlayCommand()
        {
            var response =  new Restup.Webserver.Models.Schemas.GetResponse(
              Restup.Webserver.Models.Schemas.GetResponse.ResponseStatus.OK,
              new CommandReceived() { CommandName="Play" });
            Debug.WriteLine("server responeding with: {0}", response);
            return response;
        }

        [UriFormat("/Stop")]
        public Restup.Webserver.Models.Contracts.IGetResponse StopCommand()
        {
            var response = new Restup.Webserver.Models.Schemas.GetResponse(
              Restup.Webserver.Models.Schemas.GetResponse.ResponseStatus.OK,
              new CommandReceived() { CommandName = "Stop" });
            Debug.WriteLine("server responeding with: {0}", response);
            return response;
        }
    }
}
