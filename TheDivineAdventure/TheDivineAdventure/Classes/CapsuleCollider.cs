using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace TheDivineAdventure
{
    public class CapsuleCollider
    {
        private short height, ground;
        private short radius;
        private Matrix world;
        private short scale = 1;
        public BoundingSphere bottom, top;
        private CollisionProcessor collisionChecker;
        private Vector3[] drawPoints;

        //Debug Drawing variables
        public Shapes repBox;  //bounding box of sphere until I implement capsule drawing

        #region Constructor
        //Constructor
        public CapsuleCollider(short height_,short diameter_, Vector3 position_, Vector3 rotation_, GraphicsDevice gpu, Color color, short lowerOffset = 0)
        {
            height = height_;
            ground = lowerOffset;
            radius = (short)(diameter_ / 2);
            world = Matrix.CreateScale(scale) * Matrix.CreateTranslation(position_- new Vector3(0,ground,0));

            top = new BoundingSphere(Position + new Vector3(0,height - radius,0), radius);
            bottom = new BoundingSphere(Position + new Vector3(0,radius,0), radius);

            repBox = new Shapes(gpu, color, rotation_, position_);
            repBox.DefineCuboid(diameter_ / 2, (int)radius, 0, height, (int)radius, (int)radius);

            collisionChecker = new CollisionProcessor(this);

            Update(position_);
        }
        #endregion

        //Update Capsule
        public void Update(Vector3 Position_)
        {
            world = Matrix.CreateScale(scale) * Matrix.CreateTranslation(Position_- new Vector3(0,ground,0));
            top = new BoundingSphere(Position + new Vector3(0, height - radius, 0), radius);
            bottom = new BoundingSphere(Position + new Vector3(0, radius, 0), radius);
            repBox.Position = Position;
        }

        #region Collision Functions
        //check against capsule collision
        public bool Intersects(CapsuleCollider other)
        {
            Vector2 fSelf = new Vector2(Position.X, Position.Z); //Get flattened position
            Vector2 fOther = new Vector2(other.Position.X, other.Position.Z); //get flattened postion

            //Check if colliding with radius alone
            if (Vector2.Distance(fSelf, fOther) > radius+other.radius) return false;

            //Check vertical collision
            if (Position.Y > other.Position.Y)
            {
                if (bottom.Intersects(other.top)) return true;
                if (bottom.Intersects(other.bottom)) return true;
                if (bottom.Center.Y < other.Position.Y + other.height) return true;
            }
            else
            {
                if (top.Intersects(other.bottom)) return true;
                if (top.Intersects(other.top)) return true;
                if (top.Center.Y < other.Position.Y + other.radius) return true;
            }
            return false;
        }

        //check against bounding box
        public bool Intersects(BoundingBox other)
        {
            //Check vertical collision
            if (Position.Y > other.Min.Y && Position.Y + height > other.Max.Y)
            {
                if (bottom.Intersects(other)) return true;
            }
            else
            {
                if (top.Intersects(other)) return true;
            }

            Vector2 fSelf = new Vector2(Position.X, Position.Z); //Get flattened position
            Vector3[] otherCorners = other.GetCorners();        //get corners of cube

            //check against radius for each corner
            foreach(Vector3 point in otherCorners)
            {
                if (Vector3.Distance(Position, point) < radius) return true;
            }            

            return false;
        }

        //check against BoundingSphere
        public bool Intersects(BoundingSphere other)
        {

            Vector2 fSelf = new Vector2(Position.X, Position.Z); //Get flattened position
            Vector2 fOther = new Vector2(other.Center.X, other.Center.Z); //get flattened postion

            //Check if colliding with radius alone
            if (Vector2.Distance(fSelf, fOther) > radius+other.Radius) return false;

            //Check vertical collision
            if (Position.Y > other.Center.Y)
            {
                if (bottom.Intersects(other)) return true;
            }
            else
            {
                if (top.Intersects(other)) return true;
                if (top.Center.Y > other.Center.Y) return true;
            }

            return false;
        }

        //check against Bounding
        public bool Intersects(BoundingFrustum other)
        {
            if (other.Intersects(top) || other.Intersects(bottom)) return true;

            return false;
        }

        //check against Ray
        public float? Intersects(Ray other)
        {
            //check if ray hits spheres
            if (other.Intersects(top) !=null) return other.Intersects(top);
            if (other.Intersects(bottom) != null) return other.Intersects(bottom);

            Vector2 fSelf = new Vector2(Position.X, Position.Z); //Get flattened position
            Vector2 fOther = new Vector2(other.Position.X, other.Position.Z); //get flattened postion
            Vector2 fOtherDir = new Vector2(other.Direction.X, other.Direction.Z); //get flattened postion

            //return position of cylinder if collding (not elegant, but the distance should be close enugh for any needs I have
            if (Vector2.Distance(fSelf, Vector2.Distance(fSelf, fOther) * fOtherDir) <= radius) return Vector3.Distance(Position, other.Position);

            return null;
        }

        //Check collision with convex shape
        public bool Intersects(Shapes other)
        {
            return collisionChecker.GJKCapsuleIntersection(other);
        }
        public Vector3 Collisiondirection(Shapes other)
        {
            (Vector3, Vector3)? newDir = collisionChecker.GetCollisionDirection(other);
            if (newDir != null) return newDir.Value.Item1;
            else return Vector3.Zero;
        }
        #endregion

        #region Draw Functions

        public void DrawCubeDepict(Camera cam)
        {
            repBox.Position = Position;
            repBox.Draw(cam);
        }
        #endregion
        #region Getters/Setters
        public Matrix World
        {
            get { return world; }
        }
        public Vector3 Position
        {
            get { return world.Translation; }
            set { world.Translation = value; collisionChecker.UpdateCache(); }
        }
        public short Height
        {
            get { return height; }
        }
        public short Ground
        {
            get { return ground; }
        }
        public short Radius
        {
            get { return radius; }
        }
        #endregion
    }
}
