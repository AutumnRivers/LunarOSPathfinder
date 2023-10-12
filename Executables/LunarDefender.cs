using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Pathfinder;
using Hacknet;
using Hacknet.Gui;
using LunarOSPathfinder;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LunarOSPathfinder.Executables
{
    class LunarDefender : Pathfinder.Executable.GameExecutable
    {
        public LunarDefender() : base() {
            this.baseRamCost = 200;
            this.ramCost = 200;
            this.PID = 1;
            this.CanBeKilled = false;
            this.IdentifierName = "LunarDefender";
            this.name = "LunarDefender";
        }

        private readonly Random random = new Random();

        public enum Status : int
        {
            Active = 0,
            Deadlocked = 1,
            Inactive = 2,
            Crashed = 3
        }

        bool rectIsActive = false;
        int rectYPosition = 0;
        float rectHeight = 0.0f;

        float rectOpacity = 0.0f;
        float rectOpTime = 0.0f;
        bool raiseRectOpacity = true;

        public static Texture2D logo;

        public override void Draw(float t)
        {
            base.Draw(t);

            drawTarget();
            drawOutline();

            int logoWidth = bounds.Width / 2;
            int logoHeight = (int)Math.Ceiling(bounds.Height / 1.75f);

            Rectangle logoRect = new Rectangle();
            logoRect.X = bounds.Center.X / 2;
            logoRect.Y = (int)Math.Ceiling(bounds.Center.Y / 1.55f);
            logoRect.Width = logoWidth;
            logoRect.Height = logoHeight;

            GuiData.spriteBatch.Draw(logo, logoRect, Color.White * 0.1f); // Draw Logo

            Vector2 vector = GuiData.smallfont.MeasureString(LocaleTerms.Loc("LunarDefender is ACTIVE"));
            Vector2 position = new Vector2((float)(tmpRect.X + bounds.Width / 2) - vector.X / 2f, (float)(bounds.Y + bounds.Height / 2 - 10));
            spriteBatch.DrawString(GuiData.smallfont, LocaleTerms.Loc("LunarDefender is ACTIVE"), position, Color.White);

            // Background FX
            if(!rectIsActive) {
                rectYPosition = random.Next(bounds.Y + 30, (bounds.Height + bounds.Y) - 30);
                rectIsActive = true;
            }

            if(rectIsActive)
            {
                int realRectHeight = (int)Math.Ceiling(MathHelper.Lerp(rectHeight * 20, (rectHeight + 0.5f * t) * 20, MathHelper.SmoothStep(0.0f, 1.0f, t)));
                int realRectYPos = rectYPosition - (realRectHeight / 2);

                rectOpacity = rectOpTime / 10.0f;

                RenderedRectangle.doRectangle(bounds.X, realRectYPos, bounds.Width, realRectHeight, Color.WhiteSmoke * rectOpacity, false);

                rectHeight += 1.0f * t;

                if (rectOpTime >= 2.0f) { raiseRectOpacity = false; }

                if(raiseRectOpacity)
                {
                    rectOpTime += 2f * t;
                } else
                {
                    rectOpTime -= 2f * t;
                }

                if(rectHeight >= 2.0f)
                {
                    rectHeight = 0.0f;
                    rectOpTime = 0.0f;
                    rectIsActive = false;
                    raiseRectOpacity = true;
                }
            }
        }

        public override void Update(float t)
        {
            this.bounds.Width = os.ram.bounds.Width;

            base.Update(t);
        }
    }
}
