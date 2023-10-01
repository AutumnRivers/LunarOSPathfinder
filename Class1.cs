using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

using Pathfinder;
using Pathfinder.Port;
using Pathfinder.Daemon;
using Pathfinder.Executable;
using Pathfinder.Command;

using Hacknet;

using BepInEx;
using BepInEx.Hacknet;
using HarmonyLib;

using LunarOSPathfinder.Daemons;

namespace LunarOSPathfinder
{
    [BepInPlugin(ModGUID, ModName, ModVer)]
    public class LunarOSMod : HacknetPlugin
    {
        public const string ModGUID = "autumnrivers.lunarospf";
        public const string ModName = "LunarOSv3";
        public const string ModVer = "0.1.0";

        public override bool Load() {
            var i = 0;
            foreach(var type in Assembly.GetExecutingAssembly().DefinedTypes)
            {
                i++;
                if(type.GetCustomAttribute(typeof(HarmonyPatch)) != null)
                {
                    Log.LogDebug("Patching " + type);
                    HarmonyInstance.PatchAll(type);
                }
            }

            Console.WriteLine("[LunarOSv3] Registering Ports");
            PortManager.RegisterPort("moonshine", "Moonshine Services", 3653); // Moonshine Services for LunarOS
            PortManager.RegisterPort("lunardefender", "LunarDefender", 7600); // LunarDefender

            Console.WriteLine("[LunarOSv3] Registering Daemons");
            DaemonManager.RegisterDaemon<LunarOSDaemon>(); // LunarOS
            DaemonManager.RegisterDaemon<VaultDaemon>(); // Vaults

            return true;

        }
    }
}
