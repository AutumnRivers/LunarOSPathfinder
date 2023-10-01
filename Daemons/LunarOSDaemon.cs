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
        public string TitleYOffset = "-10";

        [XMLStorage]
        public string SubXOffset = "-125";

        [XMLStorage]
        public string SubYOffset = "25";

        [XMLStorage]
        public string ButtonText = "Debug Menu";

        public override void draw(Rectangle bounds, SpriteBatch sb)
        {
            base.draw(bounds, sb);

            // TODO: Add LunarOS graphic to background. Doesn't have to be super fancy

            // Parse strings as int
            int titleXOffset = int.Parse(TitleXOffset);
            int titleYOffset = int.Parse(TitleYOffset);
            int subXOffset = int.Parse(SubXOffset);
            int subYOffset = int.Parse(SubYOffset);

            var center = os.display.bounds.Center; // Get center of daemon display
            TextItem.doLabel(new Vector2(center.X + titleXOffset, center.Y + titleYOffset), "LunarOS v" + Version, Color.Aquamarine); // Draw LunarOS version

            if(Subtitle != null) { TextItem.doLabel(new Vector2(center.X + subXOffset, center.Y + subYOffset), Subtitle, Color.White); } // Draw (optional) subtitle

            var backButton = Button.doButton(123, os.display.bounds.X + 30, os.display.bounds.Y + 30, 125, 50, ButtonText, Color.CornflowerBlue); // Exit daemon button

            if(backButton) { os.display.command = "connect"; }
        }
    }
}
