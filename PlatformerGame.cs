#region File Description
//-----------------------------------------------------------------------------
// PlatformerGame.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion
#region Libraries
using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Input.Touch;
// Allow the use of Message Boxes
using System.Runtime.InteropServices;
#endregion

/// <summary>
/// This is the main type for your game
/// It inherits some functions from the Microsoft.Xna.Framework.Game class
/// </summary>
namespace Platformer {
 
    public class PlatformerGame : Microsoft.Xna.Framework.Game
    {
        // Allows the use of messgae boxes!!
        [DllImport("user32.dll", EntryPoint = "MessageBox")]
        public static extern int ShowMessage(int hWnd,
        string text, string caption, uint type);
        
        // Settings Menu items
        public bool isFullScreen = false;
        public bool isMusicOn = false;
        public bool isSFXOn = true;

        // Resources for drawing.
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        public GameTime gametime;
        bool drawscore = false;
        
        // Mouse position, state, and texture
        public MouseState ms;
        public Texture2D pointer;
        public Point mouseposition;

        // Fonts for writing
        private SpriteFont hudFont;
        private SpriteFont timeFont;
        public SpriteFont textFont;
        // Textures for all of the overlays that pop up

        private Texture2D splashScreen;
        private Texture2D unlockedOverlay;
        private Texture2D timeOverlay;
        private Texture2D IconBob, IconCarbon, IconIron, IconHelium, IconOxygen, IconLN;
        private Texture2D winOverlay;
        private Texture2D loseOverlay;
        private Texture2D diedOverlay;

        // Sound
        private SoundEffect mouseHover;
        private SoundEffect mouseClick;
        private int sfxTimer = 0;

        // Meta-level game state.
        private Level level;
        private bool wasContinuePressed;

        // When the time remaining is less than the warning time, it blinks on the hud
        private static readonly TimeSpan WarningTime = TimeSpan.FromSeconds(30);

        // We store our input states so that we only poll once per frame, 
        // then we use the same input state wherever needed
        private GamePadState gamePadState;
        private KeyboardState keyboardState;
        private TouchCollection touchState;
        private AccelerometerState accelerometerState;

        public MouseState mouse;
        public Rectangle MouseBox;
        
        // The number of levels in the Levels directory of our content. We assume that
        // levels in our content are 0-based and that all numbers under this constant
        // have a level file present. This allows us to not need to check for the file
        // or handle exceptions, both of which can add unnecessary time to level loading.
        private int numberOfLevels = 54; //*.txt + 1 (we add the +1 because the list is 0-indexing)
        private int levelIndex = -1;     // -1 is the null value
        private int levelIndexLast = -1;    // -1 is the null value
        public int inWorld = 0;             // which world the player is currently in, used for differing tilesets
        public int inWorldLast = 0;         // which world the player was last in
        public int highestLevel = 54; // Set to w1l1 by default so that player can access first level
        
        // Menu = 0; means the menu is loaded from the "0.txt" file
        public const int MenuMain = 0;
        public const int MenuHelp = 1;
        public const int MenuSettings = 2;
        public const int MenuPlay = 3;
        public const int w1l1 = 4;
        public const int w2l1 = 14;
        public const bool w2unlocked = true;
        public const int w3l1 = 24;
        public const bool w3unlocked = true;
        public const int w4l1 = 34;
        public const bool w4unlocked = true;
        public const int w5l1 = 44;
        public const bool w5unlocked = true;

        /// <summary>
        /// Initialises the class.
        /// Sets the graphics device, external content directory, and screen resolution
        /// </summary>
        public PlatformerGame()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            if (isFullScreen == true)
                graphics.IsFullScreen = true;
            else
                graphics.IsFullScreen = false;

            graphics.PreferredBackBufferHeight = 768;  // 24 tiles
            graphics.PreferredBackBufferWidth = 1040;  // 26 tiles

            #region Load highest level.
            // Comment out for regular coding
            /*
            using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (IsolatedStorageFileStream file = new IsolatedStorageFileStream("Content/Levels/levels.txt", FileMode.Open, FileAccess.Read, store))
                {
                    //using (StreamReader reader = new StreamReader(file))
                    using ( TextReader reader = new StreamReader(file) )
                    {
                        string text = reader.ReadLine();
                        string[] bits = text.Split(' '); // split by spaces
                        highestLevel = int.Parse(bits[0]); // zero indexing
                        w1l1 = int.Parse(bits[1]);
                        w2l1 = int.Parse(bits[2]);
                        String strW2 = bits[3];
                        w3l1 = int.Parse(bits[4]);
                        String strW3 = bits[5];
                        w4l1 = int.Parse(bits[6]);
                        String strW4 = bits[7];
                        w5l1 = int.Parse(bits[8]);
                        String strW5 = bits[9];
                        numberOfLevels = int.Parse(bits[10]);
                        if (strW2.Equals("true")) w2unlocked = true; else w2unlocked = false;
                        if (strW3.Equals("true")) w3unlocked = true; else w3unlocked = false;
                        if (strW4.Equals("true")) w4unlocked = true; else w4unlocked = false;
                        if (strW5.Equals("true")) w5unlocked = true; else w5unlocked = false;

                        reader.Close();
                    }
                }
            }
            */
            // Comment out for release
            /*
            using (TextReader reader = File.OpenText("Content/Levels/levels.txt"))
            {
                string text = reader.ReadLine();
                string[] bits = text.Split(' '); // split by spaces
                highestLevel = int.Parse(bits[0]); // zero indexing
                w1l1 = int.Parse(bits[1]);
                w2l1 = int.Parse(bits[2]);
                String strW2 = bits[3];
                w3l1 = int.Parse(bits[4]);
                String strW3 = bits[5];
                w4l1 = int.Parse(bits[6]);
                String strW4 = bits[7];
                w5l1 = int.Parse(bits[8]);
                String strW5 = bits[9];
                numberOfLevels = int.Parse(bits[10]);
                if (strW2.Equals("true")) w2unlocked = true; else w2unlocked = false;
                if (strW3.Equals("true")) w3unlocked = true; else w3unlocked = false;
                if (strW4.Equals("true")) w4unlocked = true; else w4unlocked = false;
                if (strW5.Equals("true")) w5unlocked = true; else w5unlocked = false;
            }
            */
            #endregion

#if WINDOWS_PHONE
            graphics.IsFullScreen = true;
            TargetElapsedTime = TimeSpan.FromTicks(333333);
#endif

            Accelerometer.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // Load fonts
            hudFont = Content.Load<SpriteFont>("Fonts/Hud");
            timeFont = Content.Load<SpriteFont>("Fonts/timefont");
            textFont = Content.Load<SpriteFont>("Fonts/Hud");
            // Load overlay textures
            winOverlay = Content.Load<Texture2D>("Overlays/you_win");
            loseOverlay = Content.Load<Texture2D>("Overlays/you_lose");
            diedOverlay = Content.Load<Texture2D>("Overlays/you_died");
            pointer = Content.Load<Texture2D>("Sprites/mouse");

            splashScreen = Content.Load<Texture2D>("Overlays/splashScreen");
            timeOverlay = Content.Load<Texture2D>("Overlays/TimeOverlay");
            unlockedOverlay = Content.Load<Texture2D>("Overlays/ElementOverlay");
            IconBob = Content.Load<Texture2D>("Overlays/Unlocked/Icons/IconBob");
            IconCarbon = Content.Load<Texture2D>("Overlays/Unlocked/Icons/IconCarbon");
            IconIron = Content.Load<Texture2D>("Overlays/Unlocked/Icons/IconIron");
            IconHelium = Content.Load<Texture2D>("Overlays/Unlocked/Icons/IconHelium");
            IconOxygen = Content.Load<Texture2D>("Overlays/Unlocked/Icons/IconOxygen");
            IconLN = Content.Load<Texture2D>("Overlays/Unlocked/Icons/IconLN");

            mouseHover = Content.Load<SoundEffect>("Sounds/Other/mouseHover");
            mouseClick = Content.Load<SoundEffect>("Sounds/Other/mouseClick");

            //Known issue that you get exceptions if you use Media PLayer while connected to your PC
            //See http://social.msdn.microsoft.com/Forums/en/windowsphone7series/thread/c8a243d2-d360-46b1-96bd-62b1ef268c66
            //Which means its impossible to test this from VS.
            //So we have to catch the exception and throw it away

            LoadNextLevel();
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Handle polling for our input and handling high-level input
            HandleInput();

            ms = Mouse.GetState();
            mouseposition.X = ms.X;
            mouseposition.Y = ms.Y;

            CheckMenuMouseover();

            // update our level, passing down the GameTime along with all of our input states
            level.Update(gameTime, keyboardState, gamePadState, touchState, 
                         accelerometerState, Window.CurrentOrientation, levelIndex);

            base.Update(gameTime);
        }

        /// <summary>
        /// HandleInput is called when the program needs to check for device input
        /// </summary>
        private void HandleInput()
        {
            // get all of our input states
            keyboardState = Keyboard.GetState();
            gamePadState = GamePad.GetState(PlayerIndex.One);
            touchState = TouchPanel.GetState();
            accelerometerState = Accelerometer.GetState();

            if (levelIndex >= w1l1) // not in a menu
            {
                ChangeWithKeyPress();
            }

            // Exit the game when back is pressed.
            if (gamePadState.Buttons.Back == ButtonState.Pressed ||
                keyboardState.IsKeyDown(Keys.Escape))
            {
                if (levelIndex > MenuMain)
                {
                    levelIndex = MenuMain - 1;
                    LoadNextLevel();
                }
            }
            
            bool continuePressed =
                keyboardState.IsKeyDown(Keys.Enter) ||
                gamePadState.IsButtonDown(Buttons.A) ||
                touchState.AnyTouch();

            // Perform the appropriate action to advance the game and
            // to get the player back to playing.
            if (!wasContinuePressed && continuePressed)
            {
                if (!level.Player.IsAlive)
                {
                    //level.StartNewLife();
                    ReloadCurrentLevel();
                }
                else if (level.TimeRemaining == TimeSpan.Zero)
                {
                    if (level.ReachedExit)
                        LoadNextLevel();
                    else
                        ReloadCurrentLevel();
                }
            }

            wasContinuePressed = continuePressed;
        }
        
        /// <summary>
        /// Determines which level to load next,
        /// Dictates which world (set of 10 levels) the player is in,
        /// Loads the next levels file into a Stream Reader and executes it
        /// </summary>
        private void LoadNextLevel()
        {
            // If the player reaches the end of the game
            if (levelIndex.Equals(numberOfLevels-1))
                ShowMessage(0, "You have completed the game, Congrats!\nYou will be returned to the menu\nHope you enjoyed it!", "", 0);
            
            // index to next level
            levelIndexLast = levelIndex;
            levelIndex = (levelIndex + 1) % numberOfLevels;

            #region Determine/Unlock world, play bg music
            // Determine which world we are currently in
            inWorldLast = inWorld;
            if (levelIndex >= w1l1 && levelIndex < w2l1)
                inWorld = 1;
            else if (levelIndex >= w2l1 && levelIndex < w3l1)
                inWorld = 2;
            else if (levelIndex >= w3l1 && levelIndex < w4l1)
                inWorld = 3;
            else if (levelIndex >= w4l1 && levelIndex < w5l1)
                inWorld = 4;
            else if (levelIndex >= w5l1)
                inWorld = 5;

            PlayMusic();
            #endregion

            // If current level is higher than highest previous level
            if (levelIndex > highestLevel)
            {
                highestLevel = levelIndex;
            }
            
            // Unloads the content for the current level before loading the next one.
            if (level != null)
                level.Dispose();

            // Check to play musics
            PlayMusic();

            // Load the level.
            string levelPath = string.Format("Content/Levels/{0}.txt", levelIndex);
            using (Stream fileStream = TitleContainer.OpenStream(levelPath))
                level = new Level(Services, fileStream, levelIndex, graphics, isMusicOn, isSFXOn, MenuMain, MenuHelp, MenuSettings, MenuPlay,
                                  highestLevel, inWorld, w1l1, w2l1, w3l1, w4l1, w5l1, numberOfLevels, w2unlocked, w3unlocked, w4unlocked, w5unlocked);
        }

        /// <summary>
        /// Decrements the levelIndex (remembers which level the player is currently on) by one
        /// Calls the LoadNextLevel() function
        /// </summary>
        private void ReloadCurrentLevel()
        {
            --levelIndex;
            LoadNextLevel();
        }

        /// <summary>
        /// Draws the game from background to foreground.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(Color.CornflowerBlue);

            gametime = gameTime; // needed for the drawing of the player after keypress

            level.Draw(gameTime, spriteBatch);

            DrawHud();

            base.Draw(gameTime);
        }

        /// <summary>
        /// Draws the overlays for the time and 'unlocked elements' panels as well as the win/lose overlays
        /// </summary>
        private void DrawHud()
        {
            spriteBatch.Begin();

            // Version 1.1.5  = Beta Testing #1
            DrawShadowedString(hudFont, "Version=1.2.0.2", new Vector2(graphics.PreferredBackBufferWidth/2-100, 0), Color.White);

            #region Menu Texts
            //Help Screen Text
            if (levelIndex == MenuHelp)
            {
                spriteBatch.DrawString(level.HelpTextFont, level.HelpTextList[level.HelpTextListIndex].ToString(), new Vector2(level.HelpTextPosition.X + 80, level.HelpTextPosition.Y + 50), Color.Black);

                if (level.HelpTextListIndex == 2)
                {
                    int leftside = 160;
                    spriteBatch.DrawString(textFont, "{key}, Speed,    Jump,   Flamable?   Gaseous?   Floats?   Freezes?      Name", new Vector2(leftside - 60, 80), Color.Green);
                    for (int i = 0; i < level.Player.list.Elements.Count; i++)
                    {
                        spriteBatch.DrawString(textFont, "# " + i + ": ", new Vector2(leftside - 60, 100 + (i * 20)), Color.SteelBlue);
                        spriteBatch.DrawString(textFont, level.Player.list.Elements[i].speed.ToString(), new Vector2(leftside, 100 + (i * 20)), Color.SteelBlue);
                        spriteBatch.DrawString(textFont, level.Player.list.Elements[i].jump.ToString(), new Vector2(leftside + 100, 100 + (i * 20)), Color.SteelBlue);
                        spriteBatch.DrawString(textFont, level.Player.list.Elements[i].flamable.ToString(), new Vector2(leftside + 200, 100 + (i * 20)), Color.SteelBlue);
                        spriteBatch.DrawString(textFont, level.Player.list.Elements[i].gas.ToString(), new Vector2(leftside + 300, 100 + (i * 20)), Color.SteelBlue);
                        spriteBatch.DrawString(textFont, level.Player.list.Elements[i].floats.ToString(), new Vector2(leftside + 400, 100 + (i * 20)), Color.SteelBlue);
                        spriteBatch.DrawString(textFont, level.Player.list.Elements[i].freezes.ToString(), new Vector2(leftside + 500, 100 + (i * 20)), Color.SteelBlue);
                        spriteBatch.DrawString(textFont, level.Player.list.Elements[i].name, new Vector2(leftside + 600, 100 + (i * 20)), Color.SteelBlue);
                    }
                }
            }
            #endregion

            #region Score/Time
            Rectangle titleSafeArea = GraphicsDevice.Viewport.TitleSafeArea;
            Vector2 hudLocation = new Vector2(titleSafeArea.X+65, titleSafeArea.Y+15);
            Vector2 center = new Vector2(titleSafeArea.X + titleSafeArea.Width / 2.0f,
                                         titleSafeArea.Y + titleSafeArea.Height / 2.0f);

            // Draw time remaining. Uses modulo division to cause blinking when the
            // player is running out of time.
            string timeString = level.TimeRemaining.Minutes.ToString("00") + ":" + level.TimeRemaining.Seconds.ToString("00");
            Color timeColor;
            if (level.TimeRemaining > WarningTime ||
                level.ReachedExit ||
                (int)level.TimeRemaining.TotalSeconds % 2 == 0)
            {
                timeColor = Color.BlanchedAlmond;
            }
            else
            {
                timeColor = Color.Orchid;
            }
            if (levelIndex >= w1l1)
            {
                //DrawShadowedString(hudFont, "TIME", new Vector2(65,0), Color.Yellow);
                //DrawShadowedString(timefont, timeString, hudLocation, timeColor);
            }
            // Draw score
            float timeHeight = hudFont.MeasureString(timeString).Y;
            if(drawscore == true)
                DrawShadowedString(hudFont, "SCORE: " + level.Score.ToString(), hudLocation + new Vector2(0.0f, timeHeight * 1.2f), Color.Yellow);
            #endregion

            #region Win/Lose Overlay
            // Determine the status overlay message to show.
            Texture2D status = null;
            if (level.TimeRemaining == TimeSpan.Zero)
            {
                if (level.ReachedExit)
                {
                    status = winOverlay;
                }
                else
                {
                    status = loseOverlay;
                }
            }
            else if (!level.Player.IsAlive)
            {
                status = diedOverlay;
            }

            if (status != null)
            {
                // Draw status message.
                Vector2 statusSize = new Vector2(status.Width, status.Height);
                spriteBatch.Draw(status, center - statusSize / 2, Color.White);
            }
            #endregion

            #region Unlock Overlay
            if (level.Player.status != null && level.Player.OverlayTime != TimeSpan.Zero)
            {
                Vector2 statusSize = new Vector2(level.Player.status.Width, level.Player.status.Height);
                spriteBatch.Draw(level.Player.status, center - statusSize / 2, Color.White);
            } 
            #endregion

            #region Game HUD
            if (levelIndex < w1l1)
                spriteBatch.Draw(pointer, new Vector2(mouseposition.X, mouseposition.Y), Color.White);

            if (levelIndex >= w1l1)
            {
                Vector2 timeSize = new Vector2(unlockedOverlay.Width, unlockedOverlay.Height);
                Rectangle timeRect = new Rectangle(0, 0, (int)timeSize.X, (int)timeSize.Y);
                spriteBatch.Draw(timeOverlay, timeRect, Color.White);

                DrawShadowedString(timeFont, timeString, new Vector2(timeRect.Left + 85, 2), timeColor);
                
                DrawShadowedString(hudFont, (levelIndex - 3 - ((inWorld-1)*10)).ToString(), new Vector2(timeRect.Right - 110, 2), Color.BlanchedAlmond);
                DrawShadowedString(hudFont, inWorld.ToString(), new Vector2(timeRect.Right - 35, 2), Color.BlanchedAlmond);

                Vector2 overlaySize = new Vector2(unlockedOverlay.Width, unlockedOverlay.Height);
                Rectangle overlayRect = new Rectangle(graphics.PreferredBackBufferWidth - (int)overlaySize.X, 0, (int)overlaySize.X, (int)overlaySize.Y);
                spriteBatch.Draw(unlockedOverlay, overlayRect, Color.White);
                Vector2 iconSize = new Vector2(IconBob.Width, IconBob.Height);
                Rectangle iconRect = new Rectangle((overlayRect.Left + overlayRect.Width / 2) + 3, overlayRect.Top + 6, (int)iconSize.X, (int)iconSize.Y);
                spriteBatch.Draw(IconBob, iconRect, Color.White);
                
                for (int i = 0; i < level.Player.list.Elements.Count; i++)
                {
                    if (level.Player.list.Elements[i].unlocked)
                    {
                        if(i.Equals(1))
                            spriteBatch.Draw(IconCarbon, new Rectangle(iconRect.Left + 27 * i, iconRect.Y, iconRect.Width, iconRect.Height) , Color.White);
                        if (i.Equals(2))
                            spriteBatch.Draw(IconIron, new Rectangle(iconRect.Left + 27 * i, iconRect.Y, iconRect.Width, iconRect.Height), Color.White);
                        if (i.Equals(3))
                            spriteBatch.Draw(IconHelium, new Rectangle(iconRect.Left + 27 * i, iconRect.Y, iconRect.Width, iconRect.Height), Color.White);
                        if (i.Equals(4))
                            spriteBatch.Draw(IconOxygen, new Rectangle(iconRect.Left + 27 * i, iconRect.Y, iconRect.Width, iconRect.Height), Color.White);
                        if (i.Equals(5))
                            spriteBatch.Draw(IconLN, new Rectangle(iconRect.Left + 27 * i, iconRect.Y, iconRect.Width, iconRect.Height), Color.White);
                    }
                }

            }
            #endregion
            /*
            #region splashScreen
            if (levelIndex.Equals(0))
            {
                Vector2 splashSize = new Vector2(splashScreen.Width, splashScreen.Height);
                Rectangle splashRect = new Rectangle(20, 20, (int)splashSize.X, (int)splashSize.Y);
                spriteBatch.Draw(splashScreen, splashRect, Color.White);
            }
            else
                splashScreen = Content.Load<Texture2D>(null);
            #endregion
            */
            spriteBatch.End();
        }
        
        /// <summary>
        /// Called by the HandleInput() function
        /// Contains all the code that deals with the mouse collision of menu items and results
        /// </summary>
        public void CheckMenuMouseover()
        {
            ms = Mouse.GetState();
            level.mouse = Mouse.GetState();
            Point p = new Point();

            #region MenuMain
            if (levelIndex == MenuMain)
            {
                if (level.MouseBox.Intersects(level.NewGameOuterBox))
                {
                    if(level.MouseBox.Intersects(level.NewGameButtonBox) == false)
                        sfxTimer = 0;
                    p = level.GetCoords("Tiles/Buttons/PlayButton");
                    level.tiles[p.X, p.Y].Texture = Content.Load<Texture2D>("Tiles/Buttons/PlayButton");
                    if (level.MouseBox.Intersects(level.NewGameButtonBox))
                    {
                        sfxTimer++;
                        if(sfxTimer <= 1 && isSFXOn)
                            mouseHover.Play();
                        level.tiles[p.X, p.Y].Texture = Content.Load<Texture2D>("Tiles/Buttons/PlayButtonHover");
                        if (ms.LeftButton == ButtonState.Pressed)
                        {
                            level.tiles[p.X, p.Y].Texture = Content.Load<Texture2D>("Tiles/Buttons/PlayButtonClick");
                            if(isSFXOn)
                                mouseClick.Play();
                            System.Threading.Thread.Sleep(200);
                            levelIndex = MenuPlay - 1; // Choose a level to play
                            LoadNextLevel();
                        }
                    }
                }

                if (level.MouseBox.Intersects(level.ExitOuterBox))
                {
                    if (level.MouseBox.Intersects(level.ExitButtonBox) == false)
                        sfxTimer = 0;
                    p = level.GetCoords("Tiles/Buttons/ExitButton");
                    level.tiles[p.X, p.Y].Texture = Content.Load<Texture2D>("Tiles/Buttons/ExitButton");
                    if (level.MouseBox.Intersects(level.ExitButtonBox))
                    {
                        sfxTimer++;
                        if (sfxTimer <= 1 && isSFXOn)
                            mouseHover.Play();
                        level.tiles[p.X, p.Y].Texture = Content.Load<Texture2D>("Tiles/Buttons/ExitButtonHover");
                        if (ms.LeftButton == ButtonState.Pressed)
                        {
                            if(isSFXOn)
                                mouseClick.Play();
                            level.tiles[p.X, p.Y].Texture = Content.Load<Texture2D>("Tiles/Buttons/ExitButtonClick");
                            System.Threading.Thread.Sleep(200);
                            Exit();
                        }
                    }
                }
                
                if (level.MouseBox.Intersects(level.HelpOuterBox))
                {
                    if (level.MouseBox.Intersects(level.HelpButtonBox) == false)
                        sfxTimer = 0;
                    p = level.GetCoords("Tiles/Buttons/HelpButton");
                    level.tiles[p.X, p.Y].Texture = Content.Load<Texture2D>("Tiles/Buttons/HelpButton");
                    if (level.MouseBox.Intersects(level.HelpButtonBox))
                    {
                        sfxTimer++;
                        if (sfxTimer <= 1 && isSFXOn)
                            mouseHover.Play();
                        level.tiles[p.X, p.Y].Texture = Content.Load<Texture2D>("Tiles/Buttons/HelpButtonHover");
                        if (ms.LeftButton == ButtonState.Pressed)
                        {
                            if(isSFXOn)
                                mouseClick.Play();
                            level.tiles[p.X, p.Y].Texture = Content.Load<Texture2D>("Tiles/Buttons/HelpButtonClick");
                            System.Threading.Thread.Sleep(200);
                            levelIndex = MenuHelp-1;
                            LoadNextLevel();
                        }
                    }
                }

                if (level.MouseBox.Intersects(level.SettingsOuterBox))
                {
                    if (level.MouseBox.Intersects(level.SettingsButtonBox) == false)
                        sfxTimer = 0;
                    p = level.GetCoords("Tiles/Buttons/SettingsButton");
                    level.tiles[p.X, p.Y].Texture = Content.Load<Texture2D>("Tiles/Buttons/SettingsButton");
                    if (level.MouseBox.Intersects(level.SettingsButtonBox))
                    {
                        sfxTimer++;
                        if (sfxTimer <= 1 && isSFXOn)
                            mouseHover.Play();
                        level.tiles[p.X, p.Y].Texture = Content.Load<Texture2D>("Tiles/Buttons/SettingsButtonHover");
                        if (ms.LeftButton == ButtonState.Pressed)
                        {
                            if(isSFXOn)
                                mouseClick.Play();
                            level.tiles[p.X, p.Y].Texture = Content.Load<Texture2D>("Tiles/Buttons/SettingsButtonClick");
                            System.Threading.Thread.Sleep(200);
                            levelIndex = MenuSettings-1;
                            LoadNextLevel();
                        }
                    }
                }
            }//Levelindex == 0
            #endregion

            #region MenuPlay
            if (levelIndex == MenuPlay)
            {               
                // roll through level buttons
                for (int i = 0; i < level.MenuItemList.Count; i++)
                {
                    if (level.MouseBox.Intersects(level.MenuItemList[i].getOuterBoundingBox()))
                    {
                        level.tiles[16, (i + 1) * 2].Texture = Content.Load<Texture2D>(level.MenuItemList[i].LoadNormalImage());
                    }
                    if (level.MouseBox.Intersects(level.MenuItemList[i].getInnerBoundingBox()))
                    {
                        level.tiles[16, (i + 1) * 2].Texture = Content.Load<Texture2D>(level.MenuItemList[i].LoadHoverImage());
                        if (ms.LeftButton == ButtonState.Pressed)
                        {
                            level.tiles[16, (i + 1) * 2].Texture = Content.Load<Texture2D>(level.MenuItemList[i].LoadClickImage());
                            if(isSFXOn)
                                mouseClick.Play();
                            System.Threading.Thread.Sleep(200);
                            levelIndex = level.MenuItemList[i].getValue() - 1;
                            LoadNextLevel();
                        }
                    }
                }

                p = level.GetCoords("Tiles/Buttons/MenuPlay/NewGameButton");
                if(p != new Point())
                    level.tiles[p.X, p.Y].Texture = Content.Load<Texture2D>("Tiles/Buttons/MenuPlay/NewGameButton");
                
                if (level.NewGameButtonBox.Contains(level.MouseBox) == false &&
                    level.NewGameOuterBox.Contains(level.MouseBox))
                    sfxTimer = 0;

                if (level.NewGameButtonBox.Contains(level.MouseBox))
                {
                    sfxTimer++;
                    if (sfxTimer <= 1 && isSFXOn)
                        mouseHover.Play();

                    level.tiles[p.X, p.Y].Texture = Content.Load<Texture2D>("Tiles/Buttons/MenuPlay/NewGameButtonHover");
                    if (ms.LeftButton == ButtonState.Pressed)
                    {
                        level.tiles[p.X, p.Y].Texture = Content.Load<Texture2D>("Tiles/Buttons/MenuPlay/NewGameButtonClick");
                        if(isSFXOn)
                            mouseClick.Play();
                        System.Threading.Thread.Sleep(200);
                        level.WorldSelected(1);

                    }
                }

                
                if (!w2unlocked)
                {
                    p = level.GetCoords("Tiles/Buttons/MenuPlay/2Locked");
                    if (p != new Point())
                        level.tiles[p.X, p.Y].Texture = Content.Load<Texture2D>("Tiles/Buttons/MenuPlay/2Locked");
                }
                else
                {
                    p = level.GetCoords("Tiles/Buttons/MenuPlay/2");
                    if (p != new Point()) 
                        level.tiles[p.X, p.Y].Texture = Content.Load<Texture2D>("Tiles/Buttons/MenuPlay/2");

                    if (level.MouseBox.Intersects(level.World2OuterBox) == true  &&
                        level.MouseBox.Intersects(level.World2Box) == false)
                        sfxTimer = 0;

                    if (level.MouseBox.Intersects(level.World2Box))
                    {
                        sfxTimer++;
                        if (sfxTimer <= 1 && isSFXOn)
                            mouseHover.Play();

                        level.tiles[p.X, p.Y].Texture = Content.Load<Texture2D>("Tiles/Buttons/MenuPlay/2Hover");
                        if (ms.LeftButton == ButtonState.Pressed)
                        {
                            if(isSFXOn)
                                mouseClick.Play();
                            level.tiles[p.X, p.Y].Texture = Content.Load<Texture2D>("Tiles/Buttons/MenuPlay/2Click");
                            System.Threading.Thread.Sleep(200);
                            level.WorldSelected(2);
                        }
                    }
                }

                
                
                if (!w3unlocked)
                {
                    p = level.GetCoords("Tiles/Buttons/MenuPlay/3Locked");
                    if (p != new Point()) 
                        level.tiles[p.X, p.Y].Texture = Content.Load<Texture2D>("Tiles/Buttons/MenuPlay/3Locked");
                }
                else
                {
                    p = level.GetCoords("Tiles/Buttons/MenuPlay/3");
                    if (p != new Point()) 
                        level.tiles[p.X, p.Y].Texture = Content.Load<Texture2D>("Tiles/Buttons/MenuPlay/3");

                    if (level.MouseBox.Intersects(level.World3OuterBox) == true &&
                        level.MouseBox.Intersects(level.World3Box) == false)
                        sfxTimer = 0;
                    
                    if (level.MouseBox.Intersects(level.World3Box))
                    {
                        sfxTimer++;
                        if (sfxTimer <= 1 && isSFXOn)
                            mouseHover.Play();

                        level.tiles[p.X, p.Y].Texture = Content.Load<Texture2D>("Tiles/Buttons/MenuPlay/3Hover");
                        if (ms.LeftButton == ButtonState.Pressed)
                        {
                            if(isSFXOn)
                                mouseClick.Play();
                            level.tiles[p.X, p.Y].Texture = Content.Load<Texture2D>("Tiles/Buttons/MenuPlay/3Click");
                            System.Threading.Thread.Sleep(200);
                            level.WorldSelected(3);
                        }
                    }
                }                        
                
                if (!w4unlocked)
                {
                    p = level.GetCoords("Tiles/Buttons/MenuPlay/4Locked");
                    if (p != new Point()) 
                        level.tiles[p.X, p.Y].Texture = Content.Load<Texture2D>("Tiles/Buttons/MenuPlay/4Locked");
                }
                else
                {
                    p = level.GetCoords("Tiles/Buttons/MenuPlay/4");
                    if (p != new Point()) 
                        level.tiles[p.X, p.Y].Texture = Content.Load<Texture2D>("Tiles/Buttons/MenuPlay/4");

                    if (level.MouseBox.Intersects(level.World4OuterBox) == true &&
                        level.MouseBox.Intersects(level.World4Box) == false)
                        sfxTimer = 0;
                    
                    if (level.MouseBox.Intersects(level.World4Box))
                    {
                        sfxTimer++;
                        if (sfxTimer <= 1 && isSFXOn)
                            mouseHover.Play();

                        level.tiles[p.X, p.Y].Texture = Content.Load<Texture2D>("Tiles/Buttons/MenuPlay/4Hover");
                        if (ms.LeftButton == ButtonState.Pressed)
                        {
                            if(isSFXOn)
                                mouseClick.Play();
                            level.tiles[p.X, p.Y].Texture = Content.Load<Texture2D>("Tiles/Buttons/MenuPlay/4Click");
                            System.Threading.Thread.Sleep(200);
                            level.WorldSelected(4);
                        }
                    }
                }
                
               
                if (!w5unlocked)
                {
                    p = level.GetCoords("Tiles/Buttons/MenuPlay/5Locked");
                    if (p != new Point()) 
                        level.tiles[p.X, p.Y].Texture = Content.Load<Texture2D>("Tiles/Buttons/MenuPlay/5Locked");
                }
                else if (w5unlocked == true)
                {
                    p = level.GetCoords("Tiles/Buttons/MenuPlay/5");
                    if (p != new Point()) 
                        level.tiles[p.X, p.Y].Texture = Content.Load<Texture2D>("Tiles/Buttons/MenuPlay/5");

                    if (level.MouseBox.Intersects(level.World5OuterBox) == true &&
                        level.MouseBox.Intersects(level.World5Box) == false)
                        sfxTimer = 0;
                    
                    if (level.MouseBox.Intersects(level.World5Box))
                    {
                        sfxTimer++;
                        if (sfxTimer <= 1 && isSFXOn)
                            mouseHover.Play();

                        level.tiles[p.X, p.Y].Texture = Content.Load<Texture2D>("Tiles/Buttons/MenuPlay/5Hover");
                        if (ms.LeftButton == ButtonState.Pressed)
                        {
                            if(isSFXOn)
                                mouseClick.Play();
                            level.tiles[p.X, p.Y].Texture = Content.Load<Texture2D>("Tiles/Buttons/MenuPlay/5Click");
                            System.Threading.Thread.Sleep(200);
                            level.WorldSelected(5);
                        }
                    }
                }
                
                if (ms.LeftButton == ButtonState.Pressed &&
                    level.MouseBox.Intersects(level.NewGameButtonBox) == false &&
                    level.MouseBox.Intersects(level.World2Box) == false &&
                    level.MouseBox.Intersects(level.World3Box) == false &&
                    level.MouseBox.Intersects(level.World4Box) == false &&
                    level.MouseBox.Intersects(level.World5Box) == false)
                    level.WorldSelected(0);
            }
            else { }
            #endregion

            #region Back / Accept / Arrow Buttons
            if (levelIndex.Equals(MenuHelp) || levelIndex.Equals(MenuSettings) || levelIndex.Equals(MenuPlay))
            {
                if (level.MouseBox.Intersects(level.BackOuterBox))
                {
                    if (level.MouseBox.Intersects(level.BackButtonBox) == false)
                        sfxTimer = 0;
                    p = level.GetCoords("Tiles/Buttons/BackButton");
                    level.tiles[p.X, p.Y].Texture = Content.Load<Texture2D>("Tiles/Buttons/BackButton");
                    if (level.MouseBox.Intersects(level.BackButtonBox))
                    {
                        sfxTimer++;
                        if (sfxTimer <= 1 && isSFXOn)
                            mouseHover.Play();
                        level.tiles[p.X, p.Y].Texture = Content.Load<Texture2D>("Tiles/Buttons/BackButtonHover");
                        if (ms.LeftButton == ButtonState.Pressed)
                        {
                            level.tiles[p.X, p.Y].Texture = Content.Load<Texture2D>("Tiles/Buttons/BackButtonClick");
                            if(isSFXOn)
                                mouseClick.Play();
                            System.Threading.Thread.Sleep(200);
                            levelIndex = -1;
                            LoadNextLevel();
                        }
                    }
                }
                
                if (level.MouseBox.Intersects(level.AcceptOuterBox))
                {
                    if (level.MouseBox.Intersects(level.AcceptButtonBox) == false)
                        sfxTimer = 0;
                    p = level.GetCoords("Tiles/Buttons/AcceptButton");
                    level.tiles[p.X, p.Y].Texture = Content.Load<Texture2D>("Tiles/Buttons/AcceptButton");
                    if (level.MouseBox.Intersects(level.AcceptButtonBox))
                    {
                        sfxTimer++;
                        if (sfxTimer <= 1 && isSFXOn)
                            mouseHover.Play();
                        level.tiles[p.X, p.Y].Texture = Content.Load<Texture2D>("Tiles/Buttons/AcceptButtonHover");
                        if (ms.LeftButton == ButtonState.Pressed)
                        {
                            level.tiles[p.X, p.Y].Texture = Content.Load<Texture2D>("Tiles/Buttons/AcceptButtonClick");
                            if (graphics.IsFullScreen != level.tempfullscreen)
                            {
                                graphics.ToggleFullScreen();
                                graphics.IsFullScreen = level.tempfullscreen;
                            }
                            if (level.tempmusic == false)
                                isMusicOn = false;
                            else if (level.tempmusic == true)
                                isMusicOn = true;
                            if (!level.tempSFX)
                                isSFXOn = false;
                            else if (level.tempSFX)
                                isSFXOn = true;
                            if(isSFXOn)
                                mouseClick.Play();
                            System.Threading.Thread.Sleep(200);
                            levelIndex = -1; //0.txt = Menu
                            LoadNextLevel();
                        }
                    }
                }

                if (level.MouseBox.Intersects(level.NextArrowBox))
                {
                    if ((ms.LeftButton == ButtonState.Pressed) && (level.HelpTextListIndex < level.HelpTextList.Count - 1))
                    {
                        if(isSFXOn)
                            mouseClick.Play();
                        System.Threading.Thread.Sleep(200);
                        level.HelpTextListIndex++;
                    }
                }
                if (level.MouseBox.Intersects(level.BackArrowBox) && level.HelpTextListIndex > 0)
                {
                    if (ms.LeftButton == ButtonState.Pressed)
                    {
                        if (isSFXOn)
                            mouseClick.Play();
                        System.Threading.Thread.Sleep(200);
                        level.HelpTextListIndex = level.HelpTextListIndex - 1;
                    }
                }
            #endregion
            
            #region Options Menu
            if (level.MouseBox.Intersects(level.FullScreenBox) && ms.LeftButton == ButtonState.Pressed)
            {
                if(level.GetCoords("Tiles/Buttons/FullScreenOn") != new Point(0,0))
                    p = level.GetCoords("Tiles/Buttons/FullScreenOn");
                else
                    p = level.GetCoords("Tiles/Buttons/FullScreenOff");
                System.Threading.Thread.Sleep(200);
                if (level.tempfullscreen == true)
                {
                    level.tempfullscreen = false;
                    level.tiles[p.X, p.Y].Texture = Content.Load<Texture2D>("Tiles/Buttons/FullScreenOff");
                }
                else if (level.tempfullscreen == false)
                {
                    level.tempfullscreen = true;
                    level.tiles[p.X, p.Y].Texture = Content.Load<Texture2D>("Tiles/Buttons/FullScreenOn");
                }
                if (isSFXOn)
                    mouseClick.Play();
                System.Threading.Thread.Sleep(200);
            }
            if (level.MouseBox.Intersects(level.MusicBox) && ms.LeftButton == ButtonState.Pressed)
            {
                if (level.GetCoords("Tiles/Buttons/MusicOn") != new Point(0, 0))
                    p = level.GetCoords("Tiles/Buttons/MusicOn");
                else
                    p = level.GetCoords("Tiles/Buttons/MusicOff");
                if (level.tempmusic == false)
                {
                    level.tempmusic = true;
                    level.tiles[p.X, p.Y].Texture = Content.Load<Texture2D>("Tiles/Buttons/MusicOn");
                }
                else if (level.tempmusic == true)
                {
                    level.tempmusic = false;
                    level.tiles[p.X, p.Y].Texture = Content.Load<Texture2D>("Tiles/Buttons/MusicOff");
                }
                if (isSFXOn)
                    mouseClick.Play();
                System.Threading.Thread.Sleep(200);
            }
            if (level.MouseBox.Intersects(level.SFXBox) && ms.LeftButton == ButtonState.Pressed)
            {
                if (level.GetCoords("Tiles/Buttons/SFXOn") != new Point(0, 0))
                    p = level.GetCoords("Tiles/Buttons/SFXOn");
                else
                    p = level.GetCoords("Tiles/Buttons/SFXOff");
                if (level.tempSFX == false)
                {
                    level.tempSFX = true;
                    level.tiles[p.X, p.Y].Texture = Content.Load<Texture2D>("Tiles/Buttons/SFXOn");
                }
                else if (level.tempSFX == true)
                {
                    level.tempSFX = false;
                    level.tiles[p.X, p.Y].Texture = Content.Load<Texture2D>("Tiles/Buttons/SFXOff");
                }
                if (isSFXOn)
                    mouseClick.Play();
                System.Threading.Thread.Sleep(200);
            }
            }//LeveIndex == Settings || Help || Play
            #endregion

        } //CheckMouseOver()

        /// <summary>
        /// Determines which background music to play based on which world the player is in
        /// </summary>
        public void PlayMusic()
        {
            if(isMusicOn)
            {
                #region On entry to new world, force start
                if (!inWorldLast.Equals(inWorld))
                {
                    if (levelIndex.Equals(MenuMain))
                        MediaPlayer.Play(Content.Load<Song>("Sounds/Music/Menu"));
                    if (inWorld.Equals(1))
                        MediaPlayer.Play(Content.Load<Song>("Sounds/Music/1"));
                    if (inWorld.Equals(2))
                        MediaPlayer.Play(Content.Load<Song>("Sounds/Music/2"));
                    if (inWorld.Equals(3))
                        MediaPlayer.Play(Content.Load<Song>("Sounds/Music/3"));
                    if (inWorld.Equals(4))
                        MediaPlayer.Play(Content.Load<Song>("Sounds/Music/4"));
                    if (inWorld.Equals(5))
                        MediaPlayer.Play(Content.Load<Song>("Sounds/Music/5"));
                    MediaPlayer.IsRepeating = true;
                }
                #endregion
                #region On entry to new level
                if (MediaPlayer.State != MediaState.Playing)
                  //  (levelIndex.Equals(MenuMain) ^ levelIndex.Equals(w1l1) ^ levelIndex.Equals(w2l1) ^
                   // levelIndex.Equals(w3l1) ^  levelIndex.Equals(w4l1) ^ levelIndex.Equals(w5l1)))
                    {
                    // THEN let there be musics!!
                    if (levelIndex.Equals(MenuMain))
                        MediaPlayer.Play(Content.Load<Song>("Sounds/Music/Menu"));
                    if (inWorld.Equals(1))
                        MediaPlayer.Play(Content.Load<Song>("Sounds/Music/1"));
                    if (inWorld.Equals(2))
                        MediaPlayer.Play(Content.Load<Song>("Sounds/Music/2"));
                    if (inWorld.Equals(3))
                        MediaPlayer.Play(Content.Load<Song>("Sounds/Music/3"));
                    if (inWorld.Equals(4))
                        MediaPlayer.Play(Content.Load<Song>("Sounds/Music/4"));
                    if (inWorld.Equals(5))
                        MediaPlayer.Play(Content.Load<Song>("Sounds/Music/5"));
                    MediaPlayer.IsRepeating = true;
                    }
                #endregion
            } // If isMusicOn
            else
                MediaPlayer.Stop();
        } // End PlayMusic()

        /// <summary>
        /// Called by HandleInput(), Changes which 'element' the player changes into
        /// </summary>
        public void ChangeWithKeyPress()
        {
            if (keyboardState.IsKeyDown(Keys.D1) || keyboardState.IsKeyDown(Keys.NumPad1))
            {
                level.Player.DoTransform(0);

            }
            if ((keyboardState.IsKeyDown(Keys.D2) || keyboardState.IsKeyDown(Keys.NumPad2)) && level.Player.list.Elements[1].unlocked == true)
            {
                level.Player.DoTransform(1);

            }
            if ((keyboardState.IsKeyDown(Keys.D3) || keyboardState.IsKeyDown(Keys.NumPad3)) && level.Player.list.Elements[2].unlocked == true)
            {
                level.Player.DoTransform(2);

            }
            if ((keyboardState.IsKeyDown(Keys.D4) || keyboardState.IsKeyDown(Keys.NumPad4)) && level.Player.list.Elements[3].unlocked == true)
            {
                level.Player.DoTransform(3);

            }
            if ((keyboardState.IsKeyDown(Keys.D5) || keyboardState.IsKeyDown(Keys.NumPad5)) && level.Player.list.Elements[4].unlocked == true)
            {
                level.Player.DoTransform(4);

            }
            if ((keyboardState.IsKeyDown(Keys.D6) || keyboardState.IsKeyDown(Keys.NumPad6)) && level.Player.list.Elements[5].unlocked == true)
            {
                level.Player.DoTransform(5);

            }
        }

        /// <summary>
        ///  Simple two lines of code that draws a copy of the text slightly behind it, and in black, so it gives the perception of shadow
        /// </summary>
        /// <param name="font">Font to draw in</param>
        /// <param name="value">String of text to be printed</param>
        /// <param name="position">Where it is to be drawn</param>
        /// <param name="color">Colour of main text</param>
        private void DrawShadowedString(SpriteFont font, string value, Vector2 position, Color color)
        {
            spriteBatch.DrawString(font, value, position + new Vector2(1.0f, 1.0f), Color.Black);
            spriteBatch.DrawString(font, value, position, color);
        }

    }
}
