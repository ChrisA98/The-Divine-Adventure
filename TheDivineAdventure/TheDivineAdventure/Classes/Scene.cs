using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;

using System;

namespace TheDivineAdventure
{
    public class Scene
    {
        ///////////////
        ///VARIABLES///
        ///////////////

        // Essential
        protected GraphicsDeviceManager _graphics;
        protected ContentManager Content;
        protected SpriteBatch _spriteBatch;
        public Game1 parent;
        protected Texture2D fade;
        protected MouseState mouseState;
        public Random rand;
        protected float startFade;


        public Scene(SpriteBatch sb, GraphicsDeviceManager graph, Game1 game, ContentManager content)
        {
            Content = content;
            _graphics = graph;
            _spriteBatch = sb;
            parent = game;
            rand = new Random();
        }

        public virtual void Initialize()
        {
            // Set screen scale to determine size of UI
            ReloadContent();
            LoadContent();
            startFade = 1f;
        }

        public virtual void LoadContent()
        {
            //procedural texture
            fade = new Texture2D(parent.GraphicsDevice, 1, 1);
            fade.SetData(new[] { Color.Black });
        }

        public virtual void ReloadContent()
        {
            parent.ReloadContent();
        }

        public virtual void Update(GameTime gameTime)
        {
            mouseState = Mouse.GetState();
        }

        public virtual void Draw(GameTime gameTime)
        {


        }

        public virtual void FadeIn(float progress)
        {
            if (startFade >= 0)
            {
                _spriteBatch.Draw(fade, Vector2.Zero, new Rectangle(0, 0, (int)parent.currentScreenScale.X * 1920, (int)parent.currentScreenScale.Y * 1080),
                    new Color(Color.Black, startFade), 0, Vector2.Zero, 1, SpriteEffects.None, 0);
                startFade -= progress;
            }
        }
        public virtual void FadeIn()
        {
            float  progress = 0.05f;
            if (startFade >= 0)
            {
                _spriteBatch.Draw(fade, Vector2.Zero, new Rectangle(0, 0, (int)parent.currentScreenScale.X * 1920, (int)parent.currentScreenScale.Y * 1080),
                    new Color(Color.Black, startFade), 0, Vector2.Zero, 1, SpriteEffects.None, 0);
                startFade -= progress;
            }
        }
    }
}
