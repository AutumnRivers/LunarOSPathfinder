using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Pathfinder.Executable;
using Pathfinder.Util;

using Hacknet;
using Hacknet.UIUtils;
using Hacknet.Gui;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace LunarOSPathfinder.Executables
{
    class LunarEclipse : GameExecutable
    {
        public LunarEclipse() : base()
        {
            this.baseRamCost = 100;
            this.ramCost = 250;
            this.IdentifierName = "LUNAR_ECLIPSE_WIP";
            this.name = "Lunar_Eclipse";
            this.needsProxyAccess = true;
        }

        public static Texture2D outline;
        public static Texture2D filled;

        private readonly Random random = new Random();

        private Color fillColor = Color.White;

        private bool isRed = true;

        private float fillWidth = 0;
        private int fillWidthMarker = 0;
        private int finalFillWidth = 0;

        private float fillHeight = 0;
        private int fillHeightMarker = 0;
        private int finalFillHeight = 0;

        private float fillOpacity = 0.1f;

        private bool active = false;

        float endCooldown = 1000.0f;

        string msg = "";

        public override void OnInitialize()
        {
            base.OnInitialize();

            int originPort = 3653;
            int displayPort = Programs.getComputer(os, targetIP).GetDisplayPortNumberFromCodePort(originPort);

            int originSSH = 22;
            int sshPort = Programs.getComputer(os, targetIP).GetDisplayPortNumberFromCodePort(originSSH);

            int originRTSP = 554;
            int rtspPort = Programs.getComputer(os, targetIP).GetDisplayPortNumberFromCodePort(originRTSP);

            if (Args.Length <= 1)
            {
                os.write("No Port Number Provided");
                os.write("Execution Failed");
                needsRemoval = true;
            } else if(!Args.Contains(displayPort.ToString()))
            {
                os.write("Target Port is Closed");
                os.write("Execution Failed");
                needsRemoval = true;
            } else if((!Args.Contains("-s") && !Args.Contains("-r"))
                || (Args.Contains("-s") && !Args.Contains(sshPort.ToString()))
                || (Args.Contains("-r") && !Args.Contains(rtspPort.ToString()))
            )
            {
                os.write("*() ERROR :: LunarEclipse Requires an Open SSH or RTSP Port ()*");
                os.write("Execution Failed");
                needsRemoval = true;
            } else if (
                (Args.Contains("-r") && Args.Contains(rtspPort.ToString()) && !Programs.getComputer(os, targetIP).isPortOpen(rtspPort))
                || (Args.Contains("-s") && Args.Contains(sshPort.ToString()) && !Programs.getComputer(os, targetIP).isPortOpen(sshPort))
            )
            {
                os.write("*() ERROR :: LunarEclipse Requires an **OPEN** SSH or RTSP Port ()*");
                os.write("Execution Failed");
                needsRemoval = true;
            } else
            {
                active = true;

                Programs.getComputer(os, targetIP).hostileActionTaken();

                os.terminal.writeLine("*() LunarEclipse :: Block Out The Moonshine! ()*");
            }

            fillWidthMarker = (int)Math.Ceiling(bounds.Width / 1.75f);
            fillHeightMarker = (int)Math.Ceiling(bounds.Height / 1.75f);
        }

        public override void Draw(float t)
        {
            if (!active) { return; }

            base.Draw(t);

            drawTarget();
            drawOutline();

            finalFillHeight = (int)Math.Ceiling(bounds.Height / 1.25f);
            finalFillWidth = (int)Math.Ceiling(bounds.Width / 1.25f);

            if (endCooldown <= 0) { this.Completed(); }

            // real sizes
            int realFillWidth = (int)Math.Ceiling(MathHelper.Lerp(fillWidth * 15, (fillWidth + 0.5f * t) * 15, MathHelper.SmoothStep(0.0f, 1.0f, t)));
            int realFillHeight = (int)Math.Ceiling(MathHelper.Lerp(fillHeight * 15, (fillHeight + 0.5f * t) * 15, MathHelper.SmoothStep(0.0f, 1.0f, t)));

            fillWidth += realFillWidth <= fillWidthMarker ? 0.9f * t : 0.6f * t;
            fillHeight += realFillHeight <= fillHeightMarker ? 0.9f * t : 0.6f * t;

            if(realFillWidth >= finalFillWidth) { 
                realFillWidth = (int)finalFillWidth;
                msg = "E    C   L  I P  S   E    D";
                endCooldown -= (float)Math.Ceiling(MathHelper.SmoothStep(0.0f, 1.0f, t));
            };

            if(realFillHeight >= finalFillHeight) { realFillHeight = (int)finalFillHeight; };

            // fill rect
            Rectangle innerMoonRect = new Rectangle()
            {
                Width = realFillWidth,
                Height = realFillHeight
            };

            innerMoonRect.Y = bounds.Y + bounds.Height / 2 - (int)(innerMoonRect.Height / 2.0f);
            innerMoonRect.X = (tmpRect.X + bounds.Width / 2) - (innerMoonRect.Width / 2);

            Rectangle moonRect = new Rectangle()
            {
                Width = (int)Math.Ceiling(bounds.Width / 1.25f),
                Height = (int)Math.Ceiling(bounds.Height / 1.25f)
            };

            moonRect.Y = bounds.Y + bounds.Height / 2 - (int)(moonRect.Height / 2.0f);
            moonRect.X = (tmpRect.X + bounds.Width / 2) - (moonRect.Width / 2);

            fillOpacity = (float)random.NextDouble() * (0.5f - 0.1f) + 0.1f;

            Color finalFillColor = isRed ? Color.Lerp(Color.Red, Color.Orange, t) : Color.Lerp(Color.Orange, Color.Red, t);

            fillColor = realFillWidth <= fillWidthMarker ? Color.White : Color.Lerp(fillColor, finalFillColor, MathHelper.SmoothStep(0.0f, 1.0f, t * 0.1f));

            if(finalFillColor == Color.Red)
            {
                isRed = true;
            } else if(finalFillColor == Color.Orange)
            {
                isRed = false;
            }

            GuiData.spriteBatch.Draw(filled, innerMoonRect, fillColor * fillOpacity);
            GuiData.spriteBatch.Draw(outline, moonRect, Color.White * 0.3f);

            float textOpacity = (float)random.NextDouble() * (1.0f - 0.4f) + 0.4f;

            Vector2 vector = GuiData.smallfont.MeasureString(LocaleTerms.Loc(msg));
            Vector2 position = new Vector2((float)(tmpRect.X + bounds.Width / 2) - vector.X / 2f, (float)(bounds.Y + bounds.Height / 2 - 10));
            spriteBatch.DrawString(GuiData.smallfont, LocaleTerms.Loc(msg), position, Color.WhiteSmoke * textOpacity);
        }

        private float total = 0.0f;
        private bool msgSent = false;
        public override void Update(float t)
        {
            base.Update(t);

            total += t;

            if(total >= 17.0f)
            {
                Programs.getComputer(os, targetIP).openPort(3653, os.thisComputer.ip);
                if(!msgSent)
                {
                    msgSent = true;
                    os.terminal.writeLine("*() LunarEclipse :: SUCCESS :: ECLIPSED ()*");
                }
            }

            if(total >= 25.0f)
            {
                this.isExiting = true;
            }
        }
    }
}
