using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace TheDivineAdventure.SkinModels
{
    public class DirectionLight
    {
        public Vector3 direction;
        public Vector3 diffuseColor;
        public Vector3 specularColor;
    }

    public class SkinFx
    {
        public const int MAX_BONES = 180;          // This should match number in custom SkinEffect.fx
        public const int WEIGHTS_PER_VERTEX = 4;

        public Camera cam;                        // reference to camera
        public Effect fx;
        public Texture2D default_tex;
        public Vector4 diffuseCol = Vector4.One;
        public Vector3 emissiveCol = Vector3.Zero;
        public Vector3 specularCol = Color.LightYellow.ToVector3();
        public float specularPow = 32f;
        public Vector3 ambientCol = Vector3.Zero;
        public bool fogEnabled = false;
        public DirectionLight[] lights;           // lights: key, fill, back 
        public float alpha = 1f;
        public float fogStart = 0f;
        public float fogEnd = 1f;
        public Matrix world = Matrix.Identity;
        public Matrix worldView = Matrix.Identity;



        //------------------
        // CONSTRUCT
        //------------------
        public SkinFx(ContentManager Content, Camera Cam, string fx_filename, bool enableFog = false)
        {
            lights = new DirectionLight[3];
            for (int i = 0; i < 3; i++) lights[i] = new DirectionLight();
            cam = Cam;
            default_tex = Content.Load<Texture2D>("default_tex");
            fx = Content.Load<Effect>(fx_filename);
            Matrix[] identityBones = new Matrix[MAX_BONES];
            for (int i = 0; i < MAX_BONES - 1; i++)
            {
                identityBones[i] = Matrix.Identity;
            }
            SetBoneTransforms(identityBones);
            fx.Parameters["TexDiffuse"].SetValue(default_tex);
            fx.Parameters["DiffuseColor"].SetValue(diffuseCol);
            fx.Parameters["EmissiveColor"].SetValue(emissiveCol);
            fx.Parameters["SpecularColor"].SetValue(specularCol);
            fx.Parameters["SpecularPower"].SetValue(specularPow);

            SetDefaultLighting();
            if (enableFog) ToggleFog();
        }

        public SkinFx(ContentManager Content, string fx_filename, bool enableFog = false)
        {
            lights = new DirectionLight[3];
            for (int i = 0; i < 3; i++) lights[i] = new DirectionLight();
            default_tex = Content.Load<Texture2D>("default_tex");
            fx = Content.Load<Effect>(fx_filename);
            Matrix[] identityBones = new Matrix[MAX_BONES];
            for (int i = 0; i < MAX_BONES - 1; i++)
            {
                identityBones[i] = Matrix.Identity;
            }
            SetBoneTransforms(identityBones);
            fx.Parameters["TexDiffuse"].SetValue(default_tex);
            fx.Parameters["DiffuseColor"].SetValue(diffuseCol);
            fx.Parameters["EmissiveColor"].SetValue(emissiveCol);
            fx.Parameters["SpecularColor"].SetValue(specularCol);
            fx.Parameters["SpecularPower"].SetValue(specularPow);

            SetDefaultLighting();
            if (enableFog) ToggleFog();
        }

        //--------------------------------------
        // SET BONE TRANSFORMS 
        //--------------------------------------
        /// <summary> Sets an array of skinning bone transform matrices. </summary>
        public void SetBoneTransforms(Matrix[] boneTransforms)
        {
            if ((boneTransforms == null) || (boneTransforms.Length == 0)) throw new ArgumentNullException("boneTransforms");
            if (boneTransforms.Length > MAX_BONES) throw new ArgumentException();
            fx.Parameters["Bones"].SetValue(boneTransforms);
        }



        //----------------------------------------------
        // SET DEFAULT LIGHTING
        //----------------------------------------------
        public void SetDefaultLighting()
        {
            float u = Vector3.Up.Y;     // I assume up is -Y or +Y
            ambientCol = new Vector3(.1f, 0.09882354f, 0.1819608f);
            lights[0].direction = new Vector3(-0.5265408f, -0.5735765f * u, -0.6275069f); // Key light.
            lights[0].diffuseColor = new Vector3(1, 0.9607844f, 0.8078432f);
            lights[0].specularColor = new Vector3(1, 0f, 0f);
            lights[1].direction = new Vector3(0.7198464f, 0.3420201f * u, 0.6040227f);    // Fill light
            lights[1].diffuseColor = new Vector3(0.9647059f, 0.7607844f, 0.4078432f);
            lights[0].specularColor = new Vector3(1, 0f, 0f);
            lights[2].direction = new Vector3(0.4545195f, -0.7660444f * u, 0.4545195f);   // Back light
            lights[2].diffuseColor = new Vector3(0.3231373f, 0.3607844f, 0.3937255f);
            lights[2].specularColor = new Vector3(1, 0f, 0f);
            fx.Parameters["LightDir1"].SetValue(lights[0].direction);
            fx.Parameters["LightDiffCol1"].SetValue(lights[0].diffuseColor);
            fx.Parameters["LightSpecCol1"].SetValue(lights[0].specularColor);
            fx.Parameters["LightDir2"].SetValue(lights[1].direction);
            fx.Parameters["LightDiffCol2"].SetValue(lights[1].diffuseColor);
            fx.Parameters["LightSpecCol2"].SetValue(lights[1].specularColor);
            fx.Parameters["LightDir3"].SetValue(lights[2].direction);
            fx.Parameters["LightDiffCol3"].SetValue(lights[2].diffuseColor);
            fx.Parameters["LightSpecCol3"].SetValue(lights[2].specularColor);
        }



        // SET DIRECTIONAL LIGHT 
        public void SetDirectionalLight(int index, Vector3 direction, Color diffuse_color, Color specular_color)
        {
            if (index >= 3) return;
            lights[index].direction = direction;
            lights[index].diffuseColor = diffuse_color.ToVector3();
            lights[index].specularColor = specular_color.ToVector3();
            switch (index)
            {
                case 0:
                    fx.Parameters["LightDir1"].SetValue(lights[0].direction);
                    fx.Parameters["LightDiffCol1"].SetValue(lights[0].diffuseColor);
                    fx.Parameters["LightSpecCol1"].SetValue(lights[0].specularColor); break;
                case 1:
                    fx.Parameters["LightDir2"].SetValue(lights[1].direction);
                    fx.Parameters["LightDiffCol2"].SetValue(lights[1].diffuseColor);
                    fx.Parameters["LightSpecCol2"].SetValue(lights[1].specularColor); break;
                case 2:
                    fx.Parameters["LightDir3"].SetValue(lights[2].direction);
                    fx.Parameters["LightDiffCol3"].SetValue(lights[2].diffuseColor);
                    fx.Parameters["LightSpecCol3"].SetValue(lights[2].specularColor); break;
            }
        }

        public void SetFogStart(float fog_start) { fogEnabled = false; fogStart = fog_start; ToggleFog(); }
        public void SetFogEnd(float fog_end) { fogEnabled = false; fogEnd = fog_end; ToggleFog(); }
        public void SetFogColor(Color fog_color) { fx.Parameters["FogColor"].SetValue(fog_color.ToVector3()); }

        // T0GGLE FOG 
        public void ToggleFog()
        {
            if (!fogEnabled)
            {
                if (fogStart == fogEnd)
                {
                    fx.Parameters["FogVector"].SetValue(new Vector4(0, 0, 0, 1));   //disable fog
                }
                else
                {
                    float scale = .2f / (fogStart - fogEnd);
                    Vector4 fogVector = new Vector4
                    {
                        X = worldView.M13 * scale,
                        Y = worldView.M23 * scale,
                        Z = worldView.M33 * scale,
                        W = (worldView.M43 + fogStart) * scale
                    };
                    fx.Parameters["FogVector"].SetValue(fogVector);
                    fogEnabled = true;
                }
            }
            else { fx.Parameters["FogVector"].SetValue(Vector4.Zero); fogEnabled = false; }
        }

        public void DisableFog()
        {
            fx.Parameters["FogVector"].SetValue(Vector4.Zero); fogEnabled = false;
        }

        //------------------------------
        // SET DRAW PARAMS(use just before drawing)
        //------------------------------
        public void SetDrawParams(Camera cam, Texture2D texture = null, Texture2D normalMapTex = null, Texture2D specularTex = null)
        {
            Matrix.Multiply(ref world, ref cam.view, out worldView); // (used with fog)
            Matrix worldInverse, worldInverseTranspose;
            Matrix.Invert(ref world, out worldInverse);
            Matrix.Transpose(ref worldInverse, out worldInverseTranspose);
            Vector4 diffuse = new Vector4();
            Vector3 emissive = new Vector3();
            diffuse.X = diffuseCol.X * alpha;
            diffuse.Y = diffuseCol.Y * alpha;
            diffuse.Z = diffuseCol.Z * alpha;
            diffuse.W = alpha;
            emissive.X = (emissiveCol.X + ambientCol.X * diffuseCol.X) * alpha;
            emissive.Y = (emissiveCol.Y + ambientCol.Y * diffuseCol.Y) * alpha;
            emissive.Z = (emissiveCol.Z + ambientCol.Z * diffuseCol.Z) * alpha;
            fx.Parameters["World"].SetValue(world);
            fx.Parameters["WorldViewProj"].SetValue(world * cam.view_proj);
            fx.Parameters["CamPos"].SetValue(cam.pos);
            if (texture != null) fx.Parameters["TexDiffuse"].SetValue(texture);
            else fx.Parameters["TexDiffuse"].SetValue(default_tex);
            fx.Parameters["WorldInverseTranspose"].SetValue(worldInverseTranspose);
            fx.Parameters["DiffuseColor"].SetValue(diffuse);
            fx.Parameters["EmissiveColor"].SetValue(emissive);

            // Update Fog
            if (fogEnabled)
            {
                float scale = .2f / (fogStart - fogEnd);
                Vector4 fogVector = new Vector4
                {
                    X = worldView.M13 * scale,
                    Y = worldView.M23 * scale,
                    Z = worldView.M33 * scale,
                    W = (worldView.M43 + fogStart) * scale
                };
                fx.Parameters["FogVector"].SetValue(fogVector);
            }

            if (normalMapTex == null)
            {
                fx.CurrentTechnique = fx.Techniques["Skin_Directional_Fog"];
            }
            else
            {
                fx.Parameters["TexNormalMap"].SetValue(normalMapTex);
                fx.CurrentTechnique = fx.Techniques["Skin_NormalMapped_Directional_Fog"];
            }
            fx.CurrentTechnique.Passes[0].Apply();
        }

        public void SetDiffuseCol(Vector4 diffuse) { diffuseCol = diffuse; }
        public void SetEmissiveCol(Vector3 emissive) { emissiveCol = emissive; }
        public void SetSpecularCol(Vector3 specular) { this.specularCol = specular; fx.Parameters["SpecularColor"].SetValue(specularCol); }
        public void SetSpecularPow(float power) { specularPow = power; fx.Parameters["SpecularPower"].SetValue(power); }    

    }
}
