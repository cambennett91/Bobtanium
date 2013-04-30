#region File Description
//-----------------------------------------------------------------------------
// Player.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
#region include libraries
using System;
using System.IO;
using System.Xml;
using System.Text;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Media;
#endregion

namespace Platformer
{
    /// <summary>
    /// Our fearless adventurer!
    /// </summary>
    class Player
    {
        public ElementList list = new ElementList();
        public int listIndex = 0; // Zero indexing
        // Start position
        public Vector2 start_position;
        // Animations
        private Animation idleAnimation;
        private Animation runAnimation;
        private Animation jumpAnimation;
        private Animation celebrateAnimation;
        private Animation dieAnimation;
        private Animation morphAnimation;
        private SpriteEffects flip = SpriteEffects.None;
        private AnimationPlayer sprite;
        // Sounds
        private SoundEffect MovementNormal, MovementCarbon, MovementIron, MovementOxygen, MovementHelium, MovementLN;
        private SoundEffect JumpNormal, JumpCarbon, JumpIron, JumpOxygen, JumpHelium, JumpLN;
        public SoundEffect ElementChange, ExitReached;
        private SoundEffect Death, DeathFire, DeathFireGas, DeathIce, DeathSpikes, DeathWater, DeathFall;
        
        // Return the current level the player is on
        public Level Level { get { return level; } }
        Level level;
        // Return whether or not we are dead
        public bool IsAlive { get { return isAlive; } }
        bool isAlive;

        // Get/Set Player character position
        public Vector2 Position {
            get { return position; }
            set { position = value; }
        }
        Vector2 position;

        // Use this to check if the bottom of the character is on the ground
        private float previousBottom;

        // Get/Set player character velocity
        public Vector2 Velocity {
            get { return velocity; }
            set { velocity = value; }
        }
        Vector2 velocity;

        // Elapsed time from last poll
        public float elapsed;

        // Constants for controling horizontal movement
        private const float MoveAcceleration = 13000.0f;
        private const float MaxMoveSpeed = 1750.0f;
        private const float GroundDragFactor = 0.48f;
        private const float AirDragFactor = 0.58f;

        // Constants for controlling vertical movement
        private const float MaxJumpTime = 0.35f;
        private const float JumpLaunchVelocity = -3500.0f;
        private const float GravityAcceleration = 3400.0f;
        private const float MaxFallSpeed = 550.0f;
        private const float JumpControlPower = 0.14f; 

        // Input configuration
        private const float MoveStickScale = 1.0f;
        private const float AccelerometerScale = 1.5f;
        private const Buttons JumpButton = Buttons.A;

        // Gets whether or not the player's feet are on the ground.
        public bool IsOnGround
        {
            get { return isOnGround; }
        }
        bool isOnGround;

        // Current user movement input.
        private float movement;

        // Jumping state
        private bool isJumping;
        private bool wasJumping;
        private float jumpTime;

        // Textures for the "new element unlocked" overlays
        public int OverlayTimeMax = 10;
        public TimeSpan OverlayTime; //Length of time overlay is up for
        public Texture2D status = null;
        public Texture2D Unlocked1Overlay;
        public Texture2D Unlocked2Overlay;
        public Texture2D Unlocked3Overlay;
        public Texture2D Unlocked4Overlay;
        public Texture2D Unlocked5Overlay;
        public Texture2D Unlocked6Overlay;

        private Rectangle localBounds;
        // Gets a rectangle which bounds this player in world space
        public Rectangle BoundingRectangle {
            get
            {
                int left = (int)Math.Round(Position.X - sprite.Origin.X) + localBounds.X;
                int top = (int)Math.Round(Position.Y - sprite.Origin.Y) + localBounds.Y;

                return new Rectangle(left, top, localBounds.Width, localBounds.Height);
            }
        }
        
        // Constructors a new player.
        public Player(Level level, Vector2 position)
        {
            this.level = level;
            OverlayTime = TimeSpan.FromSeconds(OverlayTimeMax);
            LoadContent();
            LoadCharacters();
            start_position = position;
            Reset(position);
        }

        // Loads the player sprite sheet and sounds.
        public void LoadContent()
        {
            // Load Animations.
            LoadAnimations();

            // Calculate bounds within texture size.            
            int width = (int)(idleAnimation.FrameWidth * 0.4);
            int left = (idleAnimation.FrameWidth - width) / 2;
            int height = (int)(idleAnimation.FrameWidth * 0.8);
            int top = idleAnimation.FrameHeight - height;
            localBounds = new Rectangle(left, top, width, height);

            // Load sounds.
            LoadSounds();

            Unlocked1Overlay = level.Content.Load<Texture2D>("Overlays/Unlocked/Unlock2");
            Unlocked2Overlay = level.Content.Load<Texture2D>("Overlays/Unlocked/Unlock3");
            Unlocked3Overlay = level.Content.Load<Texture2D>("Overlays/Unlocked/Unlock4");
            Unlocked4Overlay = level.Content.Load<Texture2D>("Overlays/Unlocked/Unlock5");
            Unlocked5Overlay = level.Content.Load<Texture2D>("Overlays/Unlocked/Unlock6");
        }

        /// <summary>
        /// Resets the player to life.
        /// </summary>
        /// <param name="position">The position to come to life at.</param>
        public void Reset(Vector2 position)
        {
            Position = position;
            Velocity = Vector2.Zero;
            isAlive = true;
            sprite.PlayAnimation(idleAnimation);
            DoTransform(0);
        }

        /// <summary>
        /// Handles input, performs physics, and animates the player sprite.
        /// </summary>
        /// <remarks>
        /// We pass in all of the input states so that our game is only polling the hardware
        /// once per frame. We also pass the game's orientation because when using the accelerometer,
        /// we need to reverse our motion when the orientation is in the LandscapeRight orientation.
        /// </remarks>
        public void Update(
            GameTime gameTime, 
            KeyboardState keyboardState, 
            GamePadState gamePadState, 
            TouchCollection touchState, 
            AccelerometerState accelState,
            DisplayOrientation orientation)
        {
            GetInput(keyboardState, gamePadState, touchState, accelState, orientation);

            OverlayTime -= gameTime.ElapsedGameTime;

            // Clamp the time remaining at zero.
            if (OverlayTime < TimeSpan.Zero)
                OverlayTime = TimeSpan.Zero;

            ApplyPhysics(gameTime);

            if (IsAlive)
            {
                if (IsOnGround)
                {
                    if (Math.Abs(Velocity.X) - 0.02f > 0)
                    {
                        
                        sprite.PlayAnimation(runAnimation);
                    }
                    else
                    {
                        sprite.PlayAnimation(idleAnimation);
                    }
                }
                else
                    sprite.PlayAnimation(idleAnimation);
            }

            // Clear input.
            movement = 0.0f;
            isJumping = false;
        }

        /// <summary>
        /// Gets player horizontal movement and jump commands from input.
        /// </summary>
        private void GetInput(
            KeyboardState keyboardState, 
            GamePadState gamePadState, 
            TouchCollection touchState,
            AccelerometerState accelState, 
            DisplayOrientation orientation)
        {
            // Get analog horizontal movement.
            movement = gamePadState.ThumbSticks.Left.X * MoveStickScale;

            // Ignore small movements to prevent running in place.
            if (Math.Abs(movement) < 0.5f)
                movement = 0.0f;

            // Move the player with accelerometer
            if (Math.Abs(accelState.Acceleration.Y) > 0.10f)
            {
                // set our movement speed
                movement = MathHelper.Clamp(-accelState.Acceleration.Y * AccelerometerScale, -1f, 1f);

                // if we're in the LandscapeLeft orientation, we must reverse our movement
                if (orientation == DisplayOrientation.LandscapeRight)
                    movement = -movement;
            }

            // If any digital horizontal movement input is found, override the analog movement.
            if (gamePadState.IsButtonDown(Buttons.DPadLeft) ||
                keyboardState.IsKeyDown(Keys.Left) ||
                keyboardState.IsKeyDown(Keys.A))
            {
                movement = -(list.Elements[listIndex].speed);
            }
            else if (gamePadState.IsButtonDown(Buttons.DPadRight) ||
                     keyboardState.IsKeyDown(Keys.Right) ||
                     keyboardState.IsKeyDown(Keys.D))
            {
                movement = list.Elements[listIndex].speed;
            }

            // If Oxygen then UP/DOWN will move player up and down
            if (list.Elements[listIndex].name == "Oxygen")
            {
                if (gamePadState.IsButtonDown(Buttons.DPadUp) ||
                    keyboardState.IsKeyDown(Keys.Up) ||
                    keyboardState.IsKeyDown(Keys.W))
                {
                    velocity.Y += -(list.Elements[listIndex].speed * MoveAcceleration * elapsed);
                }
                else if (gamePadState.IsButtonDown(Buttons.DPadDown) ||
                         keyboardState.IsKeyDown(Keys.Down) ||
                         keyboardState.IsKeyDown(Keys.S))
                {
                    velocity.Y += list.Elements[listIndex].speed * MoveAcceleration * elapsed;
                }
            }

            // Check if the player wants to jump.
            isJumping =
                (gamePadState.IsButtonDown(JumpButton) ||
                keyboardState.IsKeyDown(Keys.Space) ||
                keyboardState.IsKeyDown(Keys.Up) ||
                keyboardState.IsKeyDown(Keys.W) ||
                touchState.AnyTouch()) && list.Elements[listIndex].name != "Oxygen";
        }

        /// <summary>
        /// Updates the player's velocity and position based on input, gravity, etc.
        /// </summary>
        public void ApplyPhysics(GameTime gameTime)
        {
            elapsed = (float)gameTime.ElapsedGameTime.TotalSeconds;

            Vector2 previousPosition = Position;

            // Base velocity is a combination of horizontal movement control and
            // acceleration downward due to gravity.
            velocity.X += movement * MoveAcceleration * elapsed;
            if (list.Elements[listIndex].name == "Helium")
                velocity.Y = MathHelper.Clamp(velocity.Y + -(GravityAcceleration) * elapsed, -MaxFallSpeed, MaxFallSpeed);
            else
                velocity.Y = MathHelper.Clamp(velocity.Y + GravityAcceleration * elapsed, -MaxFallSpeed, MaxFallSpeed);

            if (list.Elements[listIndex].name != "Oxygen")
                velocity.Y = DoJump(velocity.Y, gameTime);
            
            // Apply pseudo-drag horizontally.
            if (IsOnGround)
                velocity.X *= GroundDragFactor;
            else
                velocity.X *= AirDragFactor;

            // Prevent the player from running faster than his top speed.            
            velocity.X = MathHelper.Clamp(velocity.X, -MaxMoveSpeed, MaxMoveSpeed);
            velocity.Y = MathHelper.Clamp(velocity.Y, -MaxMoveSpeed, MaxMoveSpeed);

            // Apply velocity.
            Position += velocity * elapsed;
            Position = new Vector2((float)Math.Round(Position.X), (float)Math.Round(Position.Y));

            // If the player is now colliding with the level, separate them.
            HandleCollisions();

            // If the collision stopped us from moving, reset the velocity to zero.
            if (Position.X == previousPosition.X)
                velocity.X = 0;

            if (Position.Y == previousPosition.Y)
                velocity.Y = 0;
        }

        /// <summary>
        /// Calculates the Y velocity accounting for jumping and
        /// animates accordingly.
        /// </summary>
        /// <remarks>
        /// During the accent of a jump, the Y velocity is completely
        /// overridden by a power curve. During the decent, gravity takes
        /// over. The jump velocity is controlled by the jumpTime field
        /// which measures time into the accent of the current jump.
        /// </remarks>
        /// <param name="velocityY">
        /// The player's current velocity along the Y axis.
        /// </param>
        /// <returns>
        /// A new Y velocity if beginning or continuing a jump.
        /// Otherwise, the existing Y velocity.
        /// </returns>
        private float DoJump(float velocityY, GameTime gameTime)
        {
            // If the player wants to jump
            if (isJumping)
            {
                // Begin or continue a jump
                if ((!wasJumping && IsOnGround) || jumpTime > 0.0f)
                {

                    if (jumpTime == 0.0f && level.tempSFX == true)
                    {
                        if(list.Elements[listIndex].name.Equals("Carbon"))
                            JumpCarbon.Play();
                        else if(list.Elements[listIndex].name.Equals("Iron"))
                            JumpIron.Play();
                        else if(list.Elements[listIndex].name.Equals("Helium"))
                            JumpHelium.Play();
                        else if(list.Elements[listIndex].name.Equals("Oxygen"))
                            JumpOxygen.Play();
                        else if(list.Elements[listIndex].name.Equals("Liquid Nitrogen"))
                            JumpLN.Play();
                        else
                            JumpNormal.Play();
                    }
                    jumpTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
                    
                    sprite.PlayAnimation(jumpAnimation);
                }

                // If we are in the ascent of the jump
                if (0.0f < jumpTime && jumpTime <= MaxJumpTime)
                {
                    // Fully override the vertical velocity with a power curve that gives players more control over the top of the jump
                    velocityY = JumpLaunchVelocity * (list.Elements[listIndex].jump - (float)Math.Pow(jumpTime / MaxJumpTime, JumpControlPower));
                }
                else
                {
                    // Reached the apex of the jump
                    jumpTime = 0.0f;
                }
            }
            else
            {
                // Continues not jumping or cancels a jump in progress
                jumpTime = 0.0f;
            }
            wasJumping = isJumping;

            return velocityY;
        }

        /// <summary>
        /// Detects and resolves all collisions between the player and his neighboring
        /// tiles. When a collision is detected, the player is pushed away along one
        /// axis to prevent overlapping. There is some special logic for the Y axis to
        /// handle platforms which behave differently depending on direction of movement.
        /// </summary>
        private void HandleCollisions()
        {
            // Get the player's bounding rectangle and find neighboring tiles.
            Rectangle bounds = BoundingRectangle;
            int leftTile = (int)Math.Floor((float)bounds.Left / Tile.Width);
            int rightTile = (int)Math.Ceiling(((float)bounds.Right / Tile.Width)) - 1;
            int topTile = (int)Math.Floor((float)bounds.Top / Tile.Height);
            int bottomTile = (int)Math.Ceiling(((float)bounds.Bottom / Tile.Height)) - 1;

            // Reset flag to search for ground collision.
            isOnGround = false;

            // For each potentially colliding tile,
            for (int y = topTile; y <= bottomTile; ++y)
            {
                for (int x = leftTile; x <= rightTile; ++x)
                {
                    // If this tile is collidable,
                    TileCollision collision = Level.GetCollision(x, y);
                    if (collision == TileCollision.Platform ||
                        collision == TileCollision.Impassable ||
                        collision == TileCollision.Water ||
                        collision == TileCollision.Ice)
                    {
                        // Determine collision depth (with direction) and magnitude.
                        Rectangle tileBounds = Level.GetBounds(x, y);
                        Vector2 depth = RectangleExtensions.GetIntersectionDepth(bounds, tileBounds);
                        if (depth != Vector2.Zero)
                        {
                            float absDepthX = Math.Abs(depth.X);
                            float absDepthY = Math.Abs(depth.Y);

                            // Resolve the collision along the shallow axis.
                            if (absDepthY < absDepthX || collision == TileCollision.Platform)
                            {
                                // If we crossed the top of a tile, we are on the ground.
                                if (previousBottom <= tileBounds.Top)
                                    isOnGround = true;

                                // Ignore platforms, unless we are on the ground.
                                if (collision == TileCollision.Impassable || IsOnGround)
                                {
                                    // Resolve the collision along the Y axis.
                                    Position = new Vector2(Position.X, Position.Y + depth.Y);

                                    // Perform further collisions with the new bounds.
                                    bounds = BoundingRectangle;
                                }
                            }
                            else if (collision == TileCollision.Impassable) // Ignore platforms.
                            {
                                // Resolve the collision along the X axis.
                                Position = new Vector2(Position.X + depth.X, Position.Y);

                                // Perform further collisions with the new bounds.
                                bounds = BoundingRectangle;
                            }
                        }
                        if (collision == TileCollision.Water)
                            WaterCollision(collision, depth, tileBounds, bounds, x, y);if (collision == TileCollision.Ice)
                            IceCollision(collision, x, y);
                    }
                        if (collision == TileCollision.Spikes)
                            SpikesCollision(collision, x, y);

                        if (collision == TileCollision.Debris)
                            DebrisCollision(collision, x, y);

                        if (collision == TileCollision.Enemy)
                            EnemyCollision(collision, x, y);

                        

                        if (collision == TileCollision.Fire)
                            FireCollision(collision, x, y);                        

                        UnlockTransform(collision, x, y);
                    
                    if (collision == TileCollision.Heat)
                        HeatCollision(collision, x, y);
                }
            }

            // Save the new bounds bottom.
            previousBottom = bounds.Bottom;
        }

        /// <summary>
        /// Called when the player has been killed.
        /// </summary>
        /// <param name="killedBy">
        /// The enemy who killed the player. This parameter is null if the player was
        /// not killed by an enemy (fell into a hole).
        /// </param>
        public void OnKilled(TileCollision killedBy)
        {
            isAlive = false;
            if (level.tempSFX == true)
            {
                switch (killedBy)
                {
                    case TileCollision.Fire:
                        DeathFire.Play(); break;
                    case TileCollision.Heat:
                        DeathFireGas.Play(); break;
                    case TileCollision.Ice:
                        DeathIce.Play(); break;
                    case TileCollision.Spikes:
                        DeathSpikes.Play(); break;
                    case TileCollision.Water:
                        DeathWater.Play(); break;
                    default:
                        Death.Play(); // fall sound
                        break;
                }
            }
            sprite.PlayAnimation(dieAnimation);
        }

        /// <summary>
        /// Called when this player reaches the level's exit.
        /// </summary>
        public void OnReachedExit()
        {
            sprite.PlayAnimation(celebrateAnimation);
        }

        /// <summary>
        /// Draws the animated player.
        /// </summary>
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            // Flip the sprite to face the way we are moving.
            if (Velocity.X < 0)
                flip = SpriteEffects.FlipHorizontally;
            else if (Velocity.X > 0)
                flip = SpriteEffects.None;          
            
            // Draw that sprite.
            sprite.Draw(gameTime, spriteBatch, Position, flip, Color.White);
        }

        public void LoadAnimations()
        {
            // Load animated textures.
            idleAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/Player/Idle"), 0.1f, true);
            runAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/Player/Run"), 0.1f, true);
            jumpAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/Player/Jump"), 0.1f, false);
            celebrateAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/Player/Celebrate"), 0.1f, false);
            dieAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/Player/Die"), 0.1f, false);
            morphAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/Player/Transform"), 0.1f, false);
        }

        public void LoadSounds()
        {
            
            MovementNormal = Level.Content.Load<SoundEffect>("Sounds/Player/MovementNormal");
            MovementCarbon = Level.Content.Load<SoundEffect>("Sounds/Player/MovementCarbon");
            MovementIron = Level.Content.Load<SoundEffect>("Sounds/Player/MovementIron");
            MovementOxygen = Level.Content.Load<SoundEffect>("Sounds/Player/MovementGas");
            MovementHelium = Level.Content.Load<SoundEffect>("Sounds/Player/MovementGas");
            MovementLN = Level.Content.Load<SoundEffect>("Sounds/Player/MovementLN");

            JumpNormal = Level.Content.Load<SoundEffect>("Sounds/Player/JumpNormal");
            JumpCarbon = Level.Content.Load<SoundEffect>("Sounds/Player/JumpCarbon");
            JumpIron = Level.Content.Load<SoundEffect>("Sounds/Player/JumpIron");
            JumpOxygen = Level.Content.Load<SoundEffect>("Sounds/Player/JumpOxygen");
            JumpHelium = Level.Content.Load<SoundEffect>("Sounds/Player/JumpHelium");
            JumpLN = Level.Content.Load<SoundEffect>("Sounds/Player/JumpLN");

            Death = Level.Content.Load<SoundEffect>("Sounds/Player/Death");
            DeathFire = Level.Content.Load<SoundEffect>("Sounds/Player/DeathFire");
            DeathFireGas = Level.Content.Load<SoundEffect>("Sounds/Player/DeathFireGas");
            DeathIce = Level.Content.Load<SoundEffect>("Sounds/Player/DeathIce");
            DeathSpikes = Level.Content.Load<SoundEffect>("Sounds/Player/DeathSpikes");
            DeathWater = Level.Content.Load<SoundEffect>("Sounds/Player/DeathWater");
            DeathFall = Level.Content.Load<SoundEffect>("Sounds/Player/DeathFall");
            
            ElementChange = Level.Content.Load<SoundEffect>("Sounds/Player/ElementChange");
            ExitReached = Level.Content.Load<SoundEffect>("Sounds/Player/ExitReached");
            
        }

        public void LoadCharacters()
        {
            list = list.loadDefinitions();
            for (int i = 0; i < list.Elements.Count; i++)
            {
                if (this.Level.mlevelIndex > list.Elements[i].level)
                    list.Elements[i].unlocked = true;
            }

            #region Creating Demo
            //////////////////////////////////////////////////////////////////////////////////////
            // http://www.neowin.net/forum/topic/874496-c%23-xna-xml-reading/page__p__592227370 //
            //////////////////////////////////////////////////////////////////////////////////////

            // Already done above!
            // var definitions = new Definitions();
            //ElementList list = new ElementList();

            //definitions.ItemDefinitions = new List<ItemDefinition>();
            //list.Elements = new List<ElementDefinition>();
            
            //definitions.ItemDefinitions.Add(new ItemDefinition() { Type = "Big****ingSword", Value = 10000 });
            /*list.Elements.Add(new ElementDefinition() { Speed = 1.4f, JumpHeight = 1.2f });

            list.Elements.Add(new ElementDefinition() { Speed = 2f, JumpHeight = 2f });
            list.Elements.Add(new ElementDefinition() { Speed = 1.1f, JumpHeight = 1.2f });
            list.Elements.Add(new ElementDefinition() { Speed = 0.9f, JumpHeight = 0.7f });
            list.Elements.Add(new ElementDefinition() { Speed = 1f, JumpHeight = 0.9f });
            */
            
            // Generate Definitions.xml
            /*using (var fileStream = new FileStream("Content/ElementList.xml", FileMode.Open))
            {
                var xmlSerializer = new XmlSerializer(typeof(ElementList));
                xmlSerializer.Serialize(fileStream, list);
            }
            */
            #endregion
        } //LoadCharacters()

        public void DoTransform(int collision)
        {
            // if the player isnt changing into the same element
            if (collision.Equals(listIndex) == false)
            {
                if (collision == 0)
                {
                    idleAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/Player/Idle"), 0.1f, true);
                    runAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/Player/Run"), 0.1f, true);
                    jumpAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/Player/Jump"), 0.1f, false);
                    celebrateAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/Player/Celebrate"), 0.1f, false);
                    listIndex = 0;
                }
                if (collision >= 1 && collision <= 9)
                {
                    idleAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/Player/" + collision + "/Idle"), 0.1f, true);
                    runAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/Player/" + collision + "/Run"), 0.1f, true);
                    jumpAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/Player/" + collision + "/Jump"), 0.1f, false);
                    celebrateAnimation = new Animation(Level.Content.Load<Texture2D>("Sprites/Player/" + collision + "/Celebrate"), 0.1f, false);
                    listIndex = collision;
                }
                if (!ElementChange.Duration.Equals(TimeSpan.Zero))
                    ElementChange.Play();
            }
        } //DoTransform()

        public void UnlockTransform(TileCollision collision, int x, int y)
        {
            if (collision == TileCollision.Transform1)
            {
                for (int i = 0; i < level.transforms.Count; i++)
                {
                    if (level.transforms[i].collisiontype == TileCollision.Transform1)
                    {
                        status = Unlocked1Overlay;
                        OverlayTime = TimeSpan.FromSeconds(OverlayTimeMax);

                        level.LoadNewTileAt(null, TileCollision.Passable, x, y);

                        // Unlock for player use
                        level.Player.list.Elements[1].unlocked = true;
                        level.transforms[i].collected = true;
                        
                        level.transforms.RemoveAt(i);
                    }
                }
            }
            if (collision == TileCollision.Transform2)
            {
                for (int i = 0; i < level.transforms.Count; i++)
                {
                    if (level.transforms[i].collisiontype == TileCollision.Transform2)
                    {
                        status = Unlocked2Overlay;
                        OverlayTime = TimeSpan.FromSeconds(OverlayTimeMax);

                        level.LoadNewTileAt(null, TileCollision.Passable, x, y);

                        // Unlock for player use
                        level.Player.list.Elements[2].unlocked = true;
                        level.transforms[i].collected = true;
                        
                        level.transforms.RemoveAt(i);
                    }
                }
            }
            if (collision == TileCollision.Transform3)
            {
                for (int i = 0; i < level.transforms.Count; i++)
                {
                    if (level.transforms[i].collisiontype == TileCollision.Transform3)
                    {
                        status = Unlocked3Overlay;
                        OverlayTime = TimeSpan.FromSeconds(OverlayTimeMax);

                        level.LoadNewTileAt(null, TileCollision.Passable, x, y);

                        // Unlock for player use
                        level.Player.list.Elements[3].unlocked = true;
                        level.transforms[i].collected = true;
                        
                        level.transforms.RemoveAt(i);
                    }
                }
            }
            if (collision == TileCollision.Transform4)
            {
                for (int i = 0; i < level.transforms.Count; i++)
                {
                    if (level.transforms[i].collisiontype == TileCollision.Transform4)
                    {
                        status = Unlocked4Overlay;
                        OverlayTime = TimeSpan.FromSeconds(OverlayTimeMax);

                        level.LoadNewTileAt(null, TileCollision.Passable, x, y);

                        // Unlock for player use
                        level.Player.list.Elements[4].unlocked = true;
                        level.transforms[i].collected = true;
                        
                        level.transforms.RemoveAt(i);
                    }
                }
            }
            if (collision == TileCollision.Transform5)
            {
                for (int i = 0; i < level.transforms.Count; i++)
                {
                    if (level.transforms[i].collisiontype == TileCollision.Transform5)
                    {
                        status = Unlocked5Overlay;
                        OverlayTime = TimeSpan.FromSeconds(OverlayTimeMax);

                        level.LoadNewTileAt(null, TileCollision.Passable, x, y);

                        // Unlock for player use
                        level.Player.list.Elements[5].unlocked = true;
                        level.transforms[i].collected = true;
                        level.transforms.RemoveAt(i);
                    }
                }
            }
            if (collision == TileCollision.Transform6)
            {
                for (int i = 0; i < level.transforms.Count; i++)
                {
                    if (level.transforms[i].collisiontype == TileCollision.Transform6)
                    {
                        status = Unlocked6Overlay;
                        OverlayTime = TimeSpan.FromSeconds(OverlayTimeMax);

                        level.LoadNewTileAt(null, TileCollision.Passable, x, y);

                        // Unlock for player use
                        level.Player.list.Elements[6].unlocked = true;
                        level.transforms[i].collected = true;

                        level.transforms.RemoveAt(i);
                    }
                }
            }
        } //CheckTransform()

        public void SpikesCollision(TileCollision collision, int x, int y)
        {
            //Reset(start_position);
            OnKilled(collision);
        }

        public void DebrisCollision(TileCollision collision, int x, int y)
        {
            if (list.Elements[listIndex].name == "Nitrogen" ^ list.Elements[listIndex].name == "Helium" ^ list.Elements[listIndex].name == "Oxygen")
            {
                
            }

            else
                OnKilled(collision);
        }

        public void EnemyCollision(TileCollision collision, int x, int y)
        {
            OnKilled(collision);
        } //EnemyCollision()

        public void WaterCollision(TileCollision collision, Vector2 depth, Rectangle tileBounds, Rectangle bounds, int x, int y)
        {
            /// <summary>
            /// Water kills player unless playing as carbon, liquid nitrogen
            /// carbon acts as a platform, liquid nitrogen causes water to freeze creating ice
            /// </summary>
            // TO DO: conditions of current element!

            // if playing as carbon, treat water as a platform.
            // turn this action into a function.
            if (list.Elements[listIndex].floats == true || list.Elements[listIndex].freezes == true)
            {
                if (list.Elements[listIndex].floats == true)
                {
                    Position = new Vector2(Position.X, Position.Y);
                    // Perform further collisions with the new bounds.
                    bounds = BoundingRectangle;
                }

                if (list.Elements[listIndex].freezes == true)
                {
                    // Liquid Nitrogen hits the water, make the water ice
                    //for(int i=0;i<level.animatedtiles.Count;i++)
                    //    if (level.animatedtiles[i].getPosition().Equals(new Vector2(x, y)))
                    //    {
                    //        level.animatedtiles[i].stopAnimation();
                    //        Level.LoadNewTileAt("Tiles/Hazards/Ice", TileCollision.Ice, x, y);
                    //    }
                }
            }
            else
                OnKilled(collision);
        } //WaterCollision()

        public void FireCollision(TileCollision collision, int x, int y)
        {
            if (list.Elements[listIndex].flamable == false || list.Elements[listIndex].freezes == true)
            {
                if (list.Elements[listIndex].freezes == true)
                {
                    //Liquid Nitrogen turns Fire into Water into Ice
                    level.LoadNewTileAt(null, TileCollision.Passable, x, y);
                }
            }
            else
                OnKilled(collision);
        } //FireCollision()

        public void HeatCollision(TileCollision collision, int x, int y)
        {
            if (list.Elements[listIndex].gas == true)
            {
                OnKilled(collision);
            }
            else
            { }
        }

        public void IceCollision(TileCollision collision, int x, int y)
        {
            /// <summary> 
            /// Ice kills the player on contact unless playing as a gas (oxygen, helium, Liquid nitrogen).
            /// slows down water, mercury, gold.
            /// passable 
            /// </summary>
            if (list.Elements[listIndex].freezes == true)
            {
                Position = new Vector2(Position.X, Position.Y);
            }
            // Kill everything else
            else
                OnKilled(collision);
        } //IceCollision()
    }
}
