using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using System.IO;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Input;

namespace Platformer
{
    class Catalyst
    {
        public Point position;
        public TileCollision collisiontype;
        public Texture2D texture;
        public bool collected;

        public Catalyst()
        {
            collected = false;
        }

        public void ChangePosition(Point pos)
        {
            position = pos;
        }

        public void ChangeType(TileCollision i)
        {
            collisiontype = i;
        }

        

    }
}
