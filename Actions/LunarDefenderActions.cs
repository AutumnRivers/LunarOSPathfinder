using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Hacknet;
using Pathfinder.Action;
using Pathfinder.Executable;
using Pathfinder.Util;

using LunarOSPathfinder.Executables;

namespace LunarOSPathfinder.Actions
{
    class LunarDefenderActions
    {
        public class LaunchLunarDefender : PathfinderAction
        {
            [XMLStorage]
            public string Force = "true";

            public override void Trigger(object os_obj)
            {
                Console.WriteLine("[LunarOSv3] Launching LunarDefender...");

                var os = (OS)os_obj;
                bool forceLaunch = false;

                var killer = new Hacknet.SAKillExe();
                killer.ExeName = "*";
                killer.Trigger(os_obj);

                ExecutableManager.AddGameExecutable(os, new LunarDefender(), os.ram.bounds, new string[0]);

                if(Force == "true") { forceLaunch = true; };

                if(forceLaunch) { os.Flags.AddFlag("KeepLDActive"); };
            }
        }

        public class KillLunarDefender : PathfinderAction
        {
            public override void Trigger(object os_obj)
            {
                var os = (OS)os_obj;

                var killer = new Hacknet.SAKillExe();
                killer.ExeName = "lunar";
                killer.Trigger(os_obj);

                if (os.Flags.HasFlag("KeepLDActive")) { os.Flags.RemoveFlag("KeepLDActive"); };
            }
        }
    }
}
