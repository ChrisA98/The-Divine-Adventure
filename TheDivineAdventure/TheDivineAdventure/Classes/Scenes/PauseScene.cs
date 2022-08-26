using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;

namespace TheDivineAdventure
{
    public class PauseScene : Scene
    {

        private Texture2D pauseMenu, pauseMenuSheet, emberSheet01;

        private AnimatedSprite[] titleEmbers;
        private AnimatedSprite secondaryPauseMenu;
        private Button pauseResume, pauseRestart, pauseSettings, pauseQuitMenu, pauseQuitGame, pauseYes, pauseNo;
        public int pauseIsConfirming;

        public PauseScene(SpriteBatch sb, GraphicsDeviceManager graph, Game1 parent, ContentManager cont) : base(sb, graph, parent, cont)
        {

        }

        //initialize Pause Menu
        public override void Initialize()
        {
            base.Initialize();
            parent.showCursor = true;
            pauseIsConfirming = 0;
            //create embers
            titleEmbers = new AnimatedSprite[30];
            for (int i = 0; i < titleEmbers.Length; i++)
            {
                titleEmbers[i] = new AnimatedSprite(174, 346, emberSheet01, 6);
                titleEmbers[i].Pos = new Vector2(rand.Next(1920) * parent.currentScreenScale.X,
                    rand.Next(450, 750) * parent.currentScreenScale.Y);
                titleEmbers[i].Scale = 1 - (rand.Next(-200, 50) / 100f);
                titleEmbers[i].Frame = rand.Next(6);
            }
        //create buttons
            pauseResume = new Button(new Vector2(665, 304), new Vector2(284, 60), parent.currentScreenScale);
            pauseRestart = new Button(new Vector2(656, 405), new Vector2(204, 60), parent.currentScreenScale);
            pauseSettings = new Button(new Vector2(650, 504), new Vector2(222, 60), parent.currentScreenScale);
            pauseQuitMenu = new Button(new Vector2(579, 596), new Vector2(366, 38), parent.currentScreenScale);
            pauseQuitGame = new Button(new Vector2(620, 700), new Vector2(283, 38),parent.currentScreenScale);
            pauseYes = new Button(new Vector2(1053, 527), new Vector2(134, 55),parent.currentScreenScale);
            pauseNo = new Button(new Vector2(1055, 604), new Vector2(135, 55),parent.currentScreenScale);
        }

        public override void LoadContent()
        {
            base.LoadContent();
            emberSheet01 = Content.Load<Texture2D>("TEX_EmberSheet01");
            pauseMenu = Content.Load<Texture2D>("TEX_Pause_Menu");
            pauseMenuSheet = Content.Load<Texture2D>("TEX_SideMenu_Sheet");
        }

        //update Pause Menu
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            if (Keyboard.GetState().IsKeyDown(Keys.Escape) && parent.lastKeyboard.IsKeyUp(Keys.Escape))
            {
                parent.showCursor = false;
                Mouse.SetPosition(parent.GraphicsDevice.Viewport.Width / 2, parent.GraphicsDevice.Viewport.Height / 2);
                parent.currentScene = "PLAY";
                return;
            }

            //get mouse clocks and check buttons
            if (mouseState.LeftButton == ButtonState.Pressed)
            {
                if (pauseResume.IsPressed())
                {
                    parent.showCursor = false;
                    Mouse.SetPosition(parent.GraphicsDevice.Viewport.Width / 2, parent.GraphicsDevice.Viewport.Height / 2);
                    parent.currentScene = "PLAY";
                    return;
                }
                if (pauseRestart.IsPressed())
                {
                    secondaryPauseMenu = new AnimatedSprite(439, 488, pauseMenuSheet, 4, false);
                    secondaryPauseMenu.Pos = new Vector2(910, 306);
                    secondaryPauseMenu.Framerate = 1.5f;
                    pauseIsConfirming = 1;
                    return;
                }
                if (pauseSettings.IsPressed())
                {
                    parent.lastScene = "PAUSE";
                    parent.currentScene = "SETTINGS";
                    parent.settingsScene.Initialize();
                    return;

                }
                if (pauseQuitMenu.IsPressed())
                {
                    secondaryPauseMenu = new AnimatedSprite(439, 488, pauseMenuSheet, 4, false);
                    secondaryPauseMenu.Pos = new Vector2(910, 306);
                    secondaryPauseMenu.Framerate = 1.5f;
                    pauseIsConfirming = 2;
                    return;
                }
                if (pauseQuitGame.IsPressed())
                {
                    secondaryPauseMenu = new AnimatedSprite(439, 488, pauseMenuSheet, 4, false);
                    secondaryPauseMenu.Pos = new Vector2(910, 306);
                    secondaryPauseMenu.Framerate = 1.5f;
                    pauseIsConfirming = 3;
                }
                if (pauseIsConfirming != 0)
                {
                    if (pauseYes.IsPressed())
                    {
                        switch (pauseIsConfirming)
                        {
                            case 1:
                                parent.currentScene = "PLAY";
                                parent.playScene.Initialize();
                                return;
                            case 2:
                                parent.currentScene = "TITLE";
                                parent.titleScene.Initialize();
                                break;
                            case 3:
                                parent.Exit();
                                break;
                            default:
                                pauseIsConfirming = 0;
                                break;
                        }
                    }
                    if (pauseNo.IsPressed())
                    {
                        pauseIsConfirming = 0;
                    }
                }
            }

        }

        //Draw pause Menu
        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
            //draw menu
            _spriteBatch.Begin();
            _spriteBatch.Draw(pauseMenu, Vector2.Zero, null, Color.White, 0, Vector2.Zero,parent.currentScreenScale, SpriteEffects.None, 0);
            //draw embers
            foreach (AnimatedSprite ember in titleEmbers)
            {
                if (ember.Frame == 0)
                {
                    ember.Pos = new Vector2((rand.Next(1920)) *parent.currentScreenScale.X, rand.Next(450, 750) *parent.currentScreenScale.Y);
                    ember.Scale = 1 - (rand.Next(-200, 50) / 100f);
                }
                ember.Draw(_spriteBatch,parent.currentScreenScale);
            }
            if (pauseIsConfirming != 0)
            {
                secondaryPauseMenu.Draw(_spriteBatch,parent.currentScreenScale);
            }
            _spriteBatch.End();

        }

    }
}
