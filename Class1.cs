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
using Pathfinder.Action;

using Hacknet;

using BepInEx;
using BepInEx.Hacknet;
using HarmonyLib;

using LunarOSPathfinder.Daemons;
using Hacknet.Extensions;
using System.IO;
using Microsoft.Xna.Framework.Graphics;

namespace LunarOSPathfinder
{

    [BepInPlugin(ModGUID, ModName, ModVer)]
    public class LunarOSMod : HacknetPlugin
    {
        public const string ModGUID = "autumnrivers.lunarospf";
        public const string ModName = "LunarOSv3";
        public const string ModVer = "0.1.0";

        private Texture2D lunarOSLogo;

        public override bool Load()
        {
            var i = 0;
            foreach (var type in Assembly.GetExecutingAssembly().DefinedTypes)
            {
                i++;
                if (type.GetCustomAttribute(typeof(HarmonyPatch)) != null)
                {
                    Log.LogDebug("Patching " + type);
                    HarmonyInstance.PatchAll(type);
                }
            }

            ExtensionInfo extinfo = ExtensionLoader.ActiveExtensionInfo;
            string extensionFolder = extinfo.FolderPath;
            string hacknetFolder = "./";

            string fullExtensionPath = hacknetFolder + extensionFolder;

            // Logo stuff
            FileStream logoStream = File.OpenRead(fullExtensionPath + "/Images/LunarOSLogo.png");
            lunarOSLogo = Texture2D.FromStream(GuiData.spriteBatch.GraphicsDevice, logoStream);
            logoStream.Dispose();

            Log.LogDebug("[LunarOSv3] Preloading Static Images...");
            LunarOSDaemon.logo = lunarOSLogo;
            Log.LogDebug("[LunarOSv3] LunarOSDaemon - LunarOSLogo Preloaded!");

            Console.WriteLine("[LunarOSv3] Registering Ports");
            PortManager.RegisterPort("moonshine", "Moonshine Services", 3653); // Moonshine Services for LunarOS
            PortManager.RegisterPort("lunardefender", "LunarDefender", 7600); // LunarDefender

            Console.WriteLine("[LunarOSv3] Registering Daemons");
            DaemonManager.RegisterDaemon<LunarOSDaemon>(); // LunarOS
            DaemonManager.RegisterDaemon<VaultDaemon>(); // Vaults

            Console.WriteLine("[LunarOSv3] Registering Actions");
            ActionManager.RegisterAction<CheckForDebug>("DebugCheck");

            return true;

        }
    }

    public class CheckForDebug : PathfinderAction
    {
        public override void Trigger(object os_obj)
        {
            bool isDebug = OS.DEBUG_COMMANDS;
            OS os = (OS)os_obj;

            if (isDebug)
            {
                os.terminal.writeLine(" ");
                os.terminal.writeLine("Autumn:Howdy pardner! Looks like you got dat dere debug mode enabled.");
                os.terminal.writeLine("Aw, shucks, I'll let you pass this time.");
                os.terminal.writeLine("But don't do nothing that might break my good ol' extension, alright?");
                os.terminal.writeLine(" ");

                os.thisComputer.makeFile(os.thisComputer.ip, "DebugModeEnabled", "Do not delete this file, thanks :)", os.thisComputer.getFolderPath("home"));
            }
        }
    }
}
