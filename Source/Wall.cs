using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDCHOST24
{
    public class Wall
    {
        public Dot w1;
        public Dot w2;
        public Wall(Dot iw1, Dot iw2)
        {
            w1 = iw1;
            w2 = iw2;
        }
        public Wall(Wall aWall)
        {
            w1 = aWall.w1;
            w2 = aWall.w2;
        }
    }
}
