using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace TheDivineAdventure
{
    public class DeathScene : Scene
    {
        private Texture2D screen, emberSheet01;
        private AnimatedSprite[] titleEmbers;
        private Button restart, levSelect, menu, quit;
        public float fadeIn;

        public DeathScene(SpriteBatch sb, GraphicsDeviceManager graph, Game1 parent, ContentManager cont) : base(sb, graph, parent, cont)
        {

        }

        public override void Initialize()
        {
            base.Initialize();
            parent.showCursor = true;

            fadeIn = 0;

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
            restart = new Button(new Vector2(782, 440), new Vector2(350, 90), parent.currentScreenScale);
            levSelect = new Button(new Vector2(782, 530), new Vector2(650, 90), parent.currentScreenScale);
            menu = new Button(new Vector2(782, 620), new Vector2(650, 90), parent.currentScreenScale);
            quit = new Button(new Vector2(782, 710), new Vector2(650, 90), parent.currentScreenScale);

        }

        public override void LoadContent()
        {
            base.LoadContent();
            emberSheet01 = Content.Load<Texture2D>("TEX_EmberSheet01");
            screen = Content.Load<Texture2D>("TEX_DeathScene");
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            fadeIn += 0.02f;
            if (fadeIn < 1) return;
            if(mouseState.LeftButton == ButtonState.Pressed)
            {
                if (parent.lastMouseState.LeftButton != ButtonState.Pressed)
                    Game1.gameSounds[1].Play(volume: GameSettings.Settings["SFXVolume"], pitch: 0.0f, pan: 0.0f);
                if (restart.IsPressed())
                {
                    fadeIn = 0;
                    parent.currentScene = "PLAY";
                    parent.playScene.Initialize();
                    return;
                }
                if (levSelect.IsPressed())
                {
                    fadeIn = 0;
                    parent.currentScene = "LEVEL_SELECT";
                    MediaPlayer.Stop();
                    MediaPlayer.Play(parent.gameTheme);
                    parent.levelSelectScene.Initialize();
                    return;
                }
                if (menu.IsPressed())
                {
                    fadeIn = 0;
                    parent.currentScene = "TITLE_SCREEN";
                    MediaPlayer.Stop();
                    MediaPlayer.Play(parent.gameTheme);
                    parent.titleScene.Initialize();
                    return;
                }
                if (quit.IsPressed())
                {
                    parent.Exit();
                }

            }
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
            //draw menu
            _spriteBatch.Begin();
            _spriteBatch.Draw(screen, Vector2.Zero, null, new Color(Color.White, fadeIn), 0, Vector2.Zero, parent.currentScreenScale, SpriteEffects.None, 0);
            if (fadeIn < 1)
            {
                _spriteBatch.End();
                return;
            }
            //draw embers
            foreach (AnimatedSprite ember in titleEmbers)
            {
                if (ember.Frame == 0)
                {
                    ember.Pos = new Vector2((rand.Next(1920)) * parent.currentScreenScale.X, rand.Next(450, 750) * parent.currentScreenScale.Y);
                    ember.Scale = 1 - (rand.Next(-200, 50) / 100f);
                }
                ember.Draw(_spriteBatch, parent.currentScreenScale);
            }
            _spriteBatch.End();
        }


    }
}
