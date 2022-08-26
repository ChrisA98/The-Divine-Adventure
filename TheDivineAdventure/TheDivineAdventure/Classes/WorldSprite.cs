using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace TheDivineAdventure
{
    class WorldSprite
    {
        //essential
        private ContentManager Content;
        private Game1 parent;
        //animation info
        private bool animated, loops;
        public bool finished;
        private Texture2D stillTex;
        private Texture2D[] animFrames;
        private int currFrame,frames;
        private float playSpeed, advFrame;
        private Vector2 spriteRes;
        private Rectangle currentBox;
        //location info
        private Matrix world;

        //render variables
        private Model frame;

        //create still sprite
        public WorldSprite(Texture2D tex, Game1 game, ContentManager cont)
        {
            //Assign content manager
            Content = cont;

            //Assign texture info
            animated = false;
            stillTex = tex;
            //Assign parent game 
            parent = game;

            //Assign model
            frame = Content.Load<Model>("MODEL_SpriteFrame");
        }

        //create animated sprite
        public WorldSprite(Texture2D[] tex, bool loops, float playbackSpeed, Game1 game, ContentManager cont)
        {
            //Assign content manager
            Content = cont;

            //assign frames to texture array
            animFrames = tex;

            //assign animation info
            animated = true;
            this.loops = loops;
            finished = false;
            playSpeed = playbackSpeed;
            frames = tex.Length-1;

            //Assign parent game 
            parent = game;

            //Assign model
            frame = Content.Load<Model>("MODEL_SpriteFrame");

            //set frame to 0
            currFrame = 0;
        }

        //draw at dynamic location
        public void Draw(Matrix world, Matrix camera, Matrix projection)
        {
            parent.GraphicsDevice.BlendState = BlendState.Additive;
            if (animated)
            {
                //animate and assign animated texture
                AdvanceFrame();
                foreach (ModelMesh mesh in frame.Meshes)
                {
                    foreach (BasicEffect effect in mesh.Effects)
                    {
                        effect.Texture = animFrames[currFrame];
                        effect.TextureEnabled = true;
                    }
                }
            }
            else
            {
                //assign stilltexture
                foreach (BasicEffect effect in frame.Meshes[0].Effects)
                {
                    effect.Texture = stillTex;
                    effect.TextureEnabled = true;
                }
            }

            //draw a 2d sprite for the projectile
            frame.Draw(world, camera, projection);
            parent.GraphicsDevice.BlendState = BlendState.Opaque;
        }

        //draw at static location
        public void Draw(Matrix camera, Matrix projection)
        {
            parent.GraphicsDevice.BlendState = BlendState.Additive;
            if (animated)
            {
                //animate and assign animated texture
                AdvanceFrame();
                foreach (ModelMesh mesh in frame.Meshes)
                {
                    foreach (BasicEffect effect in mesh.Effects)
                    {
                        effect.Texture = animFrames[currFrame];
                        effect.TextureEnabled = true;
                    }
                }
            }
            else
            {
                //assign stilltexture
                foreach (BasicEffect effect in frame.Meshes[0].Effects)
                {
                    effect.Texture = stillTex;
                    effect.TextureEnabled = true;
                }
            }

            //draw a 2d sprite for the projectile
            frame.Draw(world, camera, projection);
            parent.GraphicsDevice.BlendState = BlendState.Opaque;
        }


        private void AdvanceFrame()
        {
            //progress animation
            if (currFrame < frames && finished == false)
            {
                    currFrame++;
                    advFrame = 0;
            }
            else
            {
                if (loops == true)
                {
                    currFrame = 0;
                }
                else
                {
                    finished = true;
                }
            }
        }

        public static Texture2D[] GenerateAnim(Texture2D tex, int frameWidth, Game1 game)
        {
            //define frame amount
            int frames = tex.Width / frameWidth;

            //create outpur array
            Texture2D[] output = new Texture2D[frames];

            //generate frame rectangle and sprite resolution
            Vector2 spriteRes = new Vector2(0, 0);
            spriteRes.X = frameWidth;
            spriteRes.Y = tex.Height;
            Rectangle currentBox = new Rectangle(0, 0, (int)spriteRes.X, (int)spriteRes.Y);
            for (int i = 0; i < frames; i++)
            {
                //creat current frame
                Texture2D animFrame = new Texture2D(game.GraphicsDevice, (int)spriteRes.X, (int)spriteRes.Y);
                int count = (int)(spriteRes.X * spriteRes.Y);
                Color[] data = new Color[count];
                tex.GetData<Color>(0, currentBox, data, 0, count);
                animFrame.SetData(data);

                //add frame to array
                output[i] = animFrame;
            }

            return output;

        }

        public void SetPos(Matrix world)
        {
            this.world = world;
        }
    }
}
