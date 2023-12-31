﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Pathfinder;
using Pathfinder.Executable;
using Pathfinder.Util;
using Pathfinder.Port;

using Hacknet;
using Hacknet.Gui;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;

namespace LunarOSPathfinder.Executables
{
    // Modified version of Armstrong made to be ran the player on their own machine
    public class ArmstrongPlayer : GameExecutable
    {
        public ArmstrongPlayer() : base()
        {
            this.baseRamCost = 350;
            this.ramCost = 350;
            this.IdentifierName = "ARMSTRONG_RC";
            this.name = "ARMSTRONGPLAYER";
            this.needsProxyAccess = true;
            this.CanBeKilled = false;
        }

        // Variables
        List<Vector2> crosshairPos = new List<Vector2>(4);

        // This is all for the middle rectangles
        // TODO: Use idfk dictionaries or something to fix this. this looks terrible
        List<Rectangle> overloadRects = new List<Rectangle>();
        List<float> overloadRectHeights = new List<float>();
        List<float> overRectOpTime = new List<float>();
        List<bool> raiseRectOpacities = new List<bool>();
        List<Color> rectColors = new List<Color>();

        // PLAYER - Rectangles to darken the screen
        Rectangle darkenRectangle = new Rectangle();
        float darkenOpacity = 0.0f;

        // Message to show in middle of module
        string msg = "";

        // delta interval
        private float interval = 75.0f;
        private float intervalDefault = 75.0f;
        private int intervalNo = 3;

        private bool connected = false;
        private bool completed = false;

        readonly string[] angles = { "up", "right", "down", "left" };

        public override void OnInitialize()
        {
            base.OnInitialize();

            var scrn = GuiData.spriteBatch.GraphicsDevice;

            darkenRectangle = new Rectangle()
            {
                X = scrn.Viewport.X,
                Y = scrn.Viewport.Y,
                Width = scrn.Viewport.Width,
                Height = scrn.Viewport.Height
            };

            os.Flags.AddFlag("LaunchedArmstrong");
        }

        public override void Draw(float t)
        {
            base.Draw(t);

            drawTarget();
            drawOutline();

            if(connected && !completed) { darkenOpacity = MathHelper.Lerp(darkenOpacity, 1.0f - intervalDefault / 500.0f, t); };

            if(completed) { darkenOpacity = MathHelper.Lerp(darkenOpacity, 0.0f, t); };

            RenderedRectangle.doRectangle(darkenRectangle.X, darkenRectangle.Y, darkenRectangle.Width, darkenRectangle.Height, Color.Black * darkenOpacity);

            crosshairPos.Add(new Vector2(bounds.Center.X, bounds.Center.Y));
                
            for(int i = 0; i < 4; i++)
            {
                Vector2 crossCenter = new Vector2();

                int centerSpacing = 30;

                switch(i)
                {
                    case 0:
                        crossCenter.X = bounds.X + centerSpacing;
                        crossCenter.Y = (bounds.Y + 10) + centerSpacing;
                        break;
                    case 1:
                        crossCenter.X = bounds.Width - centerSpacing;
                        crossCenter.Y = (bounds.Y + 10) + centerSpacing;
                        break;
                    case 2:
                        crossCenter.X = bounds.X + centerSpacing;
                        crossCenter.Y = (bounds.Y + bounds.Height) - centerSpacing;
                        break;
                    case 3:
                        crossCenter.X = bounds.Width - centerSpacing;
                        crossCenter.Y = (bounds.Y + bounds.Height) - centerSpacing;
                        break;
                }

                int length = 10;
                int thickness = 3;
                int spacing = 5;

                Rectangle crossRect = new Rectangle();

                for(int j = 0; j < 4; j++)
                {
                    switch (angles[j])
                    {
                        case "up":
                            crossRect.Height = length;
                            crossRect.Width = thickness;

                            crossRect.Y = ((int)crossCenter.Y - spacing) - crossRect.Height;
                            crossRect.X = (int)crossCenter.X - (crossRect.Width / 2);
                            break;
                        case "right":
                            crossRect.Height = thickness;
                            crossRect.Width = length;

                            crossRect.Y = (int)crossCenter.Y - (crossRect.Height / 2);
                            crossRect.X = (int)crossCenter.X + spacing;
                            break;
                        case "down":
                            crossRect.Height = length;
                            crossRect.Width = thickness;

                            crossRect.Y = (int)crossCenter.Y + spacing;
                            crossRect.X = (int)crossCenter.X - (crossRect.Width / 2);
                            break;
                        case "left":
                            crossRect.Height = thickness;
                            crossRect.Width = length;

                            crossRect.Y = (int)crossCenter.Y - (crossRect.Height / 2);
                            crossRect.X = ((int)crossCenter.X - spacing) - crossRect.Width;
                            break;
                    }

                    RenderedRectangle.doRectangle(crossRect.X, crossRect.Y, crossRect.Width, crossRect.Height, Color.White * 0.5f);
                }
            }

            interval -= (float)Math.Ceiling(MathHelper.SmoothStep(0.0f, 1.0f, t));

            if (interval <= 0)
            {
                intervalNo++;
                interval = intervalDefault;

                if(connected) { 
                    overloadRects.Add(new Rectangle(){ X = bounds.X });
                    overloadRectHeights.Add(0.0f);
                    overRectOpTime.Add(0.0f);
                    raiseRectOpacities.Add(true);
                    rectColors.Add(Color.WhiteSmoke);

                    if (intervalDefault > 50.0f) { intervalDefault /= 1.2f; };
                };
            }

            if(!connected)
            {
                msg = "Connecting.";

                for (int d = intervalNo % 3; d > 0; d--)
                {
                    msg += ".";
                }
            } else
            {
                msg = "CONNECTED";

                for (int squares = 0; squares < overloadRects.Count(); squares++)
                {
                    int index = squares;

                    float rectOpTime = overRectOpTime.ElementAt(index);
                    float rectHeight = overloadRectHeights.ElementAt(index);
                    bool raiseRectOpacity = raiseRectOpacities.ElementAt(index);
                    Color rectColor = rectColors.ElementAt(index);

                    rectHeight += 1.5f * t;

                    float rectOpacity = rectOpTime / 7.0f;

                    if (rectOpacity >= 0.8f || rectHeight >= 3.5f) { raiseRectOpacity = false; }

                    if(completed)
                    {
                        rectColor = os.brightUnlockedColor;
                        msg = "CRASHED";
                    }

                    if (raiseRectOpacity)
                    {
                        rectOpTime += 0.5f * t;
                    }
                    else
                    {
                        rectOpTime -= 0.5f * t;
                    }

                    overRectOpTime[index] = rectOpTime;
                    overloadRectHeights[index] = rectHeight;
                    raiseRectOpacities[index] = raiseRectOpacity;

                    if(rectOpacity < 0)
                    {
                        overloadRects.RemoveAt(index);
                        overloadRectHeights.RemoveAt(index);
                        overRectOpTime.RemoveAt(index);
                        raiseRectOpacities.RemoveAt(index);
                        rectColors.RemoveAt(index);
                    }

                    int realRectHeight = (int)Math.Ceiling(MathHelper.Lerp(rectHeight * 20, (rectHeight + 0.5f * t) * 20, MathHelper.SmoothStep(0.0f, 1.0f, t)));

                    int realRectYPos = bounds.Center.Y - (realRectHeight / 2);

                    float finalOpacity = 1.0f;

                    if(completed) { finalOpacity = 1.0f - ((total - 34.0f) / 3.0f); }

                    RenderedRectangle.doRectangle(bounds.X, realRectYPos, bounds.Width, realRectHeight, rectColor * rectOpacity * finalOpacity);
                }
            }

            Vector2 vector = GuiData.font.MeasureString(LocaleTerms.Loc(msg));
            Vector2 position = new Vector2((float)(tmpRect.X + bounds.Width / 2) - vector.X / 2f, (float)(bounds.Y + bounds.Height / 2 - 18));
            spriteBatch.DrawString(GuiData.font, LocaleTerms.Loc(msg), position, Color.WhiteSmoke);
        }

        private float total = 0.0f;
        private bool msgSent = false;
        private bool connectMsgSent = false;

        public override void Update(float t)
        {
            base.Update(t);

            total += t;

            if (total >= 5.0)
            {
                connected = true;

                if(!connectMsgSent)
                {
                    connectMsgSent = true;

                    this.CanBeKilled = false;

                    intervalDefault = 500.0f;

                    os.warningFlash();
                    os.beepSound.Play();
                    os.terminal.writeLine("(( --- CONNECTION ESTABLISHED --- ))");
                    os.terminal.writeLine("(( Sending an astronomical amount of malicious data... ))");
                }
            }

            if (total >= 34.0f && !msgSent)
            {
                var targetComp = os.thisComputer;

                targetComp.RemovePort("lunardefender");
                targetComp.portsNeededForCrack--;

                var killLDAction = new Actions.LunarDefenderActions.KillLunarDefender();

                killLDAction.Trigger(os);

                msgSent = true;
                completed = true;

                os.warningFlash();
                os.beepSound.Play();

                os.Flags.AddFlag("LDOffline");
                os.Flags.RemoveFlag("LaunchedArmstrong");

                this.CanBeKilled = true;

                os.terminal.writeLine("(( LunarDefender Service : SHUT DOWN ))");
                os.terminal.writeLine("(( <3 - Leon , Sakura , Lorelle , Benjamin ))");
            }

            if (total >= 37.5f)
            {
                this.isExiting = true;
            }
        }
    }
}
