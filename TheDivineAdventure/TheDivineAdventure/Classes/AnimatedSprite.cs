using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TheDivineAdventure
{
    class AnimatedSprite
    {
        private Vector2 pos, spriteRes;
        private Rectangle currentBox;
        private Texture2D sprite;
        private int curFrame, frames;
        private float extendFrame, scale;
        private Color tint;
        bool loops;

        public AnimatedSprite(int width, int height, Texture2D spriteTex, int frames)
        {
            spriteRes.X = width;
            spriteRes.Y = height;
            sprite = spriteTex;
            currentBox = new Rectangle(0, 0, (int)spriteRes.X, (int)spriteRes.Y);
            curFrame = 0;
            extendFrame = 0;
            scale = 1;
            tint = Color.White;
            this.frames = frames;
            loops = true;
        }
        public AnimatedSprite(int width, int height, Texture2D spriteTex, int frames, bool repeats)
        {
            spriteRes.X = width;
            spriteRes.Y = height;
            sprite = spriteTex;
            currentBox = new Rectangle(0, 0, (int)spriteRes.X, (int)spriteRes.Y);
            curFrame = 0;
            extendFrame = 0;
            scale = 1;
            tint = Color.White;
            this.frames = frames;
            loops = repeats;
        }

        public void Draw(SpriteBatch sb, Vector2 screenScale)
        {
            sb.Draw(sprite, pos*screenScale, currentBox, tint, 0, Vector2.Zero, screenScale*scale, SpriteEffects.None, 0);
            if (loops==false && curFrame == frames)
                return;
            //progress animation
            if (curFrame < frames )
            {
                currentBox = new Rectangle((int)spriteRes.X * curFrame, 0, (int)spriteRes.X, (int)spriteRes.Y);
                //slow sprite framerate
                if (extendFrame < 1)
                {
                    extendFrame += 0.4f;
                }
                else
                {
                    curFrame++;
                    extendFrame = 0f;
                }
            }
            else
            {
                currentBox = new Rectangle(0, 0, (int)spriteRes.X, (int)spriteRes.Y);
                curFrame = 0;
            }
        }
        public Vector2 Pos
        {
            get { return pos; }
            set { pos = value; }
        }
        public float Scale
        {
            get { return scale; }
            set { scale = value; }
        }

        public Color Tint
        {
            get { return tint; }
            set { tint = value; }
        }

        public int Frame
        {
            get { return curFrame; }
            set { curFrame = value; }
        }
        public float Framerate
        {
            get { return extendFrame; }
            set { extendFrame = value; }
        }

    }
}
