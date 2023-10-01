using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Hacknet;
using Hacknet.Gui;

using Pathfinder;
using Pathfinder.Daemon;
using Pathfinder.Util;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LunarOSPathfinder.Daemons
{
    public class VaultDaemon : BaseDaemon
    {
        public VaultDaemon(Computer computer, string serviceName, OS opSystem) : base(computer, serviceName, opSystem) { }

        public override string Identifier => "TsukiVault";

        private static readonly Random random = new Random();

        [XMLStorage]
        public string Name = "TsukiVault";

        [XMLStorage]
        public string FlagPrefix = "vault"; // Vault keys are stored via user flags. If FlagPrefix is "moonshine", then the first key would be named "moonshine1," then the second "moonshine2," and so on.

        [XMLStorage]
        public string SecretCode = RandomString(7);

        [XMLStorage]
        public string MaximumKeys = "5";

        public override void draw(Rectangle bounds, SpriteBatch sb)
        {
            // Set the admin password to the secret code
            this.comp.adminPass = SecretCode;

            int boundaryX = bounds.X;
            int boundaryY = bounds.Y;

            int maxKeys = int.Parse(MaximumKeys);

            // Title of vault
            GuiData.spriteBatch.DrawString(GuiData.font, Name, new Vector2(boundaryX + 30, boundaryY + 30), Color.WhiteSmoke, 0.0f, Vector2.Zero, 2.1f, SpriteEffects.None, 0.1f);

            // Fancy divider
            RenderedRectangle.doRectangle(boundaryX, boundaryY + 125, bounds.Width, 30, Color.MediumPurple);

            // We use flags for the keys, this gets the user's flags
            ProgressionFlags userFlags = OS.currentInstance.Flags;

            // Base/interval offsets for the for loop
            int baseOffset = 180;
            int offsetInterval = 60;

            int unlockedKeys = 0;

            // Save the buttons for later
            bool grantedButton = false;
            bool deniedButton = false;

            // Draw a button for each key
            for(int i=0;i<=maxKeys + 1;i++)
            {
                if (i < maxKeys && userFlags.HasFlag(FlagPrefix + (i + 1).ToString())) // Haven't reached maximum keys, and user has current key
                {
                    unlockedKeys++;
                    Button.doButton(11 * (i + 1), boundaryX + 20, boundaryY + (baseOffset + (offsetInterval * i)), 400, 40, (i + 1).ToString() + " - UNLOCKED", Color.Green);
                } else if(i < maxKeys) { // Haven't reached maximum keys
                    Button.doButton(11 * (i + 1), boundaryX + 20, boundaryY + (baseOffset + (offsetInterval * i)), 400, 40, (i + 1).ToString() + " - LOCKED", Color.Red);
                } else if(i == maxKeys && unlockedKeys == maxKeys) // Reached maximum keys, and user has all keys
                {
                    grantedButton = Button.doButton(211, boundaryX + 20, boundaryY + (baseOffset + (offsetInterval * i)), 400, 30, "ACCESS GRANTED :: " + SecretCode, Color.Green);
                    if(userFlags.HasFlag("unlocked" + FlagPrefix) == false) { OS.currentInstance.Flags.AddFlag("unlocked" + FlagPrefix); }
                } else if(i == maxKeys) // Reached maximum keys, draw ACCESS DENIED button
                {
                    deniedButton = Button.doButton(211, boundaryX + 20, boundaryY + (baseOffset + (offsetInterval * i)), 400, 30, "ACCESS DENIED", Color.Black);
                }
            }

            if(grantedButton) { OS.currentInstance.runCommand("login"); }; // Send the user to the login screen for the ACCESS GRANTED button

            if(deniedButton) { // Warn the user they still need all the keys
                os.warningFlash();
                os.write("\nAccess to the vault is denied. All " + MaximumKeys + " keys are required.\n");
            };

            // *notices your code comment* owo
            TextItem.doFontLabel(new Vector2(boundaryX + 20, bounds.Height - 20), "This server protected with TsukiVault technology! q^o^p\nRead more at https://tsukivault.lun/ uwu~", GuiData.smallfont, Color.LightPink);
        }

        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789"; // Alphanumeric
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

    }
}
