using System;
using Xunit;
using LoopyVideo.Commands;
using LoopyVideo.Logging;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;
using System.Threading.Tasks;

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
                Assert.True(connection.IsValid(), "The application connection is not valid");

                LoopyCommand lc = new LoopyCommand(LoopyCommand.CommandType.Play, null);

                _log.Information($"Sending Loopy Command - {lc.ToString()}");
                LoopyCommand response = await connection.SendCommandAsync(lc);
                _log.Information($"SendCommand exit with PlaybackStatus: {response.ToString()}");
                Assert.Equal(LoopyCommand.CommandType.Play, response.Command);
            }
        }

        [Fact]
        public async void AppServiceStateTest()
        {
            using (AppConnection connection = new AppConnection("AppServiceUnitTest.connection"))
            {
                var status = await connection.OpenConnectionAsync();
                Assert.Equal(AppServiceConnectionStatus.Success, status);
                Assert.True(connection.IsValid(), "The application connection is not valid");
                LoopyCommand lc = new LoopyCommand(LoopyCommand.CommandType.State, null);
                LoopyCommand response;

                await TestPlayerStatusAsync(connection, LoopyCommand.CommandType.Unknown);
                lc.Command = LoopyCommand.CommandType.Play;
                response = await connection.SendCommandAsync(lc);
                _log.Information($"Play command sent : {response.ToString()}");
                Assert.Equal(LoopyCommand.CommandType.Play, response.Command);
                await TestPlayerStatusAsync(connection, LoopyCommand.CommandType.Play);
                lc.Command = LoopyCommand.CommandType.Stop;
                response = await connection.SendCommandAsync(lc);
                _log.Information($"Stop command sent : {response.ToString()}");
                Assert.Equal(LoopyCommand.CommandType.Stop, response.Command);
                await TestPlayerStatusAsync(connection, LoopyCommand.CommandType.Stop);

            }
        }
            
        private Task TestPlayerStatusAsync(AppConnection connection, LoopyCommand.CommandType testState)
        {
            return Task.Run(async () =>
            {
                LoopyCommand lc = new LoopyCommand(LoopyCommand.CommandType.State, null);
                LoopyCommand response = await connection.SendCommandAsync(lc);
                _log.Information($"Current state : {response.ToString()}");
                Assert.Equal(testState, response.Command);
            });

        }


    }
}
