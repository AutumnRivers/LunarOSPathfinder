using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Pathfinder.Port;

using ZeroDayToolKit.Executibles;

using HarmonyLib;

using Hacknet;

namespace LunarOSPathfinder.Patches
{
    [HarmonyPatch]
    public class PortBackdoor_LunarOS
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(PortBackdoorEXE), nameof(PortBackdoorEXE.LoadContent))]
        static bool Prefix(PortBackdoorEXE __instance)
        {
            Computer targetComp = Programs.getComputer(OS.currentInstance, __instance.targetIP);

            if(targetComp.GetAllPortStates().Exists(p => p.Record == PortManager.GetPortRecordFromProtocol("lunardefender")))
            {
                OS.currentInstance.terminal.writeLine("LunarOSv2+later patch backdoor vuln - read README for more info");
                OS.currentInstance.terminal.writeLine("Execution Failed");
                __instance.needsRemoval = true;
                return false;
            } else
            {
                return true;
            }
        }
    }
}
