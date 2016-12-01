using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoopyVideo.WebServer
{
    public enum ActionType
    {
        Play,
        Stop,
        Media
    };

    public sealed class PlaybackAction
    {
        public ActionType Type { get; set; }
        public string MediaUri { get; set; }
    }
}
