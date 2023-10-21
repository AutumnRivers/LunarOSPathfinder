using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Pathfinder.Action;
using Pathfinder.Util;

using Hacknet;

namespace LunarOSPathfinder.Actions
{
    public class CheckForDebug : PathfinderAction
    {
        [XMLStorage]
        public string Name;

        public override void Trigger(object os_obj)
        {
            bool isDebug = OS.DEBUG_COMMANDS;
            OS os = (OS)os_obj;

            if (Name == null) { Name = "Autumn"; };

            if (isDebug)
            {
                os.terminal.writeLine(" ");
                os.terminal.writeLine(Name + ":Howdy pardner! Looks like you got dat dere debug mode enabled.");
                os.terminal.writeLine("Aw, shucks, I'll let you pass this time.");
                os.terminal.writeLine("But don't do nothing that might break my good ol' extension, alright?");
                os.terminal.writeLine(" ");

                os.thisComputer.makeFile(os.thisComputer.ip, "DebugModeEnabled", "Do not delete this file, thanks :)", os.thisComputer.getFolderPath("home"));
            }
        }
    }
}
