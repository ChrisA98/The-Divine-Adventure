using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using System.Diagnostics;

namespace TheDivineAdventure
{
    class Camera
    {
        ///////////////
        ///VARIABLES///
        ///////////////
        // Essential
        GraphicsDevice gpu;
        PlayScene parent;

        // Constants
        private const float CAM_HEIGHT = 10f;
        private const float RENDER_DIST = 6000f;
        private const float FOV = 45f;

        // Vectors
        private Vector3 pos;
        private Vector3 cameraRotation;
        private Vector3 cameraLookAt;

        private Vector3 trailDistance;

        // Mouse
        private Vector3 mouseRotationBuffer;
        private MouseState curMouseState;
        private MouseState prevMouseState;

        // Player matrix
        private Matrix playerMatrix;



        /////////////////
        ///CONSTRUCTOR///
        /////////////////
        public Camera(GraphicsDevice graphicsDevice, Vector3 rotation, Player player, PlayScene parentScene)
        {
            // Set gpu
            gpu = graphicsDevice;

            // Set camera data
            trailDistance = new Vector3(0,10,0);
            Proj = Matrix.CreatePerspectiveFieldOfView(
                                MathHelper.ToRadians(FOV),
                                gpu.Viewport.AspectRatio,
                                0.05f,
                                RENDER_DIST);
            parent = parentScene;

            //get player matrix
            playerMatrix = parent.WorldPlayer;

            // Set mouseState
            prevMouseState = Mouse.GetState();

            // Set mouse position to the center of the screen
            //Mouse.SetPosition(gpu.Viewport.Width / 2, gpu.Viewport.Height / 2);

        }



        ///////////////
        ///FUNCTIONS///
        ///////////////
        public void Update(float dt, Player player)
        {
            //update player matrix
            playerMatrix = player.head;

            //Move camera with player
            Follow();
            prevMouseState = curMouseState;


        }

        // Set camera's position to follow the player
        private void Follow()
        {
            Pos = playerMatrix.Translation - (playerMatrix.Backward * 50) + new Vector3(0, 15, 10);
        }

        // Update the lookAt vector
        private void UpdateLookAt()
        {
            // Update the camera's lookAt vector
            cameraLookAt = playerMatrix.Translation + new Vector3 (0, 15, 0);
        }



        ////////////////////
        ///GETTER/SETTERS///
        ////////////////////
        public Matrix Proj
        {
            get;
            protected set;
        }

        public Matrix View
        {
            get { return Matrix.CreateLookAt(pos, cameraLookAt, Vector3.Up); }
        }

        public Vector3 LookAt
        {
            get { return new Vector3(View.M31, -View.M32, -View.M33) * new Vector3(1000, 1000, 1000); }
        }

        public Vector3 Pos
        {
            get { return pos; }
            set
            {
                pos = value;
                UpdateLookAt();
            }
        }

        public Vector3 Rot
        {
            get { return cameraRotation; }
            set
            {
                cameraRotation = value;
                UpdateLookAt();
            }
        }
    }
}
