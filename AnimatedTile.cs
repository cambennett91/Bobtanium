#region File Description
//-----------------------------------------------------------------------------
// Enemy.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

namespace Platformer
{
    /// <summary>
    /// A monster who is impeding the progress of our fearless adventurer.
    /// </summary>
    class AnimatedTile
    {
        public Level Level
        {
            get { return level; }
        }
        Level level;

        /// <summary>
        /// Position in world space of the bottom center of this enemy.
        /// </summary>
        public Vector2 Position
        {
            get { return position; }
        }
        Vector2 position;

        private Rectangle localBounds;
        /// <summary>
        /// Gets a rectangle which bounds this enemy in world space.
        /// </summary>
        public Rectangle BoundingRectangle
        {
            get
            {
                int left = (int)Math.Round(Position.X - sprite.Origin.X) + localBounds.X;
                int top = (int)Math.Round(Position.Y - sprite.Origin.Y) + localBounds.Y;

                return new Rectangle(left, top, localBounds.Width, localBounds.Height);
            }
        }

        // Animations
        private AnimationPlayer sprite;
        private Animation idleAnimation;
        private Animation dieAnimation;

        private bool playAltAnimation;

        // Sounds
        private SoundEffect killedSound;

        public bool IsAlive { get; private set; }

        /// <summary>
        /// Constructs a new Enemy.
        /// </summary>
        public AnimatedTile(Level level, Vector2 position, string spriteSet)
        {
            this.level = level;
            this.position.X = (position.X * Tile.Width)+Tile.Width/2;
            this.position.Y = (position.Y+1) * Tile.Height;
            
            this.playAltAnimation = false;

            this.IsAlive = true;

            LoadContent(spriteSet);
        }

        /// <summary>
        /// Loads a particular enemy sprite sheet and sounds.
        /// </summary>
        public void LoadContent(string spriteSet)
        {
            // Load animations.
            dieAnimation = new Animation(Level.Content.Load<Texture2D>("Tiles/Hazards/null"), 0.07f, false);
            spriteSet = "Tiles/Hazards/" + spriteSet;
            idleAnimation = new Animation(Level.Content.Load<Texture2D>(spriteSet), 0.1f, true);
            
            sprite.PlayAnimation(idleAnimation);

            // Load sounds.
            //killedSound = Level.Content.Load<SoundEffect>("Sounds/MonsterKilled");

            // Calculate bounds within texture size.
            int width = (int)(idleAnimation.FrameWidth * 0.35);
            int left = (idleAnimation.FrameWidth - width) / 2;
            int height = (int)(idleAnimation.FrameWidth * 0.7);
            int top = idleAnimation.FrameHeight - height;
            localBounds = new Rectangle(left, top, width, height);
        }

        /// <summary>
        /// Draws the animated enemy.
        /// </summary>
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            // Stop running when the game is paused or before turning around.
            if (!IsAlive)
            {
                sprite.PlayAnimation(dieAnimation);
            }
            else
                sprite.PlayAnimation(idleAnimation);


            // Draw facing the way the enemy is moving.
            sprite.Draw(gameTime, spriteBatch, Position, SpriteEffects.None);
        }
        public void OnKilled(Player killedBy)
        {
            IsAlive = false;
            killedSound.Play();
        }

    }
}