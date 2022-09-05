
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Text;

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace TheDivineAdventure
{
    class TextBox
    {
        private String          text,endMarker;
        private MouseState      mouseState;
        private KeyboardState   keyState;
        private Vector2         mousePos;
        private Vector2         pos, res, scale, center;
        private Color           backdrop;
        private Texture2D       baseTex;
        private bool            isSelected, isHi;
        private SpriteFont      font;
        private Keys[]          integerKeys;
        private int             delay, markDelay,maxLength;

        public TextBox (String? placeholder, int maxLength,SpriteFont font, Vector2 Location, int size, Color backColor, Game1 parent)
        {
            scale = parent.currentScreenScale;
            text = placeholder;
            if (placeholder == null)
                text = "";
            pos = Location * scale;
            res.Y = size * scale.Y;
            res.X = (font.MeasureString("1") * (maxLength+1) * scale).X;
            backdrop = backColor;
            center = new Vector2(pos.X + (this.res.X / 2), pos.Y + (this.res.Y / 2));
            isSelected = false;
            isHi = false;
            this.maxLength = maxLength;
            this.font = font;

            baseTex = new Texture2D(parent.GraphicsDevice, 1, 1);
            baseTex.SetData(new[] { Color.Black });

            integerKeys = new Keys[] { Keys.D1, Keys.D2, Keys.D3, Keys.D4, Keys.D5, Keys.D6, Keys.D7, Keys.D8, Keys.D9, Keys.D0, Keys.NumPad0, Keys.NumPad1, Keys.NumPad2, Keys.NumPad3
            ,Keys.NumPad4, Keys.NumPad5, Keys.NumPad6, Keys.NumPad7, Keys.NumPad8, Keys.NumPad9};
        }

        public void IsPressed()
        {
            mouseState = Mouse.GetState();
            mousePos = new Vector2(mouseState.X, mouseState.Y);
            if (Math.Abs(center.X - mousePos.X) < (res.X / 2f) && Math.Abs(center.Y - mousePos.Y) < (res.Y / 2f))
                isSelected =  true;
            else
                isSelected = false;
        }

        public void Update(GameTime gameTime)
        {
            keyState = Keyboard.GetState();
            if (isSelected && markDelay < 1)
            {
                if (isHi)
                {
                    endMarker += "|";
                    isHi = false;
                    markDelay = 22;
                }
                else
                {
                    endMarker = "";
                    isHi = true;
                    markDelay = 22;
                }
            }

            if (isSelected && delay < 1)
            {
                foreach (Keys key in keyState.GetPressedKeys())
                {
                    if (key == Keys.Back && text.Length != 0)
                    {
                        text = text.Substring(0, text.Length - 1);
                        delay = 8;
                    }
                    else if (Array.Exists(integerKeys, x => x == key) && text.Length < maxLength) {
                        text += key.ToString().Substring(key.ToString().Length - 1);
                        delay = 8;
                    }else if (key.ToString().Length == 1 && text.Length < maxLength)
                    {
                        if (!keyState.CapsLock)
                            text += key.ToString().ToLower();
                        else
                            text += key.ToString();
                        delay = 8;
                    }
                }

            }
            if (delay > 0)
                delay--;

            if (markDelay > 0)
                markDelay--;
            if (!isSelected)
            {
                endMarker = "";
            }
        }

        public void Draw(SpriteBatch sb)
        {
            sb.Draw(baseTex, pos, new Rectangle(0, 0, (int)res.X, (int)res.Y), backdrop, 0, Vector2.Zero,
                    1, SpriteEffects.None, 0);
            if (text + endMarker != null)
            {
                sb.DrawString(font, text + endMarker,new Vector2(pos.X+5, pos.Y ),Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 1);
            }
        }


        //getter setter methods
        public String Text
        {
            get { return text; }
            set { text = value; }
        }


    }
}