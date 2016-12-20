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
using LoopyVideo.Commands;

namespace LoopyVideo.AppService
{
    //[RestController(InstanceCreationType.Singleton)]
    //class LoopyCommandController
    //{
    //    private IGetResponse SendAppCommand(LoopyCommand.CommandType command, string param = "")
    //    {
    //        LoopyCommand lc = new LoopyCommand(LoopyCommand.CommandType.Play, string.Empty);
    //        ValueSet commandReturnSet;
    //        if (AppConnectionFactory.IsValid)
    //        {
    //            Task<ValueSet> sendTask = AppConnectionFactory.Instance.SendCommandAsync(lc);
    //            sendTask.Wait();
    //            commandReturnSet = sendTask.Result;
    //        }

    //        var response = new GetResponse(GetResponse.ResponseStatus.OK, lc);
    //        Debug.WriteLine("Command responding with: {0}", response);
    //        return response;
    //    }

    //    [UriFormat("/Play")]
    //    public IGetResponse PlayCommand()
    //    {
    //        return SendAppCommand(LoopyCommand.CommandType.Play);
    //    }

    //    [UriFormat("/Stop")]
    //    public IGetResponse StopCommand()
    //    {
    //        return SendAppCommand(LoopyCommand.CommandType.Stop);
    //    }
    //}
}
