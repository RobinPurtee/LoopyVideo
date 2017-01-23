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

        Logging.Logger _log = new Logging.Logger("LoopyCommandController");

        private IGetResponse SendAppCommand(LoopyCommand.CommandType command, string param = "")
        {
            LoopyCommand lc = new LoopyCommand(command, param);
            LoopyCommand retCommand = new LoopyCommand(LoopyCommand.CommandType.Error, "AppConnection is invalid");
            _log.Information($"Sending command {lc.ToString()}");
            if (AppConnectionFactory.IsValid)
            {
                retCommand = AppConnectionFactory.Instance.SendCommandAsync(lc).GetAwaiter().GetResult();
            }

            var response = new GetResponse(GetResponse.ResponseStatus.OK, retCommand);
            _log.Information($"Command responding with: {response.ToString()}");
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
