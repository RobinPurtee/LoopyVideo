﻿using System;
using Xunit;
using LoopyVideo.Commands;
using LoopyVideo.Logging;
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

                _log.Infomation($"Sending Loopy Command - {lc.ToString()}");
                LoopyCommand response = await connection.SendCommandAsync(lc);
                Assert.Equal(response.Command, LoopyCommand.CommandType.Play);
                _log.Infomation($"SendCommand exit with PlaybackStatus: {response.ToString()}");
            }
        }
    }
}
