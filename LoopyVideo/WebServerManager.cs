using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;


namespace LoopyVideo
{
    public class WebServerManager
    {
        private static string serverName = "LoopyVideo.WebServer";

        BackgroundTaskRegistration server_ = null;

        public bool IsRegistered
        {
            get
            {
                bool bRet = false;
                foreach (var task in BackgroundTaskRegistration.AllTasks)
                {
                    if (task.Value.Name == serverName)
                    {
                        bRet = true;
                        break;
                    }
                }

                return bRet;
            }
        }

        public WebServerManager()
        {


        }

        public async void Register()
        {
            if(!IsRegistered)
            {
                BackgroundTaskBuilder builder = new BackgroundTaskBuilder();
                builder.Name = serverName;
                builder.TaskEntryPoint = "LoopyVideo.WebServer.Server";
                builder.IsNetworkRequested = true;
                builder.AddCondition(new SystemCondition(SystemConditionType.FreeNetworkAvailable));
                server_ = builder.Register();
                server_.Completed += OnServerExit;   
              
            }


        }

        private void OnServerExit(IBackgroundTaskRegistration sender, BackgroundTaskCompletedEventArgs args)
        {
            server_.Unregister(false);
        }



    }
}
