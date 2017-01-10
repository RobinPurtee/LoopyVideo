using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LoopyVideo.Commands;

namespace LoopyVideo.WebService
{
    internal class AppConnectionFactory
    {
        private static readonly Lazy<AppConnection> _instance = new Lazy<AppConnection>(() => new AppConnection("WebService.ConnectionFactory"));
        internal static AppConnection Instance
        {
            get { return _instance.Value; }
        }
        internal static bool IsValid
        {
            get { return _instance.IsValueCreated && _instance.Value.Connection != null; }
        }
    }
}
