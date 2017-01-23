//  ---------------------------------------------------------------------------------
//  Copyright (c)  Rick Purtee.  All rights reserved.
// 
//  The MIT License (MIT)
// 
//  Permission is hereby granted, free of charge, to any person obtaining a copy
//  of this software and associated documentation files (the "Software"), to deal
//  in the Software without restriction, including without limitation the rights
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//  copies of the Software, and to permit persons to whom the Software is
//  furnished to do so, subject to the following conditions:
// 
//  The above copyright notice and this permission notice shall be included in
//  all copies or substantial portions of the Software.
// 
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//  THE SOFTWARE.
//  ---------------------------------------------------------------------------------
using System;
using Windows.Foundation.Collections;

namespace LoopyVideo.Commands
{

    public sealed class LoopyCommand
    {
        private readonly static string commandName = "Command";
        private readonly static string paramName = "Param";

        public enum CommandType
        {
            Unknown,
            Error,
            Play,
            Stop,
            Media
        };


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
