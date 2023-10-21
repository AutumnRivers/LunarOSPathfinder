using System;
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
using Pathfinder.Meta;

using Pathfinder.Event.Gameplay;
using Pathfinder.Event.Saving;
using Pathfinder.Event.Loading;
using Pathfinder.Util.XML;

using Hacknet;
using Hacknet.Extensions;

using BepInEx;
using BepInEx.Hacknet;
using HarmonyLib;

using LunarOSPathfinder.Actions;
using LunarOSPathfinder.Commands;
using LunarOSPathfinder.Daemons;
using LunarOSPathfinder.Executables;
using LunarOSPathfinder.Patches;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace LunarOSPathfinder
{
    [BepInPlugin(ModGUID, ModName, ModVer)]
    [BepInDependency("kr.o_r.prodzpod.zerodaytoolkit")]
    public class LunarOSMod : HacknetPlugin
    {
        public const string ModGUID = "autumnrivers.lunarospf";
        public const string ModName = "LunarOSv3";
        public const string ModVer = "1.1.0";

        private Texture2D lunarOSLogo;
        private Texture2D eclipseOutline;
        private Texture2D eclipseFill;

        public static List<LunarDefenderComp> ldComps = new List<LunarDefenderComp>();

        private readonly Random random = new Random();

        public override bool Load()
        {
            HarmonyInstance.PatchAll(typeof(LunarOSMod).Assembly);

            ExtensionInfo extinfo = ExtensionLoader.ActiveExtensionInfo;
            string extensionFolder = extinfo.FolderPath;

            string fullExtensionPath = extensionFolder;

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
            ActionManager.RegisterAction<LunarDefenderActions.LaunchLunarDefender>("LaunchLunarDefender"); // Launch LunarDefender for the player
            ActionManager.RegisterAction<LunarDefenderActions.KillLunarDefender>("KillLunarDefender"); // Kill LunarDefender for the player
            ActionManager.RegisterAction<WriteToTerminal>("WriteToTerminal"); // Write to the terminal a la `writel`

            // Executables
            Console.WriteLine("[LunarOSv3] Registering Executables");
            ExecutableManager.RegisterExecutable<LunarDefender>("#LUNARDEFENDER_DO_NOT_USE_PLEASE_THANKS#"); // LunarDefender executable, need to register this, wish I didn't
            ExecutableManager.RegisterExecutable<LunarEclipse>("#LUNAR_ECLIPSE#"); // LunarEclipse - 3653
            ExecutableManager.RegisterExecutable<Armstrong>("#ARMSTRONG_EXE#"); // Armstrong - 7600 (Static)
            ExecutableManager.RegisterExecutable<ArmstrongPlayer>("#ARMSTRONG_PLAYER_EXE#"); // Same as above, but with fancy animation and flag setting

            // Commands
            Console.WriteLine("[LunarOSv3] Registering Debug Commands");
            CommandManager.RegisterCommand("turbokill", Killers.TurboKiller, false, false);
            CommandManager.RegisterCommand("plskilllunardefenderthankyouforever", Killers.LDKiller, true, true);
            CommandManager.RegisterCommand("godiwishihadmorefuckingram", Killers.PlayerLDKiller, false, false);

            // Launch LunarDefender when extension is loaded
            Action<OSLoadedEvent> lunarDefenderDelegate = CheckForDefender; // Check for a flag and launch LunarDefender on extension load
            Action<SaveComputerEvent> modifyLunarDefenderNodes = ModifyLDNodes; // Check if a computer needs to be registered as a "LunarDefender Node"
            Action<SaveComputerLoadedEvent> checkLDComps = CheckForLDComps;
            Action<OSUpdateEvent> checkRebootLDNodes = CheckAndRebootLDComps;

            EventManager<OSLoadedEvent>.AddHandler(lunarDefenderDelegate);
            EventManager<SaveComputerEvent>.AddHandler(modifyLunarDefenderNodes);
            EventManager<SaveComputerLoadedEvent>.AddHandler(checkLDComps);
            EventManager<OSUpdateEvent>.AddHandler(checkRebootLDNodes);

            Log.LogDebug("[LunarOSv3] Events Loaded");

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

                    if(currentComp == null) { continue; }

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

            Log.LogDebug("Rebooting LD Node: " + currentComp.idName);

            currentComp.AddPort(PortManager.GetPortRecordFromProtocol("lunardefender"));
            currentComp.portsNeededForCrack++;

            foreach(var port in currentComp.GetAllPortStates())
            {
                currentComp.closePort(port.PortNumber, "MOONSHINE_SERVICES");
            }

            Log.LogDebug("[LDNode:" + currentComp.idName + "] Ports Reset");

            if (currentComp.hasProxy)
            {
                currentComp.proxyActive = true;
                currentComp.proxyOverloadTicks = currentComp.startingOverloadTicks;

                Log.LogDebug("[LDNode:" + currentComp.idName + "] Proxy Reset");
            }

            if (currentComp.firewall != null)
            {
                currentComp.firewall.solved = false;
                currentComp.firewall.resetSolutionProgress();

                Log.LogDebug("[LDNode:" + currentComp.idName + "] Firewall Reset");
            }

            currentComp.adminIP = currentComp.ip;

            currentComp.setAdminPassword(RandomString(7));

            Log.LogDebug("[LDNode:" + currentComp.idName + "] Admin Reset");

            ldComps.Remove(currentLDComp);

            Log.LogDebug("[LDNode:" + currentComp.idName + "] Removed From Array");

            currentComp.log("LUNAR_DEFENDER_REBOOT_TRIGGERED");

            currentComp.reboot("MOONSHINE_SERVICES");

            Log.LogDebug("[LDNode:" + currentComp.idName + "] Rebooted...");
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
}
