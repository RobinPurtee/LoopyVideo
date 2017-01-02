using System;
using Xunit;
using LoopyVideo.Commands;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;

namespace AppServiceUnitTest
{
    public class AppServiceTests
    {
        private LoopyVideo.Logging.Logger _log = new LoopyVideo.Logging.Logger("LoopyAppServiceUnitTest");

        [Fact]
        public async void AppServicePlayTest()
        {
            using (AppConnection connection = new AppConnection("AppServiceUnitTest.connection"))
            {
                var status = await connection.OpenConnectionAsync();
                Assert.Equal(AppServiceConnectionStatus.Success, status);

                LoopyCommand lc = new LoopyCommand(LoopyCommand.CommandType.Play, null);
                Assert.True(connection.IsValid(), "The appliccation connection is not valid");
                var messageSet = new ValueSet();
                //LoopyCommandHelper.AddToValueSet(command, messageSet);
                messageSet.Add("Command", lc.Command.ToString());

                _log.Infomation($"Sending Value Set - {ValueSetOut.ToString(messageSet)}");
                AppServiceResponse response = await connection.SendCommandAsync(messageSet);
                Assert.Equal(response.Status, AppServiceResponseStatus.Success);
                Assert.True(response.Message.Keys.Contains("Command"), "Command Response did not contain a Command Key");
                lc = LoopyCommand.FromValueSet(response.Message);
                Assert.Equal(lc.Command, LoopyCommand.CommandType.Play);
                _log.Infomation($"SendCommand exit with PlaybackStatus: {response.Status.ToString()}");
            }
        }
    }
}
