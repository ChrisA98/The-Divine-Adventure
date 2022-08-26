using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace TheDivineAdventure
{
    public class Skybox
    {
        private Model skyBox;
        private TextureCube boxTex;
        private Effect effect;
        private float size = 3000f;

        public Skybox(string skyTex, ContentManager Content)
        {
            skyBox = Content.Load<Model>("Model_Skybox");
            boxTex = Content.Load<TextureCube>(skyTex);
            effect = Content.Load<Effect>("EFFECT_Skybox");
        }

        public void Draw(Matrix view, Matrix projection, Vector3 camPos, GameTime gameTime)
        {
            GraphicsDevice GraphicsDevice = boxTex.GraphicsDevice;
            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            GraphicsDevice.DepthStencilState = DepthStencilState.None;
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;


            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                foreach (ModelMesh mesh in skyBox.Meshes)
                {
                    foreach (ModelMeshPart part in mesh.MeshParts)
                    {
                        part.Effect = effect;
                        part.Effect.Parameters["World"].SetValue(
                            Matrix.CreateScale(size) * Matrix.CreateTranslation(camPos));
                        part.Effect.Parameters["View"].SetValue(view);
                        part.Effect.Parameters["Projection"].SetValue(projection);
                        part.Effect.Parameters["SkyBoxTexture"].SetValue(boxTex);
                        part.Effect.Parameters["CameraPosition"].SetValue(camPos);
                    }

                    //draw mesh with the skybox effect
                    mesh.Draw();
                }
            }
        }

    }
}
