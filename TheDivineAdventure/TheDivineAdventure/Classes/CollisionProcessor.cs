using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Diagnostics;
using System.Collections.Generic;

namespace TheDivineAdventure
{
    class CollisionProcessor
    {
        private Shapes parent;  //shape that owns this processor
        private CapsuleCollider capsuleParent;  //for capsule colliders
        private List<CachedCollision> lastFrameCollisions;  //cache list (TO BE IMPLEMENTED)
        private List<CachedCollision> thisFrameCollisions;  //cache list (TO BE IMPLEMENTED)
        private Vector3 direction;  //currnet direction
        private List<Vector3> simplex;  //current simplex vertices
        private (Vector3,Vector3)[] baseVertices;
        private (int, int)[] baseIndices;
        private int loops;

        #region <<Constructors>>
        //Constructor - Shapes Class(Convex Shapes Only)
        public CollisionProcessor(Shapes parentShape)
        {
            parent = parentShape;
            capsuleParent = null;
            lastFrameCollisions = new List<CachedCollision>();
            thisFrameCollisions = new List<CachedCollision>();
            baseVertices = new (Vector3, Vector3)[4];
            baseIndices = new (int, int)[4];
        }

        //Constructor - Capsule Collider
        public CollisionProcessor(CapsuleCollider parentShape)
        {
            parent = null;
            capsuleParent = parentShape;
            lastFrameCollisions = new List<CachedCollision>();
            thisFrameCollisions = new List<CachedCollision>();
            baseVertices = new (Vector3, Vector3)[4];
            baseIndices = new (int, int)[4];
        }
        #endregion

        #region Public Convex Shape Collision Functions

        //check collision with convex shape
        public bool GJKIntersection(Shapes shape2)
        {
            loops = 0;
            simplex = new List<Vector3>();
            //check cached collisions for improved efficiency
            Vector3[] s1 = parent.GetGlobalVerts();
            Vector3[] s2 = shape2.GetGlobalVerts();
            //used for generating cached collisions
            direction = shape2.Position - parent.Position;
            simplex.Add(Support(s1, s2));

            direction = Vector3.Negate(simplex[0]);

            while (true)
            {
                loops++;
                if(loops > s1.Length*2) return false;   //No collide stop occasional infinite loops
                Vector3 vertA = Support(s1, s2);
                if (Vector3.Dot(vertA, direction) < 0) return false;   //No collide
                simplex.Add(vertA);
                if (HandleSimplex())
                {   
                    thisFrameCollisions.Add(new CachedCollision(shape2, baseIndices, baseVertices));
                    return true;
                }
                if (simplex.Count > 4) return false;
            }
        }

        //check collision with Bounding Box
        public bool GJKIntersection(BoundingBox shape2)
        {
            simplex = new List<Vector3>();
            //check cached collisions for improved efficiency
            Vector3[] s1 = parent.GetGlobalVerts();
            Vector3[] s2 = shape2.GetCorners();

            Vector3 p1 = Vector3.Lerp(shape2.Min, shape2.Max, 0.5f);
            direction = p1 - parent.Position;
            simplex.Add(Support(s1, s2));

            direction = Vector3.Negate(simplex[0]);

            while (true)
            {
                Vector3 vertA = Support(s1, s2);
                if (Vector3.Dot(vertA, direction) <= 0) return false;   //No collide
                simplex.Add(vertA);
                if (HandleSimplex())
                {
                    thisFrameCollisions.Add(new CachedCollision(shape2, baseIndices, baseVertices));
                    return true;
                }
                if (simplex.Count > 4) return false;
            }
        }

        //check collision between convex shapes
        public bool GJKIntersection(BoundingSphere shape2)
        {
            simplex = new List<Vector3>();
            //check cached collisions for improved efficiency
            Vector3[] s1 = parent.GetGlobalVerts();

            direction = shape2.Center - parent.Position;
            simplex.Add(Support(s1, shape2));

            direction = Vector3.Negate(simplex[0]);

            while (true)
            {
                Vector3 vertA = Support(s1, shape2);
                if (Vector3.Dot(vertA, direction) <= 0) return false;   //No collide
                simplex.Add(vertA);
                if (HandleSimplex())
                {
                    thisFrameCollisions.Add(new CachedCollision(shape2, baseIndices, baseVertices));
                    return true;
                }
                if (simplex.Count > 4) return false;
            }
        }

        //check collision with convex shape
        public bool GJKIntersection(CapsuleCollider shape2)
        {
            simplex = new List<Vector3>();
            //check cached collisions for improved efficiency
            Vector3[] s1 = parent.GetGlobalVerts();
            Vector3[] s2 = new Vector3[] { shape2.bottom.Center, shape2.top.Center };
            direction = shape2.Position - parent.Position;
            simplex.Add(Support(s1, s2, shape2.Radius));

            direction = Vector3.Negate(simplex[0]);

            while (true)
            {
                Vector3 vertA = Support(s1, s2, shape2.Radius);
                if (Vector3.Dot(vertA, direction) < 0) return false;   //No collide
                simplex.Add(vertA);
                if (HandleSimplex())
                {
                    return true;
                }
                if (simplex.Count > 4) return false;
            }
        }
        //check collision with convex shape
        public bool GJKIntersection(BoundingFrustum shape2)
        {
            loops = 0;
            simplex = new List<Vector3>();
            //check cached collisions for improved efficiency
            Vector3[] s1 = parent.GetGlobalVerts();
            Vector3[] s2 = shape2.GetCorners();
            Vector3 s2Origin = Vector3.Zero;
            
            foreach(Vector3 corner in s2)
            {
                s2Origin += corner;
            }
            s2Origin /= s2.Length;
            direction = s2Origin - parent.Position;
            simplex.Add(Support(s1, s2));

            direction = Vector3.Zero-(simplex[0]);

            while (true)
            {
                loops++;
                if (loops > s1.Length + s2.Length) return false; //no collision and break from infinite loop
                Vector3 vertA = Support(s1, s2);
                if (Vector3.Dot(vertA, direction) < 0) return false;   //No collide
                simplex.Add(vertA);
                if (HandleSimplex())
                {
                    thisFrameCollisions.Add(new CachedCollision(shape2, baseIndices, baseVertices));
                    return true;
                }
                if (simplex.Count > 4) return false;
            }
        }

        //check collision with Ray Cast
        public Vector3? GJKIntersection(Ray shape2)
        {
            loops = 0;
            simplex = new List<Vector3>();
            //check cached collisions for improved efficiency
            Vector3[] s1 = parent.GetGlobalVerts();
            Vector3[] s2 = new Vector3[] { shape2.Position, shape2.Position + (shape2.Direction * Vector3.Distance(parent.Position, shape2.Position)) };
            //used for generating cached collisions
            direction = shape2.Position - parent.Position;
            simplex.Add(Support(s1, s2));

            direction = Vector3.Negate(simplex[0]);

            while (true)
            {
                loops++;
                Vector3 vertA = Support(s1, s2);
                if (float.IsNaN(vertA.X)) return null;
                if (Vector3.Dot(vertA, direction) < 0) return null;   //No collide
                if (loops > s1.Length + s2.Length) return null;
                simplex.Add(vertA);
                if (HandleSimplex())
                {
                    Vector3 collisionPoint = baseVertices[0].Item1 + baseVertices[1].Item1 + baseVertices[2].Item1 + baseVertices[3].Item1;
                    collisionPoint /= 4;
                    return collisionPoint;
                }
                if (simplex.Count > 4) return null;
            }
        }

        #endregion

        #region Public Capsule Collision Functions
        //check collision with convex shape
        public bool GJKCapsuleIntersection(Shapes shape2)
        {
            loops = 0;
            simplex = new List<Vector3>();
            Vector3[] s1 = shape2.GetGlobalVerts();
            Vector3[] s2 = new Vector3[] { capsuleParent.bottom.Center, capsuleParent.top.Center };
            //checkcached points for better performance
            int? target = InCache(lastFrameCollisions, shape2);
            if (target != null)
            {
                CachedCollision collision = lastFrameCollisions[target.Value];
                direction = s2[collision.simplexIndices[0].Item2] - s1[collision.simplexIndices[0].Item1];
                simplex.Add(s1[collision.simplexIndices[0].Item1] - s2[collision.simplexIndices[0].Item2] + Vector3.Normalize(Vector3.Negate(direction)) * capsuleParent.Radius);
                direction = Vector3.Negate(simplex[0]);
            }
            else
            {
                direction = capsuleParent.Position - shape2.Position;
                if (float.IsNaN(direction.X)) direction = Vector3.Zero;
                simplex.Add(Support(s1, s2, capsuleParent.Radius));
                direction = Vector3.Negate(simplex[0]);
            }

            while (true)
            {
                loops++;
                Vector3 vertA = Support(s1, s2, capsuleParent.Radius);
                if (float.IsNaN(vertA.X)) return false;
                if (Vector3.Dot(vertA, direction) <= 0) return false;   //No collide
                if (loops > s1.Length+s2.Length) return false;
                simplex.Add(vertA);
                if (HandleSimplex())
                {
                    thisFrameCollisions.Add(new CachedCollision(shape2,baseIndices,baseVertices));
                    return true;
                }
                if (simplex.Count > 4) return false;
            }
        }
        #endregion

        #region Process Simplex Functions
        private bool HandleSimplex()
        {
            switch (simplex.Count)
            {
                case 2: return LineCase();
                case 3: return TriangleCase();
                case 4: return TetrahedronCase();
                default:
                    break;
            }

            //dont come here
            return false;
        }

        private bool LineCase()
        {
            Vector3 a = simplex[1];
            Vector3 b = simplex[0];
            Vector3 AB = b-a;
            Vector3 AO = Vector3.Negate(a);

            if (SameDirection(AB, AO))
            {
                direction = Vector3.Cross(Vector3.Cross(AB, AO), AB);
            }
            else
            {
                simplex = new List<Vector3>() { a };
                direction = AO;
            }

            return false;
        }

        private bool TriangleCase()
        {
            Vector3 a = simplex[2];
            Vector3 b = simplex[1];
            Vector3 c = simplex[0];

            Vector3 AB = b-a;
            Vector3 AC = c-a;
            Vector3 AO = Vector3.Negate(a);

            Vector3 ABC = Vector3.Cross(AB, AC);

            if (SameDirection(Vector3.Cross(ABC, AC), AO)){
                if (SameDirection(AC, AO))
                {
                    simplex = new List<Vector3>() { c,a };
                    direction = Vector3.Cross(Vector3.Cross(AC, AO), AC);
                }
                else
                {
                    simplex = new List<Vector3>() { b, a };
                    return LineCase();
                }
            }
            else
            {
                if (SameDirection(Vector3.Cross(AB, ABC), AO)) 
                {
                    simplex = new List<Vector3>() { b, a };
                    return LineCase();
                }
                else
                {
                    if (SameDirection(ABC, AO))
                    {
                        direction = ABC;
                    }
                    else
                    {
                        simplex = new List<Vector3>() { b,c,a };
                        direction = Vector3.Negate(ABC);
                    }
                }
            }
            return false;
        }

        private bool TetrahedronCase()
        {
            Vector3 a = simplex[3];
            Vector3 b = simplex[2];
            Vector3 c = simplex[1];
            Vector3 d = simplex[0];

            Vector3 AB = b-a;
            Vector3 AC = c-a;
            Vector3 AD = d-a;
            Vector3 AO = Vector3.Negate(a);

            Vector3 ABC = Vector3.Cross(AB, AC);
            Vector3 ACD = Vector3.Cross(AC, AD);
            Vector3 ABD = Vector3.Cross(AD, AB);

            if (SameDirection(ABC, AO))
            {
                simplex = new List<Vector3>() { c, b, a };
                return TriangleCase();
            }
            if (SameDirection(ACD, AO))
            {
                simplex = new List<Vector3>() { d, c, a };
                return TriangleCase();
            }
            if (SameDirection(ABD, AO))
            {
                simplex = new List<Vector3>() { b, d, a };
                return TriangleCase();
            }
            return true;

        }

        private bool SameDirection(Vector3 direction, Vector3 AO)
        {
            return Vector3.Dot(direction, AO) > 0;
        }

        #endregion

        #region Get Opposing Normals

        //get single collision direction
        public (Vector3,Vector3)? GetCollisionDirection(Shapes otherShape)
        {
            Vector3 s1Normal;
            Vector3 s2Normal;

            int? target = InCache(thisFrameCollisions, otherShape);

            if (target == null) return null;   //collision is not cached
            CachedCollision thisCol = thisFrameCollisions[target.Value];

            Vector3 a = thisCol.simplex[0].Item1;
            Vector3 b = thisCol.simplex[1].Item1;
            Vector3 c = thisCol.simplex[2].Item1;
            Vector3 d = thisCol.simplex[3].Item1;

            if (capsuleParent == null)
            {
                s1Normal = Vector3.Cross(b - a, c - a);
                s1Normal = Vector3.Normalize(s1Normal);
                if (Vector3.Dot(s1Normal, Vector3.Normalize(otherShape.Position)) > 0) s1Normal *= -1;  //flip normal to face away from interior of collider
                if (float.IsNaN(s1Normal.X)) s1Normal = Vector3.Zero;  //flip normal to face away from interior of collider

                a = thisCol.simplex[0].Item2;
                b = thisCol.simplex[1].Item2;
                c = thisCol.simplex[2].Item2;

                s2Normal = Vector3.Cross(b - a, c - a);
                s2Normal = Vector3.Normalize(s2Normal);
                if (Vector3.Dot(s2Normal, Vector3.Normalize(otherShape.Position)) > 0) s2Normal *= -1;  //flip normal to face away from interior of collider
                if (float.IsNaN(s2Normal.X)) s2Normal = Vector3.Zero;  //flip normal to face away from interior of collider

                return (s1Normal, s2Normal);
            }

            a = thisCol.simplex[0].Item1;
            b = thisCol.simplex[1].Item1;
            c = thisCol.simplex[2].Item1;
            d = thisCol.simplex[3].Item1;
            Vector3 simplex1_Center = (a + b + c + d) / 4;


            a = thisCol.simplex[0].Item2;
            b = thisCol.simplex[1].Item2;
            c = thisCol.simplex[2].Item2;
            d = thisCol.simplex[3].Item2;
            Vector3 simplex2_Center = (a + b + c + d) / 4;

            Vector3 returnDirect2 = simplex2_Center - simplex1_Center;
            returnDirect2.Normalize();

            if (Vector3.Distance(simplex1_Center, simplex2_Center) >= Vector3.Distance(simplex1_Center, simplex2_Center + returnDirect2)) returnDirect2 = Vector3.Zero - returnDirect2;
            if (float.IsNaN(returnDirect2.X) || returnDirect2 == null) returnDirect2 = Vector3.Zero;  //flip normal to face away from interior of collider

            return (Vector3.Zero, returnDirect2);
        }

        #endregion

        #region Support Functions
        //base support for arrays of vertices
        private Vector3 Support(Vector3[] s1, Vector3[] s2)
        {
            Vector3 v1 = GetFurthestPoint(s1, direction);
            Vector3 v2 = GetFurthestPoint(s2, Vector3.Negate(direction)) + Vector3.Normalize(Vector3.Negate(direction));
            baseVertices[simplex.Count] = (v1, v2);   //set current simplex indices
            baseIndices[simplex.Count] = (Array.IndexOf(s1, v1, 0), Array.IndexOf(s2, v2, 0)); //set current simplex indices
            return v1 - v2;
        }
        //furthest point for array of vertices
        private Vector3 GetFurthestPoint(Vector3[] shape, Vector3 direct)
        {
            Vector3 vertOut = shape[0];
            float largest = float.MinValue;
            foreach (Vector3 vert in shape)
            {
                float dist = Vector3.Dot(vert, direct);
                if (dist > largest) { vertOut = vert; largest = dist; }
            }
            return vertOut;
        }

        //Support function for Bounding Sphere
        private Vector3 Support(Vector3[] s1, BoundingSphere s2)
        {
            Vector3 v1 = GetFurthestPoint(s1, direction);
            Vector3 v2 = GetFurthestPoint(s2, Vector3.Negate(direction));
            baseVertices[simplex.Count] = (v1, v2);   //set current simplex indices
            baseIndices[simplex.Count] = (Array.IndexOf(s1, v1, 0), 0); //set current simplex indices

            return v1 - v2;
        }

        //Furthest point for Bounding Sphere
        private Vector3 GetFurthestPoint(BoundingSphere shape, Vector3 direct)
        {
            Vector3 vertOut = shape.Center + (Vector3.Normalize(direct) * shape.Radius);
            return vertOut;
        }
        //base support for capsule collider
        private Vector3 Support(Vector3[] s1, Vector3[] s2, short radius)
        {
            Vector3 v1 = GetFurthestPoint(s1, direction);
            Vector3 v2 = GetFurthestPoint(s2, Vector3.Negate(direction)) + Vector3.Normalize(Vector3.Negate(direction)) * radius;
            baseVertices[simplex.Count] = (v1,v2);   //set current simplex indices
            baseIndices[simplex.Count] = (Array.IndexOf(s1, v1, 0), Array.IndexOf(s2, v2, 0)); //set current simplex indices
            return v1 - v2;
        }
        #endregion

        #region CacheFunctions

        //reset cache at end of frame
        public void UpdateCache()
        {
            lastFrameCollisions.Clear();
            lastFrameCollisions = new List<CachedCollision>(thisFrameCollisions);
            thisFrameCollisions.Clear();
        }

        private int? InCache(List<CachedCollision> list, Shapes target)
        {
            int i = 0;
            if (list.Count == 0) return null;
            foreach(CachedCollision iter in list)
            {
                if (iter.collidedShape == target) return i;
                i++;
            }
            return null;
        }
        private int? InCache(List<CachedCollision> list, BoundingBox target)
        {
            int i = 0;
            foreach (CachedCollision iter in list)
            {
                if (iter.collidedBox == target) return i;
                i++;
            }
            return null;
        }
        private int? InCache(List<CachedCollision> list, BoundingSphere target)
        {
            int i = 0;
            foreach (CachedCollision iter in list)
            {
                if (iter.collidedSphere == target) return i;
                i++;
            }
            return null;
        }
        private int? InCache(List<CachedCollision> list, BoundingFrustum target)
        {
            int i = 0;
            foreach (CachedCollision iter in list)
            {
                if (iter.collidedFrustum == target) return i;
                i++;
            }
            return null;
        }

        #endregion

        #region Nested Classes and Structs

        private struct CachedCollision
        {
            public BoundingSphere? collidedSphere;
            public BoundingBox? collidedBox;
            public BoundingFrustum? collidedFrustum;
            public Shapes? collidedShape;
            public Type collidedObject;
            public (Vector3, Vector3)[] simplex;
            public (int, int)[] simplexIndices;

            #region constructors
            //Create cached shapes collision
            public CachedCollision(Shapes otherObj, (int, int)[] simplexInd_, (Vector3, Vector3)[] simplex_)
            {
                collidedBox = null;
                collidedFrustum = null;
                collidedSphere = null;
                collidedObject = typeof(Shapes);
                collidedShape = otherObj;
                simplex = simplex_;
                simplexIndices = simplexInd_;
            }

            //Create cached frustum collision
            public CachedCollision(BoundingFrustum otherObj, (int, int)[] simplexInd_, (Vector3, Vector3)[] simplex_)
            {
                collidedBox = null;
                collidedFrustum = otherObj;
                collidedSphere = null;
                collidedObject = typeof(BoundingFrustum);
                collidedShape = null;
                simplex = simplex_;
                simplexIndices = simplexInd_;
            }

            //Create cached boundingbox collision
            public CachedCollision(BoundingBox otherObj, (int, int)[] simplexInd_, (Vector3, Vector3)[] simplex_)
            {
                collidedBox = otherObj;
                collidedFrustum = null;
                collidedSphere = null;
                collidedObject = typeof(BoundingBox);
                collidedShape = null;
                simplex = simplex_;
                simplexIndices = simplexInd_;
            }

            //Create cached boundingsphere collision
            public CachedCollision(BoundingSphere otherObj, (int, int)[] simplexInd_, (Vector3, Vector3)[] simplex_)
            {
                collidedBox = null;
                collidedFrustum = null;
                collidedSphere = otherObj;
                collidedObject = typeof(BoundingSphere);
                collidedShape = null;
                simplex = simplex_;
                simplexIndices = simplexInd_;
            }
            #endregion

        }
        #endregion

    }
}
