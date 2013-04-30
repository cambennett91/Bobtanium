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
    class MenuItem
    {        
        private Rectangle OuterBox = new Rectangle();
        private Rectangle BoundingBox = new Rectangle();
        private Point Position = new Point();
        private int Level, World, Value;

        public MenuItem(Rectangle rect, Point pos, int level, int world)
        {
            BoundingBox = rect;
            OuterBox = new Rectangle(BoundingBox.Left - Tile.Width,
                                             BoundingBox.Top - Tile.Height,
                                             BoundingBox.Width + (Tile.Width * 2),
                                             BoundingBox.Height + (Tile.Height * 2));
            Position = pos;
            Level = level;
            World = world;
            Value = level + world;
        }

        public Rectangle getOuterBoundingBox() { return OuterBox; }
        public Rectangle getInnerBoundingBox() { return BoundingBox; }
        public Point getPosition() { return Position; }
        public int getValue() { return Value; }

        public bool isMouseInside(Rectangle mouse) {
            return mouse.Intersects(BoundingBox);
        }
        public String LoadNormalImage() {
            return ("Tiles/Buttons/MenuPlay/Levels/lv" + Level);
        }

        public String LoadHoverImage(){
             return ("Tiles/Buttons/MenuPlay/Levels/lv" + Level + "Hover");
        }
        public String LoadClickImage() {
            return ("Tiles/Buttons/MenuPlay/Levels/lv" + Level + "Click");
        }
    }
}
