
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Diagnostics;

namespace TheDivineAdventure
{
    public class Camera
    {
        public GraphicsDevice gpu;

        public float CAM_HEIGHT = 12;        // default up-distance from player's root position (depends on character size - 80 up in y direction to look at head)
        public float HEAD_OFFSET = 12;
        public const float FAR_PLANE = 4000;  // farthest camera can see (clip out things further away) 

        public Vector3 pos, target;           // camera position, target to look at
        public Matrix view, proj, view_proj; // viewing/projection transforms used to transform world vertices to screen coordinates relative to camera
        public Vector3 up;         // up direction for camera and world geometry (may depend on imported geometry's up direction [ie: is up -1 or 1 in y direction]
        Vector3 unit_direction;    // direction of camera (normalized to distance of 1 unit)
        Player player;  //target
        bool trails;    //decide whether camera has drag
        Random rand;
        public BoundingFrustum renderSpace; //bounding box to check what needs to be rendered


        public Camera(GraphicsDevice gpu_, Vector3 UpDirection)
        {
            gpu = gpu_;
            up = UpDirection;
            pos = new Vector3(20, CAM_HEIGHT, -90);
            target = Vector3.Zero;
            view = Matrix.CreateLookAt(pos, target, up);
            proj = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, gpu.Viewport.AspectRatio, 20f, FAR_PLANE);
            view_proj = view * proj;
            unit_direction = view.Forward; unit_direction.Normalize();
            trails = false;
            renderSpace = new BoundingFrustum(view_proj);
        }

        public Camera(GraphicsDevice gpu_, Vector3 UpDirection, Player player_)
        {
            gpu = gpu_;
            up = UpDirection;
            if (player_.role == "WARRIOR") CAM_HEIGHT = 36;
            pos = new Vector3(20, CAM_HEIGHT, -90);
            target = Vector3.Zero;
            view = Matrix.CreateLookAt(pos, target, up);
            proj = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, gpu.Viewport.AspectRatio, 15f, FAR_PLANE);
            view_proj = view * proj;
            unit_direction = view.Forward; unit_direction.Normalize();
            player = player_;
            trails = true;
            renderSpace = new BoundingFrustum(view_proj);
        }

        //Update Player Cam
        public void Update(Matrix hero_pos, Level level = null)
        {
            if (trails)
            {
                float trackDistance = (player.vel / player.walkMax * player.runMod);
                if (player.vel / player.walkMax > 1)
                {
                    rand = new Random();
                    trackDistance += (float)rand.NextDouble()/5;
                }
                pos = hero_pos.Translation - (hero_pos.Backward * 35) - (hero_pos.Backward *1.2f* trackDistance) + new Vector3(0, CAM_HEIGHT, 0) + (hero_pos.Left * 4);

                if (player.isAttacking[1])
                {
                    rand = new Random();
                    pos.X += (float)rand.NextDouble() / 5;
                    pos.Y += (float)rand.NextDouble() / 5;
                    pos.Z += (float)rand.NextDouble() / 5;
                }

            }
            else
            {
                pos = hero_pos.Translation - (hero_pos.Backward * 35) + new Vector3(0, CAM_HEIGHT, 0) + (hero_pos.Left * 4);
            }
            target = hero_pos.Translation;
            view = Matrix.CreateLookAt(pos, target + new Vector3(0, CAM_HEIGHT*0.65f, 0)+ (hero_pos.Left * 4), up);
            view_proj = view * proj;
            renderSpace = new BoundingFrustum(view_proj);
        }

    }
}
