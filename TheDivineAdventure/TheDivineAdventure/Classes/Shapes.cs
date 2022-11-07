using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Text;

namespace TheDivineAdventure
{
    //<<Draw primitive shapes>>
    public class Shapes
    {
        private GraphicsDevice gpu; //Display
        private Camera cam;

        private VertexBuffer vBuffer;
        private IndexBuffer iBuffer;

        private BasicEffect bEffect;

        private Matrix world;

        private Color color;

        private Vector3 position;
        private Vector3 rotation;

        private VertexPositionColor[] verticesCol;
        public Vector3[] vertices;
        short[] indices;

        public int[] initBounds;    // bounds without world transform
        public float[] bounds;      //array of bounds with position info

        public Type parentObject;   //Wha object this shape belongs to

        private CollisionProcessor collisionChecker;    //Check collision
        public bool isStatic;               //Whether or not the object can be moved (improves collision detection)
        private List<Vector3> globalVertices;   //Global vertices saved for static shapes


        #region <<Constructers>>
        //Constructor - with parent object//
        public Shapes(GraphicsDevice gpu_, Color color_, Vector3 rotation_, Vector3 position_, Type parentType, bool isStatic_ = false)
        {
            gpu = gpu_;
            color = color_;
            position = position_;
            rotation = rotation_;
            parentObject = parentType;
            collisionChecker = new CollisionProcessor(this);
            isStatic = isStatic_;
    }

        //Constructor - without parent object//
        public Shapes(GraphicsDevice gpu_, Color color_, Vector3 rotation_, Vector3 position_, bool isStatic_ = false)
        {
            gpu = gpu_;
            color = color_;
            position = position_;
            rotation = rotation_;
            collisionChecker = new CollisionProcessor(this);
            isStatic = isStatic_;
        }

        #endregion

        //publically accesable cuboid definition with option for asymetrical bounds
        public void DefineCuboid(int XForward, int XBackward, int YDown, int YUp, int ZForward, int ZBackward)
        {
            SetCuboidVerts(XForward, XBackward, YDown, YUp, ZForward, ZBackward);   //set vertices based on bounds
            SetCuboidIndices();     //set indices
            int[] bounds = {XForward, XBackward, YDown, YUp, ZForward, ZBackward};
            initBounds = bounds;
            globalVertices = new List<Vector3>();
            world = Matrix.CreateScale(1) *
                Matrix.CreateRotationX(MathHelper.ToRadians(rotation.X)) *
                Matrix.CreateRotationY(MathHelper.ToRadians(rotation.Y)) *
                Matrix.CreateRotationZ(MathHelper.ToRadians(rotation.Z)) *
                Matrix.CreateTranslation(position);

            bEffect = new BasicEffect(gpu)
            {
                World = world,
                VertexColorEnabled = true
            };
        }

        //publically accesable cuboid definition with symmetrical bounds from vector3
        public void DefineCuboid(Vector3 area)
        {
            //convert to axis distances
            DefineCuboid((int)(area.X/2), (int)(area.X / 2), (int)(area.Y / 2), (int)(area.Y / 2), (int)(area.Z / 2), (int)(area.Z / 2));
        }

        //draw
        public void Draw(Camera cam_)
        {
            world = Matrix.CreateScale(1) *
                Matrix.CreateRotationX(MathHelper.ToRadians(rotation.X)) *
                Matrix.CreateRotationY(MathHelper.ToRadians(rotation.Y)) *
                Matrix.CreateRotationZ(MathHelper.ToRadians(rotation.Z)) *
                Matrix.CreateTranslation(position);

            bEffect.View = cam_.view;
            bEffect.Projection = cam_.proj;
            bEffect.World = world;
            bEffect.VertexColorEnabled = true;

            gpu.SetVertexBuffer(vBuffer);
            gpu.Indices = IBuffer;

            foreach (EffectPass pass in bEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                gpu.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 12);
            }
        }

        #region Collision Functions
        //Handle Collision with other Shapes
        public bool Intersects(Shapes otherBox)
        {
            return collisionChecker.GJKIntersection(otherBox);
        }
        //Handle Collision with BoundingBoxes
        public bool Intersects(BoundingBox otherBox)
        {
            return collisionChecker.GJKIntersection(otherBox);
        }
        //Handle Collision with  Frustums
        public bool Intersects(BoundingFrustum otherBox)
        {
            return collisionChecker.GJKIntersection(otherBox);
        }
        //Handle Collision with Frustums
        public bool Intersects(CapsuleCollider other)
        {
            return collisionChecker.GJKIntersection(other);
        }
        //Handle Collision with bounding spheres
        public bool Intersects(BoundingSphere otherSphere)
        {
            return collisionChecker.GJKIntersection(otherSphere);
        }
        //Handle Collision with bounding spheres
        public Vector3? Intersects(Ray ray)
        {
            return collisionChecker.GJKIntersection(ray);
        }

        #endregion

        public Vector3 Collisiondirection(Shapes other)
        {
            return collisionChecker.GetCollisionDirection(other).Value.Item2;
        }
        #region Private Functions

        public Vector3[] GetGlobalVerts()
        {

            if (isStatic && globalVertices.Count != 0) return globalVertices.ToArray();  //return static vertices

            collisionChecker.UpdateCache();
            world = Matrix.CreateScale(1) *
                Matrix.CreateRotationX(MathHelper.ToRadians(rotation.X)) *
                Matrix.CreateRotationY(MathHelper.ToRadians(rotation.Y)) *
                Matrix.CreateRotationZ(MathHelper.ToRadians(rotation.Z)) *
                Matrix.CreateTranslation(position);

            bEffect.World = world;
            bEffect.VertexColorEnabled = true;

            gpu.SetVertexBuffer(vBuffer);
            gpu.Indices = IBuffer;

            Vector3[] newV = new Vector3[vertices.Length];
            int i = 0;
            foreach (Vector3 point in vertices)
            {
                newV[i] = Vector3.Transform(vertices[i], world);
                i++;
            }
            if (isStatic) globalVertices.AddRange(newV);    //set global vertices
            return newV;  //return globally defined verts
        }

        
        //Set verts based on bounds declared - Cuboid
        private void SetCuboidVerts(int XForward, int XBackward, int YDown, int YUp, int ZForward, int ZBackward)
        {
            verticesCol = new VertexPositionColor[8];
            vertices = new Vector3[8];

            //Invert negative axis
            XBackward *= -1;
            ZBackward *= -1;
            YDown *= -1;

            //front Right Bottom corner
            verticesCol[0] = new VertexPositionColor(new Vector3(XBackward, YDown, ZForward), color);
            vertices[0] = new Vector3(XBackward, YDown, ZForward);

            verticesCol[1] = new VertexPositionColor(new Vector3(XBackward, YUp, ZForward), color);
            vertices[1] = new Vector3(XBackward, YUp, ZForward);

            verticesCol[2] = new VertexPositionColor(new Vector3(XForward, YUp, ZForward), color);
            vertices[2] = new Vector3(XForward, YUp, ZForward);

            verticesCol[3] = new VertexPositionColor(new Vector3(XForward, YDown, ZForward), color);
            vertices[3] = new Vector3(XForward, YDown, ZForward);

            verticesCol[4] = new VertexPositionColor(new Vector3(XBackward, YDown, ZBackward), color);
            vertices[4] = new Vector3(XBackward, YDown, ZBackward);

            verticesCol[5] = new VertexPositionColor(new Vector3(XBackward, YUp, ZBackward), color);
            vertices[5] = new Vector3(XBackward, YUp, ZBackward);

            verticesCol[6] = new VertexPositionColor(new Vector3(XForward, YUp, ZBackward), color);
            vertices[6] = new Vector3(XForward, YUp, ZBackward);

            verticesCol[7] = new VertexPositionColor(new Vector3(XForward, YDown, ZBackward), color);
            vertices[7] = new Vector3(XForward, YDown, ZBackward);

            vBuffer = new VertexBuffer(gpu, typeof(VertexPositionColor), 8, BufferUsage.WriteOnly);
            vBuffer.SetData<VertexPositionColor>(verticesCol);
        }

        //sets indices for shape - Cuboid
        private void SetCuboidIndices()
        {
            indices = new short[36];

            //Front face
            //bottom right triangle
            indices[0] = 0;
            indices[1] = 3;
            indices[2] = 2;
            //top left triangle
            indices[3] = 2;
            indices[4] = 1;
            indices[5] = 0;
            //back face
            //bottom right triangle
            indices[6] = 4;
            indices[7] = 7;
            indices[8] = 6;
            //top left triangle
            indices[9] = 6;
            indices[10] = 5;
            indices[11] = 4;
            //Top face
            //bottom right triangle
            indices[12] = 1;
            indices[13] = 2;
            indices[14] = 6;
            //top left triangle
            indices[15] = 6;
            indices[16] = 5;
            indices[17] = 1;
            //bottom face
            //bottom right triangle
            indices[18] = 4;
            indices[19] = 7;
            indices[20] = 3;
            //top left triangle
            indices[21] = 3;
            indices[22] = 0;
            indices[23] = 4;
            //left face
            //bottom right triangle
            indices[24] = 4;
            indices[25] = 0;
            indices[26] = 1;
            //top left triangle
            indices[27] = 1;
            indices[28] = 5;
            indices[29] = 4;
            //right face
            //bottom right triangle
            indices[30] = 3;
            indices[31] = 7;
            indices[32] = 6;
            //top left triangle
            indices[33] = 6;
            indices[34] = 2;
            indices[35] = 3;

            iBuffer = new IndexBuffer(gpu, IndexElementSize.SixteenBits, sizeof(short) * indices.Length, BufferUsage.WriteOnly);
            iBuffer.SetData(indices);
        }
        #endregion

        #region <<Getters/Setters>>
        public VertexBuffer VBuffer
        { get { return vBuffer; } set { vBuffer = value; } }
        public IndexBuffer IBuffer
        { get { return iBuffer; } set { iBuffer = value; } }
        public BasicEffect BEffect
        { get { return bEffect; } set { bEffect = value; } }
        public Matrix World
        { get { return world; } set { world = value; } }
        public Vector3 Position
        { get {return position; } set { collisionChecker.UpdateCache(); position = value; } }
        public Vector3 Rotation
        { get { return rotation; } set { rotation = value; } }

        #endregion

        #region Structs and Nested Classes

        #endregion
    }
}