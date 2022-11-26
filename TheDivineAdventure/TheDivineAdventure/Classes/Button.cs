using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace TheDivineAdventure
{
    class Button
    {
        private Vector2 pos, size, scale, center;
        private MouseState mouseState;
        private Vector2 mousePos;
        private Texture2D textureMain, texturePushed;
        private SpriteFont font;
        private bool active;
        public string? buttonText;

        //for invisible butttons
        public Button(Vector2 position, Vector2 size, Vector2 screenScale)
        {
            pos.X = position.X * screenScale.X;
            pos.Y = position.Y * screenScale.Y;
            this.size.X = size.X * screenScale.X;
            this.size.Y = size.Y * screenScale.Y;
            scale = screenScale;
            center = new Vector2(pos.X + (this.size.X / 2), pos.Y + (this.size.Y / 2));
            textureMain = null;
        }

        //for buttons with a a texture
        public Button(Texture2D idle, Texture2D pushed, string? Text, SpriteFont font, Vector2 position, Vector2 size, Vector2 screenScale)
        {
            pos.X = position.X * screenScale.X;
            pos.Y = position.Y * screenScale.Y;
            this.size.X = size.X * screenScale.X;
            this.size.Y = size.Y * screenScale.Y;
            scale = screenScale;
            center = new Vector2(pos.X + (this.size.X / 2), pos.Y + (this.size.Y / 2));
            textureMain = idle;
            texturePushed = pushed;
            buttonText = Text;
            this.font = font;
        }

        public bool IsPressed()
        {
            mouseState = Mouse.GetState();
            mousePos = new Vector2(mouseState.X, mouseState.Y);
            if (Math.Abs(center.X - mousePos.X) < (size.X / 2f) && Math.Abs(center.Y - mousePos.Y) < (size.Y / 2f))
                return true;
            else
                return false;
        }

        public void DrawButton(SpriteBatch sb)
        {
            if (IsActive)
            {
                sb.Draw(texturePushed, pos, null, Color.Gold, 0, Vector2.Zero,
                    scale, SpriteEffects.None, 0);
            }
            else
            {
                sb.Draw(textureMain, pos, null, Color.White, 0, Vector2.Zero,
                    scale, SpriteEffects.None, 0);
            }
            if(buttonText != null)
                sb.DrawString(font, buttonText, new Vector2(center.X - font.MeasureString(buttonText).X*.5f*scale.X, center.Y - font.MeasureString(buttonText).Y * .5f*scale.Y), Color.Black, 0f, Vector2.Zero, scale, SpriteEffects.None, 1);
        }

        public bool IsActive
        {
            get { return active; }
            set {active = value;}
        }
    }
}
