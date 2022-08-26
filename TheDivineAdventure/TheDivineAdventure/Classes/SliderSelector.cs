using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;

using System;

namespace TheDivineAdventure
{
    class SliderSelector
    {
        private ContentManager Content;
        private float value;
        private Vector2 pos, center,  res, scale, mousePos;
        private Texture2D slider, baseTex;
        private MouseState mouseState, prevMouseState;
        private bool isClicked;
        public SliderSelector(Vector2 origin, Vector2 size, Game1 parent, ContentManager cont)
        {
            value = 0;
            pos = origin* parent.currentScreenScale;
            res = size* parent.currentScreenScale;
            center = pos + (res / 2);
            this.scale = parent.currentScreenScale;
            Content = cont;
            isClicked = false;

            //create black box texture
            slider = Content.Load<Texture2D>("TEX_Slide_Selector");
            baseTex = new Texture2D(parent.GraphicsDevice, 1, 1);
            baseTex.SetData(new[] { Color.White });
            value = 0;
        }

        public void IsPressed()
        {
            mouseState = Mouse.GetState();
            mousePos = new Vector2(mouseState.X, mouseState.Y);

            //get mouse click and hold it until it is released
            if (mouseState.LeftButton == ButtonState.Pressed)
            {
                if (Math.Abs(center.X - mousePos.X) < (res.X / 2f) && Math.Abs(center.Y - mousePos.Y) < 11*scale.Y)
                    isClicked = true;
                else if (prevMouseState.LeftButton == ButtonState.Pressed && isClicked == false)
                    isClicked = false;
            }
            else
            {
                isClicked = false;
            }
            prevMouseState = mouseState;

        }

        public void Update()
        {
            mouseState = Mouse.GetState();
            if (isClicked)
            {
                if (mouseState.X < pos.X + res.X && mouseState.X > pos.X)
                    value = Math.Abs(pos.X - mouseState.X) / res.X;
                if (mouseState.X > pos.X + res.X)
                    value = 1;
                if (mouseState.X < pos.X)
                    value = 0;
            }

        }

        public void Draw(SpriteBatch sb, GameTime gameTime)
        {
            //draw backdrop line
            sb.Draw(baseTex, pos, new Rectangle(0, 0, (int)res.X, (int)res.Y), new Color(Color.Black, 0.2f), 0, Vector2.Zero,
                    1, SpriteEffects.None, 0);
            //draw filled line
            sb.Draw(baseTex, pos, new Rectangle(0, 0, (int)(res.X*value), (int)res.Y), new Color (175, 127, 16), 0, Vector2.Zero,
                    1, SpriteEffects.None, 0);
            //draw icon
            sb.Draw(slider, new Vector2((pos.X+(value*res.X)-(10*scale.X)),pos.Y - (5 * scale.X)), null,
                Color.White, 0, Vector2.Zero, scale, SpriteEffects.None, 0);
        }

        //getter setter methods
        public float Value
        {
            get { return value; }
            set { this.value = value; }
        }

        public bool IsActive
        {
            get { return isClicked; }
        }

    }
}
