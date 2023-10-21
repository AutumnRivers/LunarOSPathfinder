using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LunarOSPathfinder.Executables;

using Pathfinder.Executable;

using HarmonyLib;

using Hacknet;

namespace LunarOSPathfinder.Patches
{
    [HarmonyPatch]
    public class LunarDefender_Reboot
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(OS), nameof(OS.rebootThisComputer))]
        static void Postfix()
        {
            OS os = OS.currentInstance;

            LunarDefender ldExe = new LunarDefender();

            if (os.Flags.HasFlag("KeepLDActive") && !os.exes.Contains(ldExe))
            {
                ldExe.bounds.X = os.ram.bounds.X;
                ldExe.bounds.Width = os.ram.bounds.Width;
                os.AddGameExecutable(ldExe);
            }
        }
    }
}
