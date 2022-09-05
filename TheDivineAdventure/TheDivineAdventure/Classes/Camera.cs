
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace TheDivineAdventure
{
    class Camera
    {
        public float CAM_HEIGHT = 12;        // default up-distance from player's root position (depends on character size - 80 up in y direction to look at head)
        public float HEAD_OFFSET = 12;
        public const float FAR_PLANE = 4000;  // farthest camera can see (clip out things further away) 

        public Vector3 pos, target;           // camera position, target to look at
        public Matrix view, proj, view_proj; // viewing/projection transforms used to transform world vertices to screen coordinates relative to camera
        public Vector3 up;         // up direction for camera and world geometry (may depend on imported geometry's up direction [ie: is up -1 or 1 in y direction]
        Vector3 unit_direction;    // direction of camera (normalized to distance of 1 unit)


        // C O N S T R U C T 
        public Camera(GraphicsDevice gpu, Vector3 UpDirection)
        {
            up = UpDirection;
            pos = new Vector3(20, 12, -90);
            target = Vector3.Zero;
            view = Matrix.CreateLookAt(pos, target, up);
            proj = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, gpu.Viewport.AspectRatio, 1.0f, FAR_PLANE);
            view_proj = view * proj;
            unit_direction = view.Forward; unit_direction.Normalize();
        }

        // U P D A T E    P L A Y E R    C A M 
        public void Update(Matrix hero_pos)
        {
            pos = hero_pos.Translation - (hero_pos.Backward * 35) + new Vector3(-4, 18, 0);
            target = hero_pos.Translation;
            view = Matrix.CreateLookAt(pos, target + new Vector3(-4, 9, 0), up);
            view_proj = view * proj;
        }

    }
}
