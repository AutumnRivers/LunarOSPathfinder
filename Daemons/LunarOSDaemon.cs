using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Hacknet;
using Hacknet.Gui;

using Pathfinder;
using Pathfinder.Daemon;
using Pathfinder.Util;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;

using LunarOSPathfinder.Properties;
using System.Windows.Media.Imaging;

using Hacknet.Extensions;
using Pathfinder.Replacements;

namespace LunarOSPathfinder.Daemons
{
    public class LunarOSDaemon : BaseDaemon
    {
        public LunarOSDaemon(Computer computer, string serviceName, OS opSystem) : base(computer, serviceName, opSystem) { }

        public override string Identifier => "LunarOS";

        [XMLStorage]
        public string Version = "1.0";

        [XMLStorage]
        public string Subtitle;

        [XMLStorage]
        public string TitleXOffset = "-100";

        [XMLStorage]
        public string TitleYOffset = "-60";

        [XMLStorage]
        public string SubXOffset = "-125";

        [XMLStorage]
        public string SubYOffset = "-25";

        [XMLStorage]
        public string ButtonText = "Debug Menu";

        public static Texture2D logo;

        public override void draw(Rectangle bounds, SpriteBatch sb)
        {
            base.draw(bounds, sb);

            // Parse strings as int
            int titleXOffset = int.Parse(TitleXOffset);
            int titleYOffset = int.Parse(TitleYOffset);
            int subXOffset = int.Parse(SubXOffset);
            int subYOffset = int.Parse(SubYOffset);

            var center = os.display.bounds.Center; // Get center of daemon display

            Rectangle logoRect = new Rectangle();
            logoRect.X = center.X - 250;
            logoRect.Y = center.Y - 150;
            logoRect.Width = 325;
            logoRect.Height = 325;

            GuiData.spriteBatch.Draw(logo, logoRect, Color.White * 0.25f); // Draw Logo

            GuiData.spriteBatch.DrawString(GuiData.smallfont, "This machine protected with Moonshine(TM) Technologies Software.\nhttps://moonshine.tech/lunardefender", new Vector2(bounds.X + 175, bounds.Y + 38), Color.CornflowerBlue);

            //TextItem.doLabel(new Vector2(center.X + titleXOffset, center.Y + titleYOffset), "LunarOS v" + Version, Color.Aquamarine); // Draw LunarOS version

            Vector2 vector = GuiData.font.MeasureString(LocaleTerms.Loc("LunarOS v" + Version));
            Vector2 position = new Vector2((float)(bounds.X + bounds.Width / 2) - vector.X / 2f, (float)(bounds.Y + bounds.Height / 2 - 10) - 20f);
            GuiData.spriteBatch.DrawString(GuiData.font, LocaleTerms.Loc("LunarOS v" + Version), position, Color.Aquamarine);

            if (Subtitle != null) {
                //TextItem.doLabel(new Vector2(center.X + subXOffset, center.Y + subYOffset), Subtitle, Color.White);

                Vector2 subVector = GuiData.font.MeasureString(LocaleTerms.Loc(Subtitle));
                Vector2 subPosition = new Vector2((float)(bounds.X + bounds.Width / 2) - subVector.X / 2f, (float)(bounds.Y + bounds.Height / 2 - 10) + 15f);
                GuiData.spriteBatch.DrawString(GuiData.font, LocaleTerms.Loc(Subtitle), subPosition, Color.White);
            } // Draw (optional) subtitle

            bool backButton = false;

            if (comp.PlayerHasAdminPermissions())
            {
                backButton = Button.doButton(123, os.display.bounds.X + 30, os.display.bounds.Y + 30, 125, 50, ButtonText, Color.CornflowerBlue); // Exit daemon button 
            }

            if (backButton) { os.display.command = "connect"; }
        }
    }
}
