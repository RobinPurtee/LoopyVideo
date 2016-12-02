using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation.Collections;

namespace LoopVideo.AppService
{
    public enum LoopyCommandType
    {
        Unknown,
        Play,
        Stop,
        Media
    };

    internal class LoopyCommand
    {
        public LoopyCommandType Command { get; set; }
        public string Param { get; set; }

        public LoopyCommand()
        {
            Command = LoopyCommandType.Unknown;
            Param = string.Empty;
        }

        public LoopyCommand(LoopyCommandType c, string p)
        {
            Command = c;
            Param = p;
        }

        public override string ToString()
        {
            return $"Command: {Command.ToString()}  Param: {Param}";
        }
    }

    internal static class LoopyCommandHelper
    {
        private readonly static string commandName = "Command";
        private readonly static string paramName = "Param";

        public static ValueSet ToValueSet(LoopyCommand lc)
        {
            ValueSet ret = new ValueSet();
            AddToValueSet(lc, ret);
            return ret;
        }

        public static LoopyCommand FromValueSet(ValueSet values)
        {
            LoopyCommand lc = new LoopyCommand();

            if (values.ContainsKey(commandName))
            {
                lc.Command = (LoopyCommandType)Enum.Parse(typeof(LoopyCommandType), values[commandName].ToString());
            }
            if (values.ContainsKey(paramName))
            {
                lc.Param = (string)values[paramName];
            }
            return lc;
        }


        public static void AddToValueSet(LoopyCommand lc, ValueSet set)
        {
            set.Add(commandName, lc.Command.ToString());
            if (!string.IsNullOrEmpty(lc.Param))
            {
                set.Add(paramName, lc.Param);
            }
        }

    }


}
