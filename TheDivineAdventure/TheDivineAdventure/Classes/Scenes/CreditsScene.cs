using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;

using System;

namespace TheDivineAdventure
{
    public class CreditsScene : Scene
    {
        public String[] credits;
        public float creditsRuntime;
        private Texture2D titleBox;

        public CreditsScene(SpriteBatch sb, GraphicsDeviceManager graph, Game1 parent, ContentManager cont) : base(sb, graph, parent, cont)
        {

        }

        //initialize Credits scene
        public override void Initialize()
        {
            base.Initialize();
            LoadContent();
            //hide cursor
            parent.showCursor = false; ;

            credits = new string[]  {"Game Programmers", "       Christopher Adkins", "       Sean Blankenship", "       Hayden Michael", "       Lucas Reed",
                " ","Game Artists", "       2D Assets: Christopher Adkins", "       3D Assets: Sean Blankenship", "       Christopher Adkins",
                " ","Music Used", "       Janus Turning", "              by Shane Ivers",
                " ","Game Testers", "       Christopher Adkins", "       Sean Blankenship", "       Hayden Michael", "       Lucas Reed","       Joseph Park",
                "Noah Adkins",
                " ","Created using MonoGame Framework ",
                " ","Thank you for your time! "};
            creditsRuntime = 820;
        }

        public override void LoadContent()
        {
            base.LoadContent();
            titleBox = Content.Load<Texture2D>("TEX_TitleBox03");
        }

        //update credits scene
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            //return to title screen
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                parent.currentScene = "TITLE";
                parent.titleScene.Initialize();
            }


        }

        //Draw Credits Scene
        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
            _spriteBatch.Begin();
            _spriteBatch.Draw(titleBox,
                new Vector2((_graphics.PreferredBackBufferWidth / 2) - (390 * parent.currentScreenScale.X), creditsRuntime - (800 * parent.currentScreenScale.Y) + _graphics.PreferredBackBufferHeight),
                null, Color.White, 0, Vector2.Zero, parent.currentScreenScale, SpriteEffects.None, 0);
            int cIndex = 0;
            foreach (String c in credits)
            {
                cIndex++;
                //draw each line in the credits array
                if (creditsRuntime + (120f * cIndex * parent.currentScreenScale.Y) + _graphics.PreferredBackBufferHeight
                    < _graphics.PreferredBackBufferHeight &&
                    creditsRuntime + (120f * cIndex * parent.currentScreenScale.Y) + _graphics.PreferredBackBufferHeight > -80)
                    _spriteBatch.DrawString(parent.creditsFont, c,
                    new Vector2(_graphics.PreferredBackBufferWidth * 0.05f, creditsRuntime + (120f * cIndex * parent.currentScreenScale.Y)
                    + _graphics.PreferredBackBufferHeight),
                    parent.textGold, 0f, Vector2.Zero, parent.currentScreenScale, SpriteEffects.None, 1);
            }
            if (creditsRuntime + (120f * credits.Length - 1) + _graphics.PreferredBackBufferHeight < -120f)
            {
                _spriteBatch.End();
                parent.currentScene = "TITLE";
                parent.titleScene.Initialize();
                return;
            }
            creditsRuntime -= 5f * parent.currentScreenScale.Y;
            FadeIn(0.05f);
            _spriteBatch.End();
        }
    }
}
