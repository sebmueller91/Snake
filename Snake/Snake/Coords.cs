using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snake
{
    class Coords
    {
        public int x { get; set; }
        public int y { get; set; }

        public Coords(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public static Coords operator +(Coords c1, Coords c2)
        {
            return new Coords(c1.x + c2.x, c1.y + c2.y);
        }
    }
}
