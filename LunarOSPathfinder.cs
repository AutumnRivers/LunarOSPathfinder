﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.IO;

using Pathfinder;
using Pathfinder.Port;
using Pathfinder.Event;
using Pathfinder.Daemon;
using Pathfinder.Executable;
using Pathfinder.Command;
using Pathfinder.Action;
using Pathfinder.Util;

using Pathfinder.Event.Gameplay;
using Pathfinder.Event.Saving;
using Pathfinder.Event.Loading;
using Pathfinder.Util.XML;

using Hacknet;
using Hacknet.Extensions;

using BepInEx;
using BepInEx.Hacknet;
using HarmonyLib;

using LunarOSPathfinder.Daemons;
using LunarOSPathfinder.Executables;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace LunarOSPathfinder
{

    [BepInPlugin(ModGUID, ModName, ModVer)]
    public class LunarOSMod : HacknetPlugin
    {
        public const string ModGUID = "autumnrivers.lunarospf";
        public const string ModName = "LunarOSv3";
        public const string ModVer = "0.1.0";

        private Texture2D lunarOSLogo;
        private Texture2D eclipseOutline;
        private Texture2D eclipseFill;

        public static List<LunarDefenderComp> ldComps = new List<LunarDefenderComp>();

        private readonly Random random = new Random();

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

            GraphicsDevice userGraphics = GuiData.spriteBatch.GraphicsDevice;

            // Logo stuff
            FileStream logoStream = File.OpenRead(fullExtensionPath + "/Images/LunarOSLogo.png");
            lunarOSLogo = Texture2D.FromStream(userGraphics, logoStream);
            logoStream.Dispose();

            FileStream eclipseOutlineStream = File.OpenRead(fullExtensionPath + "/Images/Eclipse/LunarEclipse_Outline.png");
            eclipseOutline = Texture2D.FromStream(userGraphics, eclipseOutlineStream);
            eclipseOutlineStream.Dispose();

            FileStream eclipseFillStream = File.OpenRead(fullExtensionPath + "/Images/Eclipse/LunarEclipse_Fill.png");
            eclipseFill = Texture2D.FromStream(userGraphics, eclipseFillStream);
            eclipseFillStream.Dispose();

            // Images
            Log.LogDebug("[LunarOSv3] Preloading Static Images...");
            LunarOSDaemon.logo = lunarOSLogo;
            LunarDefender.logo = lunarOSLogo;
            Log.LogDebug("[LunarOSv3] LunarOSDaemon & LunarDefender - LunarOSLogo Preloaded!");
            Log.LogDebug("[LunarOSv3] Preloading LunarEclipse Images...");
            LunarEclipse.outline = eclipseOutline;
            Log.LogDebug("[LunarOSv3] LunarEclipse - Outline Preloaded!");
            LunarEclipse.filled = eclipseFill;
            Log.LogDebug("[LunarOSv3] LunarEclipse - Fill Preloaded!");
            Log.LogDebug("[LunarOSv3] Images Preloaded!");

            // Ports
            Console.WriteLine("[LunarOSv3] Registering Ports");
            PortManager.RegisterPort("moonshine", "Moonshine Services", 3653); // Moonshine Services for LunarOS
            PortManager.RegisterPort("lunardefender", "LunarDefender", 7600); // LunarDefender
            PortManager.RegisterPort("projectpf", "Pathfinder Services", 1961); // Project Pathfinder Services

            // Daemons
            Console.WriteLine("[LunarOSv3] Registering Daemons");
            DaemonManager.RegisterDaemon<LunarOSDaemon>(); // LunarOS
            DaemonManager.RegisterDaemon<VaultDaemon>(); // Vaults

            // Actions
            Console.WriteLine("[LunarOSv3] Registering Actions");
            ActionManager.RegisterAction<CheckForDebug>("DebugCheck"); // Check for debug mode
            ActionManager.RegisterAction<Actions.LunarDefenderActions.LaunchLunarDefender>("LaunchLunarDefender"); // Launch LunarDefender for the player
            ActionManager.RegisterAction<Actions.LunarDefenderActions.KillLunarDefender>("KillLunarDefender"); // Kill LunarDefender for the player
            ActionManager.RegisterAction<Actions.WriteToTerminal>("WriteToTerminal"); // Write to the terminal a la `writel`

            // Executables
            Console.WriteLine("[LunarOSv3] Registering Executables");
            ExecutableManager.RegisterExecutable<LunarDefender>("#LUNARDEFENDER_DO_NOT_USE_PLEASE_THANKS#"); // LunarDefender executable, need to register this, wish I didn't
            ExecutableManager.RegisterExecutable<LunarEclipse>("#LUNAR_ECLIPSE#"); // LunarEclipse - 3653
            ExecutableManager.RegisterExecutable<Armstrong>("#ARMSTRONG_EXE#"); // Armstrong - 7600 (Static)
            ExecutableManager.RegisterExecutable<ArmstrongPlayer>("#ARMSTRONG_PLAYER_EXE#"); // Same as above, but with fancy animation and flag setting

            // Launch LunarDefender when extension is loaded
            Action<OSLoadedEvent> lunarDefenderDelegate = CheckForDefender; // Check for a flag and launch LunarDefender on extension load
            Action<SaveComputerEvent> modifyLunarDefenderNodes = ModifyLDNodes; // Check if a computer needs to be registered as a "LunarDefender Node"
            Action<SaveComputerLoadedEvent> checkLDComps = CheckForLDComps;
            Action<OSUpdateEvent> checkRebootLDNodes = CheckAndRebootLDComps;

            EventManager<OSLoadedEvent>.AddHandler(lunarDefenderDelegate);
            EventManager<SaveComputerEvent>.AddHandler(modifyLunarDefenderNodes);
            EventManager<SaveComputerLoadedEvent>.AddHandler(checkLDComps);
            EventManager<OSUpdateEvent>.AddHandler(checkRebootLDNodes);

            return true;
        }

        public void CheckForDefender(OSLoadedEvent os_load_event)
        {
            OS os = os_load_event.Os;
            LunarDefender ldExe = new LunarDefender();

            if (os.Flags.HasFlag("KeepLDActive") && !os.exes.Contains(ldExe))
            {
                ldExe.bounds.X = os.ram.bounds.X;
                ldExe.bounds.Width = os.ram.bounds.Width;
                os.AddGameExecutable(ldExe);
            }
        }

        public void CheckAndRebootLDComps(OSUpdateEvent os_event)
        {
            GameTime gt = os_event.GameTime;
            float subtractBy = (float)(gt.ElapsedGameTime.TotalMilliseconds / 1000);

            if(ldComps.Count() > 0)
            {
                for(int index = 0; index < ldComps.Count(); index++)
                {
                    LunarDefenderComp currentComp = ldComps.ElementAt(index);

                    currentComp.rebootTimer -= subtractBy;

                    if(currentComp.rebootTimer <= 0) { RebootAndResetNodes(currentComp); }
                    else { ldComps[index] = currentComp; }
                }
            }
        }

        public void CheckForLDComps(SaveComputerLoadedEvent scl_event)
        {
            ElementInfo xCompElem = scl_event.Info;

            if(xCompElem.Attributes.ContainsKey("timeUntilReset"))
            {
                float timeUntilReset = float.Parse(xCompElem.Attributes["timeUntilReset"]);

                LunarDefenderComp newLDComp = new LunarDefenderComp()
                {
                    comp = scl_event.Comp,
                    rebootTimer = timeUntilReset
                };

                ldComps.Add(newLDComp);
            }
        }

        public void ModifyLDNodes(SaveComputerEvent save_comp_event)
        {
            Computer comp = save_comp_event.Comp;
            XElement xCompElem = save_comp_event.Element;

            if(ldComps.Exists(ldc => ldc.comp.idName == comp.idName))
            {
                var ldComp = ldComps.First(ldc => ldc.comp.idName == comp.idName);

                xCompElem.Add(new XAttribute("timeUntilReset", (int)Math.Ceiling(ldComp.rebootTimer)));

                save_comp_event.Element = xCompElem;
            }
        }

        public class LunarDefenderComp
        {
            public Computer comp { get; set; }
            public float rebootTimer { get; set; }
        }

        public void RebootAndResetNodes(LunarDefenderComp currentLDComp)
        {
            Computer currentComp = currentLDComp.comp;

            currentComp.AddPort(PortManager.GetPortRecordFromProtocol("lunardefender"));
            currentComp.portsNeededForCrack++;

            foreach(var port in currentComp.GetAllPortStates())
            {
                currentComp.closePort(port.PortNumber, "MOONSHINE_SERVICES");
            }

            currentComp.proxyActive = true;
            currentComp.proxyOverloadTicks = currentComp.startingOverloadTicks;
            currentComp.firewall.solved = false;
            currentComp.firewall.resetSolutionProgress();
            currentComp.adminIP = currentComp.ip;

            currentComp.setAdminPassword(RandomString(7));

            ldComps.Remove(currentLDComp);

            currentComp.log("LUNAR_DEFENDER_REBOOT_TRIGGERED");

            currentComp.reboot("MOONSHINE_SERVICES");
        }

        public void AddLDComp(Computer targetComp)
        {
            LunarDefenderComp currentComp = new LunarDefenderComp()
            {
                comp = targetComp,
                rebootTimer = (float)random.Next(25, 35)
            };

            ldComps.Add(currentComp);
        }

        public string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789"; // Alphanumeric
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }

    public class CheckForDebug : PathfinderAction
    {
        [XMLStorage]
        public string Name;

        public override void Trigger(object os_obj)
        {
            bool isDebug = OS.DEBUG_COMMANDS;
            OS os = (OS)os_obj;

            if(Name == null) { Name = "Autumn"; };

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
