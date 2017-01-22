using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Restup.Webserver.Attributes;
using Restup.Webserver.Models.Contracts;
using Restup.Webserver.Models.Schemas;
using Windows.Foundation.Collections;
using Windows.ApplicationModel.AppService;
using LoopyVideo.Commands;

namespace LoopyVideo.WebService
{
    [RestController(InstanceCreationType.Singleton)]
    class LoopyCommandController
    {
        private IGetResponse SendAppCommand(LoopyCommand.CommandType command, string param = "")
        {
            LoopyCommand lc = new LoopyCommand(command, param);
            LoopyCommand retCommand = new LoopyCommand(LoopyCommand.CommandType.Error, "AppConnection is invalid");
            if (AppConnectionFactory.IsValid)
            {
                Task<LoopyCommand> sendTask = AppConnectionFactory.Instance.SendCommandAsync(lc).AsTask();
                sendTask.Wait();
                retCommand = sendTask.Result;
            }

            var response = new GetResponse(GetResponse.ResponseStatus.OK, retCommand);
            Debug.WriteLine("Command responding with: {0}", response);
            return response;
        }

        [UriFormat("/play")]
        public IGetResponse PlayCommand()
        {
            return SendAppCommand(LoopyCommand.CommandType.Play);
        }

        [UriFormat("/stop")]
        public IGetResponse StopCommand()
        {
            return SendAppCommand(LoopyCommand.CommandType.Stop);
        }
    }
}
