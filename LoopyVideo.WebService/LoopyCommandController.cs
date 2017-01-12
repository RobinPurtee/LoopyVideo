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
            LoopyCommand lc = new LoopyCommand(command, string.Empty);
            ValueSet commandReturnSet;
            if (AppConnectionFactory.IsValid)
            {
                Task<AppServiceResponse> sendTask = AppConnectionFactory.Instance.SendCommandAsync(lc.ToValueSet()).AsTask();
                sendTask.Wait();
                commandReturnSet = sendTask.Result.Message;
            }

            var response = new GetResponse(GetResponse.ResponseStatus.OK, lc);
            Debug.WriteLine("Command responding with: {0}", response);
            return response;
        }

        [UriFormat("/Play")]
        public IGetResponse PlayCommand()
        {
            return SendAppCommand(LoopyCommand.CommandType.Play);
        }

        [UriFormat("/Stop")]
        public IGetResponse StopCommand()
        {
            return SendAppCommand(LoopyCommand.CommandType.Stop);
        }
    }
}
