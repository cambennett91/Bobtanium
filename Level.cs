#region File Description
//-----------------------------------------------------------------------------
// Level.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

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
    /// <summary>
    /// A uniform grid of tiles with collections of gems and enemies.
    /// The level owns the player and controls the game's win and lose
    /// conditions as well as scoring.
    /// </summary>
    class Level : IDisposable
    {
        public SpriteBatch mspriteBatch;

        private int mMenuMain;
        private int mMenuHelp;
        private int mMenuSettings;
        private int mMenuPlay;
        private int mw1l1, mw2l1, mw3l1, mw4l1, mw5l1, mhighestLevel, mnumberOfLevels;
        private bool mw2unlocked = false;
        private bool mw3unlocked = false;
        private bool mw4unlocked = false;
        private bool mw5unlocked = false;
        private int inWorld;

        public List<MenuItem> MenuItemList = new List<MenuItem>();
        public MenuItem GetMenuItem(int i){return MenuItemList[i]; }
        public void SetMenuItem(MenuItem item, int i) { MenuItemList[i] = item; }
        private List<AnimatedTile> animatedtiles = new List<AnimatedTile>();
        public List<Catalyst> transforms = new List<Catalyst>();
        public Catalyst GetTransform(int i) { return transforms[i]; }
        public void SetTransform(Catalyst item, int i) { transforms[i] = item; }
        private List<Enemy> enemies = new List<Enemy>();

        public Rectangle MouseBox;
        public MouseState mouse;

        private GraphicsDeviceManager graphics;
        public bool tempfullscreen;
        public bool tempmusic;
        public bool tempSFX;

        public Rectangle NewGameButtonBox, ExitButtonBox, HelpButtonBox, SettingsButtonBox, AcceptButtonBox, BackButtonBox, NextArrowBox, BackArrowBox, FullScreenBox, MusicBox, SFXBox;
        public Rectangle NewGameOuterBox, ExitOuterBox, HelpOuterBox, SettingsOuterBox, AcceptOuterBox, BackOuterBox;
        public Rectangle World2Box, World3Box, World4Box, World5Box, World2OuterBox, World3OuterBox, World4OuterBox, World5OuterBox;

        public Point HelpTextPosition = new Point();
        public String HelpText1 = "Controls:\nLeft = A, Left Arrow\nRight = D, Right Arrow\nJump = W, Space, Up Arrow\nTransform = 0-9\n(IF element is unlocked)\nMain Menu = Escape Key\n\nWe hope you like it!\n\nSincerly,\nDev Team";
        public String HelpText2 = "Transforming:\nBy collecting little transforming agents you unlock the\nability to change into a different element that grants\nyou different abilities.";
        public String HelpText3 = "";
        public List<String> HelpTextList = new List<string>();
        public int HelpTextListIndex = 0;
        public SpriteFont HelpTextFont;

        public int mlevelIndex;

        // Physical structure of the level.
        public Tile[,] tiles;
        private Layer[] layers;
        // The layer which entities are drawn on top of.
        private const int EntityLayer = 2;

        //Level Game State
        private float cameraPositionXaxis;
        private float cameraPositionYaxis;

        // Entities in the level.
        public Player Player
        {
            get { return player; }
        }
        Player player;

        // Key locations in the level.        
        private Vector2 start;
        private Point exit = InvalidPosition;
        private static readonly Point InvalidPosition = new Point(-1, -1);

        //Camera Variables
        public float marginWidth;
        public float marginLeft;
        public float marginRight;
        public float TopMargin;
        public float BottomMargin;


        // Level game state.
        private Random random = new Random(354668); // Arbitrary, but constant seed, 354668

        public int Score
        {
            get { return score; }
        }
        int score;

        public bool ReachedExit
        {
            get { return reachedExit; }
        }
        bool reachedExit;

        public TimeSpan TimeRemaining
        {
            get { return timeRemaining; }
        }
        TimeSpan timeRemaining;

        private const int PointsPerSecond = 1;

        // Level content.        
        public ContentManager Content
        {
            get { return content; }
        }
        ContentManager content;

        private SoundEffect exitReachedSound;

        #region Loading

        /// <summary>
        /// Constructs a new level.
        /// </summary>
        /// <param name="serviceProvider">
        /// The service provider that will be used to construct a ContentManager.
        /// </param>
        /// <param name="fileStream">
        /// A stream containing the tile data.
        /// </param>
        public Level(IServiceProvider serviceProvider, Stream fileStream, int levelIndex,
                     GraphicsDeviceManager graphics, bool music, bool sfx, int MenuMain, int MenuHelp, int MenuSettings, int MenuPlay,
                     int highestLevel, int inWorld, int w1l1, int w2l1, int w3l1, int w4l1, int w5l1, int numberoflevels, bool w2unlocked, bool w3unlocked, bool w4unlocked, bool w5unlocked)
        {
            // Create a new content manager to load content used just by this level.
            content = new ContentManager(serviceProvider, "Content");

            timeRemaining = TimeSpan.FromSeconds(301);
            //splashScreenTimer = TimeSpan.FromSeconds(4);

            this.graphics = graphics;
            tempmusic = music;
            tempSFX = sfx;

            mMenuMain = MenuMain;
            mMenuHelp = MenuHelp;
            mMenuSettings = MenuSettings;
            mMenuPlay = MenuPlay;
            mhighestLevel = highestLevel;
            mw1l1 = w1l1;
            mw2l1 = w2l1;
            mw3l1 = w3l1;
            mw4l1 = w4l1;
            mw5l1 = w5l1;
            mnumberOfLevels = numberoflevels;
            mw2unlocked = w2unlocked;
            mw3unlocked = w3unlocked;
            mw4unlocked = w4unlocked;
            mw5unlocked = w5unlocked;
            
            mlevelIndex = levelIndex;
            this.inWorld = inWorld;
            
            LoadTiles(fileStream);

            // Load background layer textures. For now, all levels must
            // use the same backgrounds and only use the left-most part of them.
            layers = new Layer[3];
            if (mlevelIndex < mw1l1)
            {
                layers[0] = new Layer(Content, "Backgrounds/Menu/Layer0", 0.2f);
                layers[1] = new Layer(Content, "Backgrounds/Menu/Layer1", 0.5f);
                layers[2] = new Layer(Content, "Backgrounds/Menu/Layer2", 0.8f);
            }
            else
            {
                layers[0] = new Layer(Content, "Backgrounds/World" + inWorld+ "/Layer0", 0.2f);
                layers[1] = new Layer(Content, "Backgrounds/World" + inWorld+ "/Layer1", 0.5f);
                layers[2] = new Layer(Content, "Backgrounds/World" + inWorld+ "/Layer2", 0.8f);
            }

            // Load sounds.
            exitReachedSound = Content.Load<SoundEffect>("Sounds/Player/ExitReached");
            HelpTextFont = Content.Load<SpriteFont>("Fonts/HelpTextFont");

            HelpTextList.Add(HelpText1);
            HelpTextList.Add(HelpText2);
            HelpTextList.Add(HelpText3);

        }

        /// <summary>
        /// Iterates over every tile in the structure file and loads its
        /// appearance and behavior. This method also validates that the
        /// file is well-formed with a player start point, exit, etc.
        /// </summary>
        /// <param name="fileStream">
        /// A stream containing the tile data.
        /// </param>
        private void LoadTiles(Stream fileStream)
        {
            // Load the level and ensure all of the lines are the same length.
            int width;
            List<string> lines = new List<string>();
            using (StreamReader reader = new StreamReader(fileStream))
            {
                string line = reader.ReadLine();
                width = line.Length; // the length of a line
                while (line != null)    // while there are still lines left
                {
                    lines.Add(line); // add current line to list of lines
                    if (line.Length != width) // if line is not the right size, throw exception
                        throw new Exception(String.Format("The length of line {0} is different from all preceeding lines.", lines.Count));
                    line = reader.ReadLine(); //else read the line
                } // loop reading the lines
            }

            // Allocate the tile grid.
            tiles = new Tile[width, lines.Count]; // make a new tile grid as wide as a line, and as long as the list

            // Loop over every tile position,
            for (int y = 0; y < Height; ++y) // while y < the amount of lines
            {
                for (int x = 0; x < Width; ++x) // while x < the width of a line
                {
                    // to load each tile.
                    char tileType = lines[y][x];
                    tiles[x, y] = LoadTile(tileType, x, y, width);
                }
            }

            // Verify that the level has a beginning and an end.
            if (Player == null)
                throw new NotSupportedException("A level must have a starting point.");
            //if (exit == InvalidPosition)
            //    throw new NotSupportedException("A level must have an exit.");

        }

        /// <summary>
        /// Loads an individual tile's appearance and behavior.
        /// </summary>
        /// <param name="tileType">
        /// The character loaded from the structure file which
        /// indicates what should be loaded.
        /// </param>
        /// <param name="x">
        /// The X location of this tile in tile space.
        /// </param>
        /// <param name="y">
        /// The Y location of this tile in tile space.
        /// </param>
        /// <returns>The loaded tile.</returns>
        private Tile LoadTile(char tileType, int x, int y, int width)
        {
             switch (tileType)
             {
                #region Standard Tiles
                 // Blank space
                 case '.':
                     return new Tile(null, TileCollision.Passable, "");

                // Exit
                case 'X':
                    return LoadExitTile(x, y);

                // Platform block
                case '~':
                    if (inWorld > 0)
                        return LoadTile("Block_Platform_" + inWorld, TileCollision.Platform);
                 else
                        return LoadTile("Block_Platform_Default", TileCollision.Platform);

                // Impassable block
                case '#':
                    if (inWorld > 0)
                        return LoadTile("Block_Wall_" + inWorld, TileCollision.Impassable);
                    else
                        return LoadTile("Block_Wall_Default", TileCollision.Impassable);
                 #endregion

                #region Enemies/Hazards
                // Spikes of all shapes and sizes!
                case 's':
                    if (x == 0 || tiles[x - 1, y].Collision == TileCollision.Impassable)
                        return LoadTile("Hazards/Spikes/" + inWorld + "/RIGHT", TileCollision.Spikes);

                    if (x == width - 1 ) // The right screen boundary
                        return LoadTile("Hazards/Spikes/" + inWorld + "/LEFT", TileCollision.Spikes);

                    if (y == 0 || tiles[x, y - 1].Collision != TileCollision.Passable)
                        return LoadTile("Hazards/Spikes/" + inWorld + "/DOWN", TileCollision.Spikes);

                    else
                        return LoadTile("Hazards/Spikes/" + inWorld + "/UP", TileCollision.Spikes);

                 case 'S':
                    return LoadTile("Hazards/Spikes/" + inWorld + "/LEFT", TileCollision.Spikes);

                // Fire
                case 'F':
                    return LoadAnimatedTile("fire", TileCollision.Fire, x, y);

                case 'f':
                    return LoadAnimatedTile("heat", TileCollision.Heat, x, y);

                // Ice
                case 'I':
                     return LoadTile("Hazards/Ice", TileCollision.Ice);

                // Water Types
                case 'w':
                     if (tiles[x, y - 1].Collision == TileCollision.Water)
                         return LoadTile("Hazards/WaterBody", TileCollision.Water);
                     else
                       return LoadAnimatedTile("WaterClear", TileCollision.Water, x, y);

                // Debris Types
                case 'D':
                     return LoadAnimatedTile("DebrisRock", TileCollision.Debris, x, y);

                case 'd':
                    return LoadAnimatedTile("DebrisMetal", TileCollision.Debris, x, y);

                case 'i':
                     return LoadAnimatedTile("DebrisIce", TileCollision.Debris, x , y);

                // Enemies
                case 'G':
                    return LoadEnemyTile(x, y, "Green");

                case 'Y':
                    return LoadEnemyTile(x, y, "Yellow");

                #endregion

                #region Player Start/transforming items
                // Player 1 start point
                case '0':
                    return LoadStartTile(x, y);

                case '1':
                    if (mlevelIndex < mw1l1)
                        return LoadMenuTile("NewGameButton", TileCollision.Passable, x, y);
                    else
                    LoadTransformTile("Transform/IconCarbon", x, y, TileCollision.Transform1);
                    return new Tile(null, TileCollision.Transform1, "Transform" + tileType);
                case '2':
                    if (mlevelIndex < mw1l1)
                        return LoadMenuTile("2", TileCollision.Passable, x, y);
                    else
                        LoadTransformTile("Transform/IconIron", x, y, TileCollision.Transform2);
                    return new Tile(null, TileCollision.Transform2, "Transform" + tileType);
                case '3':
                    if (mlevelIndex < mw1l1)
                        return LoadMenuTile("3", TileCollision.Passable, x, y);
                    else
                        LoadTransformTile("Transform/IconHelium", x, y, TileCollision.Transform3);
                    return new Tile(null, TileCollision.Transform3, "Transform" + tileType);
                case '4':
                    if (mlevelIndex < mw1l1)
                        return LoadMenuTile("4", TileCollision.Passable, x, y);
                    else
                        LoadTransformTile("Transform/IconOxygen", x, y, TileCollision.Transform4);
                    return new Tile(null, TileCollision.Transform4, "Transform" + tileType);
                case '5':
                    if (mlevelIndex < mw1l1)
                        return LoadMenuTile("5", TileCollision.Passable, x, y);
                    else
                        LoadTransformTile("Transform/IconLN", x, y, TileCollision.Transform5);
                    return new Tile(null, TileCollision.Transform5, "Transform" + tileType);
                #endregion

                #region Main Menu
                case '[':
                    return LoadMenuTile("NewGameButton", TileCollision.Passable, x, y);

                case ']':
                    return LoadMenuTile("ExitButton", TileCollision.Passable, x, y);

                case '{':
                    return LoadMenuTile("SettingsButton", TileCollision.Passable, x, y);

                case '}':
                    return LoadMenuTile("HelpButton", TileCollision.Passable, x, y);

                case '"':
                    return LoadMenuTile("SpeechBubble", TileCollision.Passable, x, y);

                case '\\':
                    return LoadMenuTile("BackButton", TileCollision.Passable, x, y);

                case '/':
                    return LoadMenuTile("AcceptButton", TileCollision.Passable, x, y);
                #endregion

                #region Settings Menu
                case '^':
                     if(mlevelIndex < mw1l1)
                        return LoadMenuTile("FullScreenOff", TileCollision.Passable, x, y);
                     else
                         return LoadTile("Arrows/UP", TileCollision.Passable);

                case '+':
                    return LoadMenuTile("MusicOn", TileCollision.Passable, x, y);
                case '-':
                    return LoadMenuTile("SFXOn", TileCollision.Passable, x, y);
                case '>':
                     if(mlevelIndex < mw1l1)
                         return LoadMenuTile("NextArrow", TileCollision.Passable, x, y);
                     else
                         return LoadTile("Arrows/RIGHT", TileCollision.Passable);
                case '<':
                     if(mlevelIndex < mw1l1)
                        return LoadMenuTile("BackArrow", TileCollision.Passable, x, y);
                     else
                         return LoadTile("Arrows/LEFT", TileCollision.Passable);
                #endregion
                
                #region Arrows
                // "Settings Menu"
                // LEFT <
                // RIGHT >
                // UP ^
                case 'V':
                    return LoadTile("Arrows/DOWN", TileCollision.Passable);
                case 'r':
                    return LoadTile("Arrows/DOWNRIGHT", TileCollision.Passable);
                case 'l':
                    return LoadTile("Arrows/DOWNLEFT", TileCollision.Passable);
                case 'R':
                    return LoadTile("Arrows/UPRIGHT", TileCollision.Passable);
                case 'L':
                    return LoadTile("Arrows/UPLEFT", TileCollision.Passable);
                #endregion
                
                // Unknown tile type character
                default:
                    throw new NotSupportedException(String.Format("Unsupported tile type character '{0}' at position {1}, {2}.", tileType, x, y));
            }
        }


        private Tile LoadAnimatedTile(string name, TileCollision collision, int x, int y)
        {
            Vector2 position = new Vector2(x, y);
            animatedtiles.Add(new AnimatedTile(this, position, name));
            return new Tile(null, collision, name);
        }

        /// <summary>
        /// Creates a new tile. The other tile loading methods typically chain to this
        /// method after performing their special logic.
        /// </summary>
        /// <param name="name">
        /// Path to a tile texture relative to the Content/Tiles directory.
        /// </param>
        /// <param name="collision">
        /// The tile collision type for the new tile.
        /// </param>
        /// <returns>The new tile.</returns>
        private Tile LoadTile(string name, TileCollision collision)
        {
            return new Tile(Content.Load<Texture2D>("Tiles/" + name), collision, "Tiles/" + name);
        }

        private void LoadTransformTile(string name, int x, int y, TileCollision collision)
        {
            Catalyst temp = new Catalyst();
            temp.collisiontype = collision;
            temp.position = new Point(x*Tile.Width, y*Tile.Height);
            temp.texture = Content.Load<Texture2D>("Tiles/" + name);
            transforms.Add(temp);
        }

        public void LoadNewTileAt(String name, TileCollision tile, int x, int y)
        {
            if(name == null)
                tiles[x,y] = new Tile(null, TileCollision.Passable, "");
            else
                tiles[x, y] = new Tile(Content.Load<Texture2D>(name), tile, name);
        }

        private Tile LoadMenuTile(String name, TileCollision collision, int x, int y)
        {
            switch (name)
            {
                #region Main Menu
                case "NewGameButton":
                    NewGameButtonBox = new Rectangle(x*Tile.Width, (y*Tile.Height)-(Tile.Height/2), 200,64);
                    NewGameOuterBox = new Rectangle(NewGameButtonBox.Left - (Tile.Width * 2), NewGameButtonBox.Top - (Tile.Height * 2), NewGameButtonBox.Width + (Tile.Width * 4), NewGameButtonBox.Height + (Tile.Height * 4));
                    if (mlevelIndex == mMenuPlay)
                        return new Tile(Content.Load<Texture2D>("Tiles/Buttons/MenuPlay/" + name), TileCollision.Passable, "Tiles/Buttons/MenuPlay/" + name);
                    else
                        return new Tile(Content.Load<Texture2D>("Tiles/Buttons/PlayButton"), TileCollision.Passable, "Tiles/Buttons/PlayButton");

                case "ExitButton":
                    ExitButtonBox = new Rectangle(x * Tile.Width, (y * Tile.Height) - (Tile.Height / 2), 200, 64);
                    ExitOuterBox = new Rectangle(ExitButtonBox.Left - (Tile.Width * 2), ExitButtonBox.Top - (Tile.Height * 2), ExitButtonBox.Width + (Tile.Width * 4), ExitButtonBox.Height + (Tile.Height * 4));
                    return new Tile(Content.Load<Texture2D>("Tiles/Buttons/" + name), TileCollision.Passable, "Tiles/Buttons/" + name);
                    
                case "SettingsButton":
                    SettingsButtonBox = new Rectangle(x * Tile.Width, (y * Tile.Height) - (Tile.Height / 2), 200, 64);
                    SettingsOuterBox = new Rectangle(SettingsButtonBox.Left - (Tile.Width * 2), SettingsButtonBox.Top - (Tile.Height * 2), SettingsButtonBox.Width + (Tile.Width * 4), SettingsButtonBox.Height + (Tile.Height * 4));
                    return new Tile(Content.Load<Texture2D>("Tiles/Buttons/" + name), TileCollision.Passable, "Tiles/Buttons/" + name);
                    
                case "HelpButton":
                    HelpButtonBox = new Rectangle(x * Tile.Width, (y * Tile.Height) - (Tile.Height / 2), 200, 64);
                    HelpOuterBox = new Rectangle(HelpButtonBox.Left - (Tile.Width * 2), HelpButtonBox.Top - (Tile.Height * 2), HelpButtonBox.Width + (Tile.Width * 4), HelpButtonBox.Height + (Tile.Height * 4));
                    return new Tile(Content.Load<Texture2D>("Tiles/Buttons/" + name), TileCollision.Passable, "Tiles/Buttons/" + name);
                #endregion

                #region Settings/Help
                case "SpeechBubble":
                    HelpTextPosition.X = (x * Tile.Width) - 16;
                    HelpTextPosition.Y = (y * Tile.Height) + 16;
                    return new Tile(Content.Load<Texture2D>("Tiles/Buttons/" + name), TileCollision.Passable, "Tiles/Buttons/" + name);
                    
                case "AcceptButton":
                    AcceptButtonBox = new Rectangle(x * Tile.Width, (y * Tile.Height) - (Tile.Height / 2), 200, 64);
                    AcceptOuterBox = new Rectangle(AcceptButtonBox.Left - (Tile.Width * 2), AcceptButtonBox.Top - (Tile.Height * 2), AcceptButtonBox.Width + (Tile.Width * 4), AcceptButtonBox.Height + (Tile.Height * 4));
                    return new Tile(Content.Load<Texture2D>("Tiles/Buttons/" + name), TileCollision.Passable, "Tiles/Buttons/" + name);
                    
                case "BackButton":
                    BackButtonBox = new Rectangle(x * Tile.Width, (y * Tile.Height) - (Tile.Height / 2), 200, 64);
                    BackOuterBox = new Rectangle(BackButtonBox.Left - (Tile.Width * 2), BackButtonBox.Top - (Tile.Height * 2), BackButtonBox.Width + (Tile.Width * 4), BackButtonBox.Height + (Tile.Height * 4));
                    return new Tile(Content.Load<Texture2D>("Tiles/Buttons/" + name), TileCollision.Passable, "Tiles/Buttons/" + name);
                    
                case "FullScreenOff":
                    FullScreenBox = new Rectangle(x * Tile.Width, (y * Tile.Height) - (Tile.Height / 2), 250, 19);
                    if(graphics.IsFullScreen)
                        return new Tile(Content.Load<Texture2D>("Tiles/Buttons/FullScreenOn"), TileCollision.Passable, "Tiles/Buttons/FullScreenOn");
                    else if(!graphics.IsFullScreen)
                        return new Tile(Content.Load<Texture2D>("Tiles/Buttons/FullScreenOff"), TileCollision.Passable, "Tiles/Buttons/FullScreenOff");
                    else
                        return new Tile(Content.Load<Texture2D>("Tiles/Arrows/UP"), TileCollision.Passable, "Tiles/Buttons/FullScreenOff");
                    
                case "MusicOn":
                    MusicBox = new Rectangle(x * Tile.Width, (y * Tile.Height) - (Tile.Height / 2), 200, 19);
                    if(tempmusic == true)
                        return new Tile(Content.Load<Texture2D>("Tiles/Buttons/MusicOn"), TileCollision.Passable, "Tiles/Buttons/MusicOn");
                    else
                        return new Tile(Content.Load<Texture2D>("Tiles/Buttons/MusicOff"), TileCollision.Passable, "Tiles/Buttons/MusicOff");
                    
                case "SFXOn":
                    SFXBox = new Rectangle(x * Tile.Width, (y * Tile.Height) - (Tile.Height / 2), 256, 19);
                    if(tempSFX == true)
                        return new Tile(Content.Load<Texture2D>("Tiles/Buttons/SFXOn"), TileCollision.Passable, "Tiles/Buttons/SFXOn");
                    else
                        return new Tile(Content.Load<Texture2D>("Tiles/Buttons/SFXOff"), TileCollision.Passable, "Tiles/Buttons/SFXOff");
                    
                case "NextArrow":
                    NextArrowBox = new Rectangle(x * Tile.Width, (y * Tile.Height) - (Tile.Height / 2), 40, 32);
                    return new Tile(Content.Load<Texture2D>("Tiles/Buttons/" + name), TileCollision.Passable, "Tiles/Buttons/" + name);
                    
                case "BackArrow":
                    BackArrowBox = new Rectangle(x * Tile.Width, (y * Tile.Height) - (Tile.Height / 2), 40, 32);
                    return new Tile(Content.Load<Texture2D>("Tiles/Buttons/" + name), TileCollision.Passable, "Tiles/Buttons/" + name);
                #endregion

                #region Select Worlds
                case "2":
                    World2Box = new Rectangle(x*Tile.Width, (y*Tile.Height)-(Tile.Height/2), 200,64);
                    World2OuterBox = new Rectangle(World2Box.Left - (Tile.Width * 2), World2Box.Top - (Tile.Height * 2), World2Box.Width + (Tile.Width * 4), World2Box.Height + (Tile.Height * 4));
                    if(mw2unlocked == true)
                        return new Tile(Content.Load<Texture2D>("Tiles/Buttons/MenuPlay/" + name), TileCollision.Passable, "Tiles/Buttons/MenuPlay/" + name);
                    else
                        return new Tile(Content.Load<Texture2D>("Tiles/Buttons/MenuPlay/" + name + "Locked"), TileCollision.Passable, "Tiles/Buttons/MenuPlay/" + name + "Locked");

                case "3":
                    World3Box = new Rectangle(x*Tile.Width, (y*Tile.Height)-(Tile.Height/2), 200,64);
                    World3OuterBox = new Rectangle(World3Box.Left - (Tile.Width * 2), World3Box.Top - (Tile.Height * 2), World3Box.Width + (Tile.Width * 4), World3Box.Height + (Tile.Height * 4));
                    if (mw3unlocked == true)
                        return new Tile(Content.Load<Texture2D>("Tiles/Buttons/MenuPlay/" + name), TileCollision.Passable, "Tiles/Buttons/MenuPlay/" + name);
                    else
                        return new Tile(Content.Load<Texture2D>("Tiles/Buttons/MenuPlay/" + name + "Locked"), TileCollision.Passable, "Tiles/Buttons/MenuPlay/" + name + "Locked");
                case "4":
                    World4Box = new Rectangle(x*Tile.Width, (y*Tile.Height)-(Tile.Height/2), 200,64);
                    World4OuterBox = new Rectangle(World4Box.Left - (Tile.Width * 2), World4Box.Top - (Tile.Height * 2), World4Box.Width + (Tile.Width * 4), World4Box.Height + (Tile.Height * 4));
                    if (mw4unlocked == true)
                        return new Tile(Content.Load<Texture2D>("Tiles/Buttons/MenuPlay/" + name), TileCollision.Passable, "Tiles/Buttons/MenuPlay/" + name);
                    else
                        return new Tile(Content.Load<Texture2D>("Tiles/Buttons/MenuPlay/" + name + "Locked"), TileCollision.Passable, "Tiles/Buttons/MenuPlay/" + name + "Locked");
                case "5":
                    World5Box = new Rectangle(x*Tile.Width, (y*Tile.Height)-(Tile.Height/2), 200,64);
                    World5OuterBox = new Rectangle(World5Box.Left - (Tile.Width * 2), World5Box.Top - (Tile.Height * 2), World5Box.Width + (Tile.Width * 4), World5Box.Height + (Tile.Height * 4));
                    if (mw5unlocked == true)
                        return new Tile(Content.Load<Texture2D>("Tiles/Buttons/MenuPlay/" + name), TileCollision.Passable, "Tiles/Buttons/MenuPlay/" + name);
                    else
                        return new Tile(Content.Load<Texture2D>("Tiles/Buttons/MenuPlay/" + name + "Locked"), TileCollision.Passable, "Tiles/Buttons/MenuPlay/" + name + "Locked");
                #endregion
                
                case null:
                    return new Tile(null, collision, null);

                default:
                    return new Tile(Content.Load<Texture2D>("Tiles/Buttons/" + name), TileCollision.Passable, "Tiles/Buttons/" + name);
            }
        }

        /// <summary>
        /// Instantiates a player, puts him in the level, and remembers where to put him when he is resurrected.
        /// </summary>
        private Tile LoadStartTile(int x, int y)
        {
            if (Player != null)
                throw new NotSupportedException("A level may only have one starting point.");

            start = RectangleExtensions.GetBottomCenter(GetBounds(x, y));
            player = new Player(this, start);

            return new Tile(null, TileCollision.Passable, "Player");
        }

        /// <summary>
        /// Remembers the location of the level's exit.
        /// </summary>
        private Tile LoadExitTile(int x, int y)
        {
            if (exit != InvalidPosition)
                throw new NotSupportedException("A level may only have one exit.");

            exit = GetBounds(x, y).Center;

            if (mlevelIndex >= mw1l1 && mlevelIndex < mw2l1)
                return LoadTile("Exit1", TileCollision.Passable);
            else if (mlevelIndex >= mw2l1 && mlevelIndex < mw3l1)
                return LoadTile("Exit2", TileCollision.Passable);
            else if (mlevelIndex >= mw3l1 && mlevelIndex < mw4l1)
                return LoadTile("Exit3", TileCollision.Passable);
            else if (mlevelIndex >= mw4l1 && mlevelIndex < mw5l1)
                return LoadTile("Exit4", TileCollision.Passable);
            else if (mlevelIndex >= mw5l1)
                return LoadTile("Exit5", TileCollision.Passable);
            else
                return LoadTile("Exit_Default", TileCollision.Passable);
        }

        /// <summary>
        /// Instantiates an enemy and puts him in the level.
        /// </summary>
        private Tile LoadEnemyTile(int x, int y, string spriteSet)
        {
            Vector2 position = RectangleExtensions.GetBottomCenter(GetBounds(x, y));
            enemies.Add(new Enemy(this, position, spriteSet));

            return new Tile(null, TileCollision.Passable, "Enemy");
        }

        /// <summary>
        /// Unloads the level content.
        /// </summary>
        public void Dispose()
        {
            Content.Unload();
        }
        #endregion

        #region Bounds and collision

        /// <summary>
        /// Gets the collision mode of the tile at a particular location.
        /// This method handles tiles outside of the levels boundries by making it
        /// impossible to escape past the left or right edges, but allowing things
        /// to jump beyond the top of the level and fall off the bottom.
        /// </summary>
        public TileCollision GetCollision(int x, int y)
        {
            // Prevent escaping past the level ends.
            if (x < 0 || x >= Width)
                return TileCollision.Impassable;
            // Allow jumping past the level top and falling through the bottom.
            if (y < 0 || y >= Height)
                return TileCollision.Passable;

            return tiles[x, y].Collision;
        }

        /// <summary>
        /// Gets the bounding rectangle of a tile in world space.
        /// </summary>        
        public Rectangle GetBounds(int x, int y)
        {
            return new Rectangle(x * Tile.Width, y * Tile.Height, Tile.Width, Tile.Height);
        }

        /// <summary>
        /// Width of level measured in tiles.
        /// </summary>
        public int Width
        {
            get { return tiles.GetLength(0); }
        }

        /// <summary>
        /// Height of the level measured in tiles.
        /// </summary>
        public int Height
        {
            get { return tiles.GetLength(1); }
        }

        #endregion

        #region Update

        /// <summary>
        /// Updates all objects in the world, performs collision between them,
        /// and handles the time limit with scoring.
        /// </summary>
        public void Update(
            GameTime gameTime,
            KeyboardState keyboardState,
            GamePadState gamePadState,
            TouchCollection touchState, 
            AccelerometerState accelState,
            DisplayOrientation orientation,
            int mlevelIndex)
            {
            /*
                if (mlevelIndex.Equals(0))
                {
                    if (!splashScreenTimer.Equals(TimeSpan.Zero))
                    {
                        // minus the time
                        splashScreenTimer -= gameTime.ElapsedGameTime;
                    }
                    else
                        OnExitReached();

                    // Clamp the time remaining at zero.
                    if (splashScreenTimer < TimeSpan.Zero)
                        splashScreenTimer = TimeSpan.Zero;
                }
            */

            mouse = Mouse.GetState(); // Good sized box around the tip of the cursor
            MouseBox = new Rectangle(mouse.X-7, mouse.Y - 20, 10, 10); 

            // Pause while the player is dead or time is expired.
            if (!Player.IsAlive || TimeRemaining == TimeSpan.Zero)
            {
                // Still want to perform physics on the player.
                //Player.ApplyPhysics(gameTime);
            }
            else if (ReachedExit)
            {
                // Animate the time being converted into points.
                int seconds = (int)Math.Round(gameTime.ElapsedGameTime.TotalSeconds * 100.0f);
                seconds = Math.Min(seconds, (int)Math.Ceiling(TimeRemaining.TotalSeconds));
                timeRemaining -= TimeSpan.FromSeconds(seconds);
                score += seconds * PointsPerSecond;
            }
            else
            {
                if(mlevelIndex >= mw1l1) // If not in a menu
                    timeRemaining -= gameTime.ElapsedGameTime;
                Player.Update(gameTime, keyboardState, gamePadState, touchState, accelState, orientation);
                

                // Falling off the bottom of the level kills the player.
                if (Player.BoundingRectangle.Top >= Height * Tile.Height)
                    OnPlayerKilled(null);

                UpdateEnemies(gameTime);

                // The player has reached the exit if they are standing on the ground and
                // his bounding rectangle contains the center of the exit tile. They can only
                // exit when they have collected all of the gems.
                if (Player.IsAlive &&
                    Player.BoundingRectangle.Contains(exit))
                {
                    OnExitReached();
                }
            }

            // Clamp the time remaining at zero.
            if (timeRemaining < TimeSpan.Zero)
                timeRemaining = TimeSpan.Zero;
        }
        
        /// <summary>
        /// Animates each enemy and allow them to kill the player.
        /// </summary>
        private void UpdateEnemies(GameTime gameTime)
        {
            foreach (Enemy enemy in enemies)
            {
                enemy.Update(gameTime);

                // Touching an enemy instantly kills the player
                if (enemy.IsAlive && enemy.BoundingRectangle.Intersects(Player.BoundingRectangle))
                {
                    OnPlayerKilled(enemy);
                }
            }
        }
        private void OnEnemyKilled(Enemy enemy, Player killedBy)
        {
            enemy.OnKilled(killedBy);
        }

        /// <summary>
        /// Called when the player is killed.
        /// </summary>
        /// <param name="killedBy">
        /// The enemy who killed the player. This is null if the player was not killed by an
        /// enemy, such as when a player falls into a hole.
        /// </param>
        private void OnPlayerKilled(Enemy killedBy)
        {
            Player.OnKilled(TileCollision.Enemy);
        }

        /// <summary>
        /// Called when the player reaches the level's exit.
        /// </summary>
        private void OnExitReached()
        {
            Player.OnReachedExit();
            if(tempSFX == true)
                Player.ExitReached.Play();
            
            reachedExit = true;
        }

        /// <summary>
        /// Restores the player to the starting point to try the level again.
        /// </summary>
        public void StartNewLife()
        {
            Player.Reset(start);
        }

        #endregion

        #region Draw

        /// <summary>
        /// Draw everything in the level from background to foreground.
        /// </summary>
        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            mspriteBatch = spriteBatch;
            //Background?
            spriteBatch.Begin();
            for (int i = 0; i <= EntityLayer; ++i)
                layers[i].Draw(spriteBatch, cameraPositionXaxis);
            spriteBatch.End();

            // Midground and playable area?
            ScrollCamera(spriteBatch.GraphicsDevice.Viewport);
            Matrix cameraTransform = Matrix.CreateTranslation(-cameraPositionXaxis, -cameraPositionYaxis, 0.0f);
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default,
                              RasterizerState.CullCounterClockwise, null, cameraTransform);

            DrawTiles(spriteBatch);
            
            // Still need this, dont delete
            for (int i = 0; i < transforms.Count; i++)
            {
                if (transforms[i].collected == false)
                    spriteBatch.Draw(transforms[i].texture, new Vector2((int)transforms[i].position.X, (int)transforms[i].position.Y), Color.White);
            }

            foreach (AnimatedTile tile in animatedtiles)
                tile.Draw(gameTime, spriteBatch);
            
            Player.Draw(gameTime, spriteBatch);

            foreach (Enemy enemy in enemies)
                enemy.Draw(gameTime, spriteBatch);

            spriteBatch.End();

            // Draw all layers?
            spriteBatch.Begin();
            for (int i = EntityLayer + 1; i < layers.Length; ++i)
                layers[i].Draw(spriteBatch, cameraPositionXaxis);
            spriteBatch.End();
        }

        /// <summary>
        /// Draws each tile in the level.
        /// </summary>
        private void DrawTiles(SpriteBatch spriteBatch)
        {
            // Calculate the visible range of tiles.
            int left = (int)Math.Floor(cameraPositionXaxis / Tile.Width);
            int right = left + spriteBatch.GraphicsDevice.Viewport.Width / Tile.Width;
            right = Math.Min(right, Width - 1);

            // For each tile position
            for (int y = 0; y < Height; ++y)
            {
                for (int x = left; x <= right; ++x)
                {
                    // If there is a visible tile in that position
                    Texture2D texture = tiles[x, y].Texture;
                    if (texture != null)
                    {
                        // Draw it in screen space.
                        Vector2 position = new Vector2(x, y) * Tile.Size;
                        spriteBatch.Draw(texture, position, Color.White);
                    }
                }
            }
        }

        private void ScrollCamera(Viewport viewport)
        {
#if ZUNE
const float ViewMargin = 0.45f;
#else
            const float ViewMargin = 0.35f;
#endif
            // Calculate the edges of the screen.
            marginWidth = viewport.Width * ViewMargin;
            marginLeft = cameraPositionXaxis + marginWidth;
            marginRight = cameraPositionXaxis + viewport.Width - marginWidth;

            // Calcuate the vertical scrolling parts
            TopMargin = 0.5f;// cameraPositionYaxis + (viewport.Height * ViewMargin);
            BottomMargin = 0.5f;
            float marginTop = cameraPositionYaxis + viewport.Height * TopMargin;
            float marginBottom = cameraPositionYaxis + viewport.Height * BottomMargin;

            float cameraMovementY = 0.0f;
            if (Player.Position.Y < marginTop)
                cameraMovementY = player.Position.Y - marginTop;
            else if (Player.Position.Y > marginBottom)
                cameraMovementY = Player.Position.Y - marginBottom;

            // Calculate how far to scroll when the player is near the edges of the screen.
            float cameraMovement = 0.0f;
            if (Player.Position.X < marginLeft)
                cameraMovement = Player.Position.X - marginLeft;
            else if (Player.Position.X > marginRight)
                cameraMovement = Player.Position.X - marginRight;

            // Update the camera position, but prevent scrolling off the ends of the level.
            float maxCameraPositionXOffset = Tile.Width * Width - viewport.Width;
            float maxCameraPositionYOffset = Tile.Height * Height - viewport.Height;
            cameraPositionXaxis = MathHelper.Clamp(cameraPositionXaxis + cameraMovement, 0.0f, maxCameraPositionXOffset);
            cameraPositionYaxis = MathHelper.Clamp(cameraPositionYaxis + cameraMovementY, 0.0f, maxCameraPositionYOffset);
        }

        #endregion

        public Point GetCoords(String texture)
        {
            Point point = new Point();
            // Loop over every tile position,
            for (int y = 0; y < Height; ++y) // while y < the amount of lines
            {
                for (int x = 0; x < Width; ++x) // while x < the width of a line
                {
                    if (tiles[x, y].getName() != null)
                        if (tiles[x, y].getName().Contains(texture))
                        {
                            //point = "Got it!";
                            point.X = x;
                            point.Y = y;
                        }
                }
            }

            return point;
        }

        public void WorldSelected(int world)
        {
            if (mlevelIndex < mw1l1)
            {
                // Remove all "level buttons" so that the correct set can be loaded in each time
                for (int i = 1; i < 11; i++)
                    LoadNewTileAt(null, TileCollision.Passable, 16, i * 2);
                MenuItemList.Clear();

            
                if (world.Equals(5))
                    for (int i = 1; i < mnumberOfLevels + 1 - mw5l1; i++)
                    {
                        if (((mw5l1 - 1) + i) <= mhighestLevel)
                        {
                            LoadNewTileAt("Tiles/Buttons/MenuPlay/Levels/lv" + i, TileCollision.Passable, 16, i * 2);
                            MenuItemList.Add(new MenuItem(new Rectangle(16 * Tile.Width, ((i * 2) * Tile.Height) - Tile.Height / 2, 100, 32), new Point(16, i * 2), i, mw5l1 - 1));
                        }
                        else
                            LoadNewTileAt("Tiles/Buttons/MenuPlay/Levels/lv" + i + "Locked", TileCollision.Passable, 16, i * 2);
                    }
                else if (world.Equals(4))
                    for (int i = 1; i < mw5l1 + 1 - mw4l1; i++)
                    {
                        if (((mw4l1 - 1) + i) <= mhighestLevel)
                        {
                            LoadNewTileAt("Tiles/Buttons/MenuPlay/Levels/lv" + i, TileCollision.Passable, 16, i * 2);
                            MenuItemList.Add(new MenuItem(new Rectangle(16 * Tile.Width, ((i * 2) * Tile.Height) - Tile.Height / 2, 100, 32), new Point(16, i * 2), i, mw4l1 - 1));
                        }
                        else
                            LoadNewTileAt("Tiles/Buttons/MenuPlay/Levels/lv" + i + "Locked", TileCollision.Passable, 16, i * 2);
                    }
                else if (world.Equals(3))
                    for (int i = 1; i < mw4l1 + 1 - mw3l1; i++)
                    {
                        if (((mw3l1 - 1) + i) <= mhighestLevel)
                        {
                            LoadNewTileAt("Tiles/Buttons/MenuPlay/Levels/lv" + i, TileCollision.Passable, 16, i * 2);
                            MenuItemList.Add(new MenuItem(new Rectangle(16 * Tile.Width, ((i * 2) * Tile.Height) - Tile.Height / 2, 100, 32), new Point(16, i * 2), i, mw3l1 - 1));
                        }
                        else
                            LoadNewTileAt("Tiles/Buttons/MenuPlay/Levels/lv" + i + "Locked", TileCollision.Passable, 16, i * 2);
                    }
                else if (world.Equals(2))
                    for (int i = 1; i < mw3l1 + 1 - mw2l1; i++)
                    {
                        if (((mw2l1 - 1) + i) <= mhighestLevel)
                        {
                            LoadNewTileAt("Tiles/Buttons/MenuPlay/Levels/lv" + i, TileCollision.Passable, 16, i * 2);
                            MenuItemList.Add(new MenuItem(new Rectangle(16 * Tile.Width, ((i * 2) * Tile.Height) - Tile.Height / 2, 100, 32), new Point(16, i * 2), i, mw2l1 - 1));
                        }
                        else
                            LoadNewTileAt("Tiles/Buttons/MenuPlay/Levels/lv" + i + "Locked", TileCollision.Passable, 16, i * 2);
                    }
                else if (world.Equals(1)) // w2l1 + 1 = inclusive of the caps, without it it would be exclusive
                    for (int i = 1; i < mw2l1 + 1 - mw1l1; i++)
                    {
                        if (((mw1l1 - 1) + i) <= mhighestLevel)
                        {
                            LoadNewTileAt("Tiles/Buttons/MenuPlay/Levels/lv" + i, TileCollision.Passable, 16, i * 2);
                            MenuItemList.Add(new MenuItem(new Rectangle(16 * Tile.Width, ((i * 2) * Tile.Height) - Tile.Height / 2, 100, 32), new Point(16, i * 2), i, mw1l1 - 1));
                        }
                        else
                            LoadNewTileAt("Tiles/Buttons/MenuPlay/Levels/lv" + i + "Locked", TileCollision.Passable, 16, i * 2);
                    }
                else // assume invalid entry
                    for (int i = 1; i < 11; i++)
                        LoadNewTileAt(null, TileCollision.Passable, 16, i * 2);
            }
            //LoadNewTileAt();
        }

    }
}
