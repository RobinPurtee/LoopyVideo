using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Http;
using Windows.ApplicationModel.Background;
using Restup.Webserver.Rest;
using Restup.Webserver.Attributes;
using Restup.Webserver.Http;
using System.Threading.Tasks;

// The Background Application template is documented at http://go.microsoft.com/fwlink/?LinkID=533884&clcid=0x409

namespace LoopyWebService
{
    public sealed class StartupTask : IBackgroundTask
    {
        private Task serverTask_ = null;

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            BackgroundTaskDeferral deferral = taskInstance.GetDeferral();
            var restRouteHandler = new RestRouteHandler();
            restRouteHandler.RegisterController<ParameterController>();

            var configuration = new HttpServerConfiguration()
              .ListenOnPort(8800)
              .RegisterRoute("api", restRouteHandler)
              .EnableCors();
            try
            {
                var httpServer = new HttpServer(configuration);
                serverTask_ = httpServer.StartServerAsync();
                serverTask_.Wait();
            }
            catch(Exception ex)
            {
                Debug.WriteLine(string.Format("Web Server Exception: {0}", ex.Message));
            }
        }
    }

    [RestController(Restup.Webserver.Models.Schemas.InstanceCreationType.Singleton)]
    public sealed class ParameterController
    {
        internal class DataReceived
        {
            public int ID { get; set; }
            public string PropName { get; set; }
        }

        [UriFormat("/simpleparameter/{id}/property/{propName}")]
        public Restup.Webserver.Models.Contracts.IGetResponse GetWithSimpleParameters(int id, string propName)
        {
            var response =  new Restup.Webserver.Models.Schemas.GetResponse(
              Restup.Webserver.Models.Schemas.GetResponse.ResponseStatus.OK,
              new DataReceived() { ID = id, PropName = propName });
            Debug.WriteLine("server responeding with: {0}", response);
            return response;
        }
    }
}
