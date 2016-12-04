using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Restup.Webserver.Rest;
using Restup.Webserver.Attributes;
using Restup.Webserver.Models.Contracts;
using Restup.Webserver.Models.Schemas;
using Windows.Foundation.Collections;


namespace LoopyVideo.AppService
{
    [RestController(InstanceCreationType.Singleton)]
    class LoopyCommandController
    {
        [UriFormat("/Play")]
        public IGetResponse PlayCommand()
        {
            //LoopyCommand lc = new LoopyCommand(LoopyCommand.CommandType.Play, string.Empty);

            //if (LoopyAppConnection.IsValid)
            //{
            //    Task.Run<ValueSet>(LoopyAppConnection.Instance.SendCommandAsync(lc)).Wait();
            //}

            //var response = new GetResponse(GetResponse.ResponseStatus.OK, lc);
            var response = new GetResponse(GetResponse.ResponseStatus.OK);
            Debug.WriteLine("server responding with: {0}", response);
            return response;
        }

        [UriFormat("/Stop")]
        public IGetResponse StopCommand()
        {
            //LoopyCommand lc = new LoopyCommand(LoopyCommand.CommandType.Stop, string.Empty);

            //if (LoopyAppConnection.IsValid)
            //{
            //    LoopyAppConnection.Instance.SendCommand(lc);
            //}

            //var response = new GetResponse(GetResponse.ResponseStatus.OK, lc);
            var response = new GetResponse(GetResponse.ResponseStatus.OK);
            Debug.WriteLine("server responding with: {0}", response);
            return response;
        }
    }
}
