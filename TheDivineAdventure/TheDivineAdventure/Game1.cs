///Course:      CSC 495
///Project:     Final Project
///Creation:    4/14/22
///Completion:  
///Authors:     Adkins, Christopher 
///             Blankenship, Sean A.
///             Michael, Hayden T.
///             Reed, Lucas
///Description: This program is our submission for the final project in CSC 316.

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Audio;
using System.Collections.Generic;
using System;

namespace TheDivineAdventure
{
    public class Game1 : Game
    {
        ///////////////
        ///VARIABLES///
        ///////////////

        // Essential
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        // Render Settings
        public Vector2 currentScreenScale;

        //Scenes
        public TitleScene titleScene;
        public CharacterSelectScene characterSelectScene;
        public PlayScene playScene;
        public LevelEndScene levelEnd1;
        public CreditsScene creditsScene;
        public PauseScene pauseScene;
        public SettingsScene settingsScene;
        public LevelSelectScene levelSelectScene;
        public DeathScene lostScene;


        //Menu Navigation
        public static readonly string[] SCENES = { "TITLE_SCREEN", "LEVEL_SELECT", "CHARACTER_SELECT",
            "SCOREBOARD", "SETTINGS", "PLAYING", "IS_PAUSED", "CREDITS", "IS_DEAD", "LEVEL_END"};
        public string currentScene, lastScene;
        public MouseState mouseState,lastMouseState;
        public KeyboardState keyboardState, lastKeyboard;
        public GameWindow _gameWindow;
        public int level;

        //player information to traverse scenes
        public string playerRole;


        // 2D Assets
        public SpriteFont BigFont, creditsFont, smallFont;
        public Texture2D cursor, cursorPress, whiteBox;
        public bool showCursor;

        //game world variables
        public readonly float gravity = 0.1f;

        // Font Color
        public Color textGold;

        // Sound
        public Song gameTheme;
        public static  List<SoundEffect> gameSounds = new List<SoundEffect>();
        public static float volume = 1;

        //Random class
        public Random rand;

        public string[,] settings;



        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this)
            {
                IsFullScreen = false
            };
            Content.RootDirectory = "Content";

            Window.IsBorderless = true;
            IsMouseVisible = false;
        }

        protected override void Initialize()
        {
            base.Initialize();

            //create random object
            rand = new Random();

            //Rename window
            Window.Title = "The Divine Adventure";

            #region GAME SETTINGS---------------------
            //switch commenting to use saved settings
            if (GameSettings.HasSettings())
            {
                //get settings
                settings = GameSettings.ReadSettings();

                //set resolution to preferrence
                _graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
                _graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;

                //set preferred window mode
                switch (Int32.Parse(settings[2, 1]))
                {
                    case 1:
                        Window.IsBorderless = true;
                        _graphics.IsFullScreen = false;
                        break;
                    case 2:
                        Window.IsBorderless = true;
                        _graphics.IsFullScreen = true;
                        break;
                    default:
                        Window.IsBorderless = false;
                        _graphics.IsFullScreen = false;
                        break;
                }

                MediaPlayer.Volume = float.Parse(settings[4, 1]) * float.Parse(settings[5, 1]);
                MediaPlayer.IsRepeating = true;
                Player.volume = float.Parse(settings[4, 1]) * float.Parse(settings[6, 1]);
                Enemy.volume = float.Parse(settings[4, 1]) * float.Parse(settings[6, 1]);
                volume = float.Parse(settings[4, 1]) * float.Parse(settings[6, 1]);

            }
            //// Set initial screen size
            //// (Determine size of display)
            int desktop_width = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            int desktop_height = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;

            //// (Apply the determined size)
            _graphics.PreferredBackBufferWidth = desktop_width;
            _graphics.PreferredBackBufferHeight = desktop_height;


            _graphics.ApplyChanges();

            #endregion -- Settings End --
           
            //set game window
            _gameWindow = Window;

            // Set screen scale to determine size of UI
            currentScreenScale = new Vector2(_graphics.PreferredBackBufferWidth / 1920f, _graphics.PreferredBackBufferHeight / 1080f);

            //create Scenes
            titleScene = new TitleScene(_spriteBatch, _graphics, this, Content);
            characterSelectScene = new CharacterSelectScene(_spriteBatch, _graphics, this, Content);
            creditsScene = new CreditsScene(_spriteBatch, _graphics, this, Content);
            pauseScene = new PauseScene(_spriteBatch, _graphics, this, Content);
            settingsScene = new SettingsScene(_spriteBatch, _graphics, this, Content);
            levelSelectScene = new LevelSelectScene(_spriteBatch, _graphics, this, Content);
            levelEnd1 = new LevelEndScene (_spriteBatch, _graphics, this, Content);
            lostScene = new DeathScene(_spriteBatch, _graphics, this, Content);


            //initialize title menu
            titleScene.Initialize();

            currentScene = "TITLE";
        }

        //most content loading has been offloaded to neaten things up a bit. I'm keeping some things here that I haven't
        //moved or are needed in a bunch of locations (e.g.the cursor)
        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // Load fonts
            BigFont = Content.Load<SpriteFont>("BigFont");
            creditsFont = Content.Load<SpriteFont>("CreditsFont");
            smallFont = Content.Load<SpriteFont>("SmallFont");

            //set font color
            textGold = new Color(175, 127, 16);

            // Load sounds
            Content.RootDirectory = @"Content\Music";
            gameTheme = Content.Load<Song>("MUS_MainTheme");

            Content.RootDirectory = @"Content\SoundFX";
            gameSounds.Add(Content.Load<SoundEffect>("SND_ButtonClick01"));     //0
            gameSounds.Add(Content.Load<SoundEffect>("SND_ButtonClick02"));     //1
            gameSounds.Add(Content.Load<SoundEffect>("SND_Menu_Open"));         //2
            gameSounds.Add(Content.Load<SoundEffect>("SND_Lava"));              //3
            gameSounds.Add(Content.Load<SoundEffect>("SND_Doors_L1"));          //4
            gameSounds.Add(Content.Load<SoundEffect>("SND_DeathSound"));        //5
            gameSounds.Add(Content.Load<SoundEffect>("SND_DeathSound"));        //6
            gameSounds.Add(Content.Load<SoundEffect>("SND_ButtonPress_L1"));    //7
            Content.RootDirectory = "Content";

            //Cursor texture
            cursor = Content.Load<Texture2D>("TEX_cursor");
            cursorPress = Content.Load<Texture2D>("TEX_cursorClicked");
        }

        protected override void Update(GameTime gameTime)
        {
            mouseState = Mouse.GetState();
            keyboardState = Keyboard.GetState();
            //Update based on current scene
            //game info was moved to the PlayingScene function below
            switch (currentScene)
            {
                case "TITLE":
                    titleScene.Update(gameTime);
                    break;
                case "LEVEL_SELECT":
                    levelSelectScene.Update(gameTime);
                    break;
                case "CHARACTER_SELECT":
                    characterSelectScene.Update(gameTime);
                    break;
                case "SETTINGS":
                    settingsScene.Update(gameTime);
                    break;
                case "PLAY":
                    playScene.Update(gameTime);
                    break;
                case "PAUSE":
                    pauseScene.Update(gameTime);
                    break;
                case "CREDITS":
                    creditsScene.Update(gameTime);
                    break;
                case "LEVEL_END":
                    levelEnd1.Update(gameTime);
                    break;
                case "IS_DEAD":
                    playScene.Update(gameTime);
                    lostScene.Update(gameTime);
                    break;
                default:
                    titleScene.Update(gameTime);
                    break;
            }

            //DEBUG: FPS COUNTER IN DEBUG LOG
            //Debug.WriteLine(1 / gameTime.ElapsedGameTime.TotalSeconds);

            lastKeyboard = keyboardState;
            lastMouseState = mouseState;
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            // Graphics device settings
            GraphicsDevice.Clear(new Color(40, 20, 20));

            //Update based on current scene
            switch (currentScene)
            {
                case "TITLE":
                    titleScene.Draw(gameTime);
                    break;
                case "LEVEL_SELECT":
                    levelSelectScene.Draw(gameTime);
                    break;
                case "CHARACTER_SELECT":
                    characterSelectScene.Draw(gameTime);
                    break;
                case "SETTINGS":
                    settingsScene.Draw(gameTime);
                    break;
                case "PLAY":
                    playScene.Draw(gameTime);
                    break;
                case "LEVEL_END":
                    levelEnd1.Draw(gameTime);
                    break;
                case "PAUSE":
                    playScene.Draw(gameTime);
                    GraphicsDevice.DepthStencilState = DepthStencilState.Default;
                    GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
                    pauseScene.Draw(gameTime);
                    GraphicsDevice.DepthStencilState = DepthStencilState.Default;
                    GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
                    break;
                case "IS_DEAD":
                    playScene.Draw(gameTime);
                    GraphicsDevice.DepthStencilState = DepthStencilState.Default;
                    GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
                    lostScene.Draw(gameTime);
                    GraphicsDevice.DepthStencilState = DepthStencilState.Default;
                    GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
                    break;
                case "CREDITS":
                    creditsScene.Draw(gameTime);
                    break;
                default:
                    titleScene.Draw(gameTime);
                    break;
            }

            //draw cursor
            if (showCursor == true)
            {
                GraphicsDevice.DepthStencilState = DepthStencilState.Default;
                GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
                _spriteBatch.Begin();
                _spriteBatch.Draw(cursor, new Vector2(mouseState.X, mouseState.Y), null, Color.White, 0, Vector2.Zero, currentScreenScale, SpriteEffects.None, 0);
                if (Mouse.GetState().LeftButton == ButtonState.Pressed)
                    _spriteBatch.Draw(cursorPress, new Vector2(mouseState.X+2, mouseState.Y+2), null, Color.White, 0, Vector2.Zero, currentScreenScale, SpriteEffects.None, 0);
                _spriteBatch.End();
                GraphicsDevice.DepthStencilState = DepthStencilState.Default;
                GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
            }

            base.Draw(gameTime);
        }

        //Scene Functions
        public void ReloadContent()
        {
            UnloadContent();
            LoadContent();
            currentScreenScale = new Vector2(_graphics.PreferredBackBufferWidth / 1920f, _graphics.PreferredBackBufferHeight / 1080f);
        }

        public void LoadPlayScene()
        {
            playScene = new PlayScene(_spriteBatch, _graphics, this, Content, level);
        }
    }
}