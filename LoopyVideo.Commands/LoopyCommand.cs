using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation.Collections;
using Windows.UI.Xaml.Data;

namespace LoopyVideo.Commands
{
    public enum CommandType
    {
        Unknown,
        Error,
        Play,
        Stop,
        Media
    };

    public sealed class LoopyCommand
    {
        private readonly static string commandName = "Command";
        private readonly static string paramName = "Param";



        public CommandType Command { get; set; }
        public string Param { get; set; }

        public LoopyCommand() : this(CommandType.Unknown, ""){ }
        public LoopyCommand(CommandType c) : this(c, "") { }
        //public LoopyCommand(LoopyCommand lc) : this(lc.Command, lc.Param) { }
        public LoopyCommand(CommandType c, string p)
        {
            Command = c;
            Param = p;
        }

        public void Copy(LoopyCommand lc)
        {
            Command = lc.Command;
            Param = lc.Param;
        }
        public static LoopyCommand FromValueSet(ValueSet values)
        {
            LoopyCommand lc = new LoopyCommand();

            if (values.ContainsKey(commandName))
            {
                lc.Command = (CommandType)Enum.Parse(typeof(CommandType), values[commandName].ToString());
            }
            if (values.ContainsKey(paramName))
            {
                lc.Param = (string)values[paramName];
            }
            return lc;
        }

 
        public ValueSet ToValueSet()
        {
            ValueSet ret = new ValueSet();
            ret.Add(commandName, Command.ToString());
            if (!string.IsNullOrEmpty(Param))
            {
                ret.Add(paramName, Param);
            }
            return ret;
        }

        //public void AddToValueSet(out ValueSet set)
        //{
        //    set.Add(commandName, Command.ToString());
        //    if (!string.IsNullOrEmpty(Param))
        //    {
        //        set.Add(paramName, Param);
        //    }
        //}

        public override string ToString()
        {
            return $"Command: {Command.ToString()}  Param: {Param}";
        }
    }

}
