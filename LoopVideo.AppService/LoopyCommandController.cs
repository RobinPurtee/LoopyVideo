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


namespace LoopVideo.AppService
{
    [RestController(InstanceCreationType.Singleton)]
    class LoopyCommandController
    {
        [UriFormat("/Play")]
        public IGetResponse PlayCommand()
        {
            LoopyCommand lc = new LoopyCommand(LoopyCommandType.Play, string.Empty);

            if (AppConnection.IsValid)
            {
                AppConnection.Instance.SendCommand(lc);
            }

            var response = new GetResponse(GetResponse.ResponseStatus.OK, lc);
            Debug.WriteLine("server responeding with: {0}", response);
            return response;
        }

        [UriFormat("/Stop")]
        public IGetResponse StopCommand()
        {
            LoopyCommand lc = new LoopyCommand(LoopyCommandType.Stop, string.Empty);

            if (AppConnection.IsValid)
            {
                AppConnection.Instance.SendCommand(lc);
            }

            var response = new GetResponse(GetResponse.ResponseStatus.OK, lc);
            Debug.WriteLine("server responeding with: {0}", response);
            return response;
        }
    }
}
