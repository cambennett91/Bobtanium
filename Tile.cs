#region File Description
//-----------------------------------------------------------------------------
// Tile.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Platformer
{
    /// <summary>
    /// Controls the collision detection and response behavior of a tile.
    /// </summary>
    enum TileCollision
    {
        /// <summary>
        /// A passable tile is one which does not hinder player motion at all.
        /// </summary>
        Passable = 0,

        /// <summary>
        /// An impassable tile is one which does not allow the player to move through
        /// it at all. It is completely solid.
        /// </summary>
        Impassable = 1,

        /// <summary>
        /// A platform tile is one which behaves like a passable tile except when the
        /// player is above it. A player can jump up through a platform as well as move
        /// past it to the left and right, but can not fall down through the top of it.
        /// </summary>
        Platform = 2,
        
        /// <summary>
        /// A Spikes Tile is Passable, but the player dies when they touch it   .
        /// </summary>
        Spikes = 3,


        /// <summary>
        /// Fire kills the player on contact unless playing as Iron, Gold, Mercury, and Water.
        /// also causes player to explode when playing as helium and nitrogen.
        /// also causes player to rise when playing as oxygen.
        /// water extiguishes fire (renders it inert or just a chared tile).
        /// passable.
        /// </summary> 
        Fire = 4,

        Heat = 5,

        /// <summary> 
        /// Ice kills the player on contact unless playing as a gas (oxygen, helium, Liquid nitrogen).
        /// slows down water, mercury, gold.
        /// passable 
        /// </summary>
        Ice = 6,

        /// <summary>
        /// Water kills player unless playing as carbon, liquid nitrogen
        /// carbon acts as a platform, liquid nitrogen causes water to freeze creating ice
        /// </summary>
        Water = 7,

        /// <summary>
        /// Debris kills the player on contact unless playing as a gas
        /// passable.
        /// </summary>
        Debris = 8,

        /// <sumary>
        /// enemies kill the player on contact UNLESS:
        /// Enemy type
        /// Green: no special attributes
        /// Red: does not harm player IF playing as water, liquid nitrogen, mercury, Iron.
        /// Blue: does not harm player IF playing as oxygen, helium, gold, carbon
        /// Yellow: no special attributes, fast enemy.
        /// </sumary>
        Enemy = 9,

        /// <summary>
        /// Changes the player into a different element when they hit these types of tile.
        /// </summary>

        TransformReset = 10,
        Transform1 = 11,
        Transform2 = 12,
        Transform3 = 13,
        Transform4 = 14,
        Transform5 = 15,
        Transform6 = 16,
        Transform7 = 17,
        Transform8 = 18,
        Transform9 = 19,

    }

    /// <summary>
    /// Stores the appearance and collision behavior of a tile.
    /// </summary>
    struct Tile
    {
        public Texture2D Texture;
        public TileCollision Collision;
        public String Name;

        public const int Width = 40;
        public const int Height = 32;

        public static readonly Vector2 Size = new Vector2(Width, Height);

        /// <summary>
        /// Constructs a new tile.
        /// </summary>
        public Tile(Texture2D texture, TileCollision collision, String name)
        {
            Texture = texture;
            Collision = collision;
            Name = name;
        }
        public String getName()
        {
            return Name;
        }
    }
}
