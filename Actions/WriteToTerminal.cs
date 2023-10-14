using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hacknet;
using Pathfinder.Action;
using Pathfinder.Util;

namespace LunarOSPathfinder.Actions
{
    class WriteToTerminal : DelayablePathfinderAction
    {
        [XMLStorage]
        public string Quietly = "true";

        [XMLStorage(IsContent = true)]
        public string message = "";

        public override void Trigger(OS os)
        {
            bool isQuiet = (Quietly == "true");

            if(!isQuiet) { os.warningFlash(); };

            os.terminal.writeLine(" ");
            os.terminal.writeLine(message);
            os.terminal.writeLine(" ");
        }
    }
}
