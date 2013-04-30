using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Platformer
{
    public class ElementDefinition
    {
        public String name;
        public int id;
        public float speed;
        public float jump;
        public bool unlocked;

        public bool flamable; // Killed by fire
        public bool gas; // Killed by heat
        public bool floats; // NOT killed by water
        public bool freezes; // turns water to ice, and puts out fire
        public int level;    // Determines the level that the element unlocks (*.txt)
    }
}
