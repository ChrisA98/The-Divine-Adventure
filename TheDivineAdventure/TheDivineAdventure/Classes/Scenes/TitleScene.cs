using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace TheDivineAdventure
{
    public class TitleScene : Scene
    {
        private Texture2D titleScreenBack, TitleScreenFront, distantDemonSheet, titleLightning01, titleLightning02,
            titleLightning03, emberSheet01, titleLava;
        private Button titleStartGame, titleSettings, titleCredits, titleQuitGame;
        private AnimatedSprite[] titleDemons, titleEmbers;
        private bool glowState;
        private int glowRef;

        public TitleScene(SpriteBatch sb, GraphicsDeviceManager graph, Game1 game, ContentManager cont) : base(sb, graph, game, cont)
        {

        }

        public override void Initialize()
        {
            base.Initialize();
            LoadContent();
            //show custom cursor
            parent.showCursor = true;
            //create background demons
            titleDemons = new AnimatedSprite[rand.Next(20, 50)];
            for (int i = 0; i < titleDemons.Length; i++)
            {
                titleDemons[i] = new AnimatedSprite(109, 108, distantDemonSheet, 7);
                titleDemons[i].Pos = new Vector2((-1 * rand.Next(-1920, 1000)) *parent.currentScreenScale.X, rand.Next(600) *parent.currentScreenScale.Y);
                titleDemons[i].Scale = 1 - (rand.Next(50) / 100f);
                titleDemons[i].Tint = new Color(Color.White, titleDemons[i].Scale);
                titleDemons[i].Frame = rand.Next(7);
            }
            //create embers
            titleEmbers = new AnimatedSprite[10];
            for (int i = 0; i < titleEmbers.Length; i++)
            {
                titleEmbers[i] = new AnimatedSprite(174, 346, emberSheet01, 6);
                titleEmbers[i].Pos = new Vector2(rand.Next(1920) *parent.currentScreenScale.X, rand.Next(450, 750) *parent.currentScreenScale.Y);
                titleEmbers[i].Scale = 1 - (rand.Next(-200, 50) / 100f);
                titleEmbers[i].Frame = rand.Next(6);
            }
            glowRef = 0;
            //create menu buttons
            titleStartGame = new Button(new Vector2(247, 686), new Vector2(366, 60),parent.currentScreenScale);
            titleSettings = new Button(new Vector2(247, 750), new Vector2(366, 60),parent.currentScreenScale);
            titleCredits = new Button(new Vector2(247, 812), new Vector2(366, 60),parent.currentScreenScale);
            titleQuitGame = new Button(new Vector2(247, 870), new Vector2(366, 60),parent.currentScreenScale);

            if (MediaPlayer.State != MediaState.Playing)
            {
                MediaPlayer.Stop();
                MediaPlayer.Play(parent.gameTheme);
            }
        }

        public override void LoadContent()
        {
            base.LoadContent();
            titleScreenBack = Content.Load<Texture2D>("TEX_TitleScreen_Back");
            TitleScreenFront = Content.Load<Texture2D>("TEX_TitleScreen_Front");
            distantDemonSheet = Content.Load<Texture2D>("TEX_DemonSpriteSheet");
            titleLightning01 = Content.Load<Texture2D>("TEX_Title_Lightning_01");
            titleLightning02 = Content.Load<Texture2D>("TEX_Title_Lightning_02");
            titleLightning03 = Content.Load<Texture2D>("TEX_Title_Lightning_03");
            emberSheet01 = Content.Load<Texture2D>("TEX_EmberSheet01");
            titleLava = Content.Load<Texture2D>("TEX_Title_LavaGlow");
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            //get mouse clocks and check buttons
            if (parent.mouseState.LeftButton == ButtonState.Pressed && parent.lastMouseState.LeftButton != ButtonState.Pressed)
            {
                Game1.gameSounds[0].Play(volume: GameSettings.Settings["SFXVolume"], pitch: 0.0f, pan: 0.0f);

                if (titleStartGame.IsPressed())
                {
                    parent.currentScene = "LEVEL_SELECT";
                    parent.ReloadContent();
                    parent.levelSelectScene.Initialize();
                    return;
                }
                if (titleSettings.IsPressed())
                {
                    parent.lastScene = "TITLE";
                    parent.currentScene = "SETTINGS";
                    parent.settingsScene.Initialize();
                    return;
                }
                if (titleCredits.IsPressed())
                {
                    parent.currentScene = "CREDITS";
                    parent.creditsScene.Initialize();
                    return;
                }
                if (titleQuitGame.IsPressed())
                {
                    parent.Exit();
                }
            }

        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            _spriteBatch.Begin();
            _spriteBatch.Draw(titleScreenBack, Vector2.Zero, null, Color.White, 0, Vector2.Zero,parent.currentScreenScale, SpriteEffects.None, 0);
            //randomly draw lightning
            if (rand.Next(50) > 48)
            {
                switch (rand.Next(3))
                {
                    case 1:
                        _spriteBatch.Draw(titleLightning01, Vector2.Zero, null, new Color(Color.White, 1 - (rand.Next(50) / 100f)),
                            0, Vector2.Zero,parent.currentScreenScale, SpriteEffects.None, 0);
                        break;
                    case 2:
                        _spriteBatch.Draw(titleLightning02, Vector2.Zero, null, new Color(Color.White, 1 - (rand.Next(50) / 100f)),
                            0, Vector2.Zero,parent.currentScreenScale, SpriteEffects.None, 0);
                        break;
                    case 3:
                        _spriteBatch.Draw(titleLightning03, Vector2.Zero, null, new Color(Color.White, 1 - (rand.Next(50) / 100f)),
                            0, Vector2.Zero,parent.currentScreenScale, SpriteEffects.None, 0);
                        break;
                    default:
                        break;
                }
            }
            //draw background demons
            foreach (AnimatedSprite demon in titleDemons)
            {
                if (demon.Pos.X * parent.currentScreenScale.X > _graphics.PreferredBackBufferWidth)
                {
                    demon.Pos = new Vector2(-1 * rand.Next(1500) *parent.currentScreenScale.X, rand.Next(600) *parent.currentScreenScale.Y);
                    demon.Scale = 1f - (rand.Next(50) / 100f);
                    demon.Tint = new Color(Color.White, demon.Scale);
                }
                else
                {
                    demon.Pos = new Vector2(demon.Pos.X + (demon.Scale * 10 *parent.currentScreenScale.X), demon.Pos.Y);
                }
                demon.Draw(_spriteBatch,parent.currentScreenScale);
            }
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
            //draw title foreground
            _spriteBatch.Draw(TitleScreenFront, Vector2.Zero, null, Color.White, 0, Vector2.Zero,parent.currentScreenScale, SpriteEffects.None, 0);
            //draw lava glow
            if (glowState)
            {
                _spriteBatch.Draw(titleLava, Vector2.Zero, null, new Color(Color.White, glowRef), 0, Vector2.Zero,
                   parent.currentScreenScale, SpriteEffects.None, 1);
                glowRef += 1;
            }
            else
            {
                _spriteBatch.Draw(titleLava, Vector2.Zero, null, new Color(Color.White, glowRef), 0, Vector2.Zero,
                   parent.currentScreenScale, SpriteEffects.None, 1);
                glowRef -= 1;
            }

            if (glowRef >= 255 && glowState!)
                glowState = false;
            else if (glowRef <= 0)
                glowState = true;

            FadeIn();
            _spriteBatch.End();
        }
    }
}
