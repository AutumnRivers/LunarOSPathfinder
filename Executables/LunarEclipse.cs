using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Pathfinder.Executable;

using Microsoft.Xna.Framework;

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

        public override void Draw(float t)
        {
            base.Draw(t);

            
        }
    }
}
