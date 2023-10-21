using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Pathfinder.Port;

using Hacknet;

using LunarOSPathfinder.Actions;

namespace LunarOSPathfinder.Commands
{
    public class Killers
    {
        public static void TurboKiller(OS os, string[] args)
        {
            if(!OS.DEBUG_COMMANDS) { return; }

            Computer targetComp = os.connectedComp;

            if(targetComp.hasProxy)
            {
                targetComp.proxyActive = false;
            }

            if(targetComp.firewall != null)
            {
                targetComp.firewall.solved = true;
            }

            OS.currentInstance.terminal.writeLine("TURBO :: KILLED");
            return;
        }

        public static void LDKiller(OS os, string[] args)
        {
            if (!OS.DEBUG_COMMANDS) { return; }

            Computer targetComp = os.connectedComp;

            if(targetComp.GetAllPortStates().Exists(p => p.Record == PortManager.GetPortRecordFromProtocol("lunardefender")))
            {
                targetComp.RemovePort("lunardefender");
                targetComp.portsNeededForCrack--;
                OS.currentInstance.terminal.writeLine("LunarDefender Killed");
            } else
            {
                OS.currentInstance.terminal.writeLine("No LunarDefender Port Found");
            }
        }

        public static void PlayerLDKiller(OS os, string[] args)
        {
            if (!OS.DEBUG_COMMANDS) { return; }

            var killer = new Hacknet.SAKillExe();
            killer.ExeName = "lunar";
            killer.Trigger(os);

            if (os.Flags.HasFlag("KeepLDActive")) { os.Flags.RemoveFlag("KeepLDActive"); };
        }
    }
}
