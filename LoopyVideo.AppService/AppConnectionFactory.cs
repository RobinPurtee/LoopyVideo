using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LoopyVideo.Commands;

namespace LoopyVideo.AppService
{
    internal class AppConnectionFactory
    {
        private static readonly Lazy<LoopyAppConnection> _instance = new Lazy<LoopyAppConnection>(() => new LoopyAppConnection());
        internal static LoopyAppConnection Instance
        {
            get { return _instance.Value; }
        }
        internal static bool IsValid
        {
            get { return _instance.IsValueCreated && _instance.Value.Connection != null; }
        }
    }
}
