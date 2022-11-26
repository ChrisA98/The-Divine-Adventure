using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using TheDivineAdventure.SkinModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace TheDivineAdventure
{
    public class Boss
    {
        ///////////////
        ///VARIABLES///
        ///////////////

        // Info
        GraphicsDevice gpu;
        protected int height;
        public List<Attack> projList = new List<Attack>();
        protected float health, maxHealth;
        protected string attackType;  //types of attacks the creatures has
        protected bool timeToDestroy;
        protected PlayScene parentScene;
        protected Player player;
        public bool isActive;
        public string name;

        //Movement Variables
        protected float vel;  //Current velocity
        protected float speedMax;
        protected float accel;    //Acceleration
        protected float runMod;   //speed modifcation when running (Mostly running, but may add idle walking when player is hidden)
        protected bool onGround;
        protected float fallSpeed;
        protected List<Vector3> cachedPosition;
        protected int timeColiding;


        //random spawning
        protected Random rand = new Random();

        // Movement
        protected Vector3 pos;
        protected Vector3 rot;
        public Matrix world;
        protected float prefDistance; //preferred distance from player
        public bool isRunning;

        // Attack Info
        protected float attackSpeed; //time between attacks
        protected float attackTimer;  //Keep time on attacking
        protected float attackRange;
        protected bool isAttacking;
        protected float attackDamage;
        protected float attackLength; // how long into animation befre attack hits
        protected float attDurTimer; //timer for when to attack during animation
        protected float maxAttackRange; //maxiumum distance to shoot
        protected int phase, phaseTimer;        //attack phase and timer
        protected float phaseStartHealth;

        //hitbox info
        public CapsuleCollider boundingCollider;


        // Sound
        protected List<SoundEffect> soundEffects;
        public static float volume;

        //Animations
        protected float[] animWeights;
        protected SkinModelLoader loader;
        protected SkinModel[] animations;
        protected SkinFx skinFx;
        protected bool isMeleeAttacking;
        protected bool waking; //slowly wake up timer
        protected bool doRender;

        /////////////////
        ///CONSTRUCTOR///
        /////////////////
        public Boss(List<SoundEffect> s, Vector3 spawnLoc, PlayScene parent, ContentManager content)
        {
            gpu = parent.gpu;
            soundEffects = s;
            height = 0;
            pos = spawnLoc*2.2f;
            parentScene = parent;
            player = parent.player;
            FaceTarget(parent.player.world);

            //Attack Info
            isAttacking = false;
            isActive = false;
            isMeleeAttacking = false;
            attackTimer = 0f;
            attDurTimer = 0f;

            //Location and movement info
            world = Matrix.CreateScale(1f) *
                        Matrix.CreateRotationY(MathHelper.ToRadians(Rot.Y)) *
                        Matrix.CreateTranslation(Pos);
            isRunning = false;
            cachedPosition = new List<Vector3>
            {
                pos,
                pos,
                pos,
                pos,
                pos,
                pos,
                pos
            };
            timeColiding = 0;

            //Loader and effect
            loader = new SkinModelLoader(content, parent.gpu);
            loader.SetDefaultOptions(0.1f, "default_tex");
            animations = new SkinModel[7];
            skinFx = new SkinFx(content, parent.Camera, "SkinEffect");

            //wakeup timer
            waking = true;
        }


        ///////////////
        ///FUNCTIONS///
        ///////////////


        public virtual void Update(float dt, GameTime gametime, Camera cam)
        {

            //apply transformations
            world = Matrix.CreateScale(1f) *
                        Matrix.CreateRotationY(MathHelper.ToRadians(Rot.Y)) *
                        Matrix.CreateTranslation(Pos);

            //update hitbox
            boundingCollider.Update(Pos);


            if (waking)
            {                
                skinFx.SetDiffuseCol(Color.White.ToVector4());
                skinFx.SetSpecularCol(Color.Red.ToVector3());
                skinFx.SetSpecularPow(255f);
                skinFx.SetFogColor(Color.Red);
                foreach (SkinModel sk in animations)
                {
                    if (sk != null && sk != animations[0])
                    {
                        sk.BeginAnimation(0, gametime);
                    }
                }
                waking = false;
            }

            if (!isActive) return; //enemy is turned off

            foreach (Attack p in projList)
            {
                p.Update(dt, player, parentScene);
                if (p.TimeToDestroy)
                {
                    projList.Remove(p);
                    break;
                }
            }
            if (health <= 0)
            {
                //death of entity
                PlayScene.score += 1200;
                timeToDestroy = true;
            }


            //check phase
            PhaseUpdate(gametime);


            //update AI
            AIProcessing(phase, cam);
            

        }
        
        protected virtual void Attack(bool isMelee, Camera cam)
        {          
            attDurTimer = 0;
            attackTimer = rand.Next((int)(attackSpeed*-.5f),0);
        }

        public virtual void Damage(float amt)
        {
            health -= amt;
        }

        //check for phase update
        protected virtual void PhaseUpdate(GameTime gt)
        {
            //Virtual function to be used by children classes
        }

        #region <<AI Processing>>
        protected virtual void AIProcessing(int Phase, Camera cam)
        {
            //Virtual function to be used by children classes
        }

        public void FaceTarget(Matrix targ)
        {
            //change enemies orientation, face to the player
            rot.Y = MathHelper.ToDegrees((float)(Math.Atan2(targ.Translation.X - Pos.X, targ.Translation.Z - Pos.Z )));
        }
        public void FaceAwayTarget(Matrix targ)
        {
            //change enemies orientation, face to the player
            rot.Y = MathHelper.ToDegrees((float)(Math.Atan2(Pos.X - targ.Translation.X, Pos.Z- targ.Translation.Z)));
        }

        protected void Move()
        {
            Vector3 walkDir = world.Backward;
            // Move forward
            if (Vector3.Distance(player.Pos, Pos) > prefDistance)
            {
                if (vel < 0) vel /= 3;
                if (vel < Math.Abs(speedMax)) vel += accel;
            }
            else if (Vector3.Distance(player.Pos, Pos) < prefDistance)
            {
                if(vel > 0) vel /= 3;
                if (vel < Math.Abs(speedMax)) vel -= accel;
            }
            if (boundingCollider.Intersects(player.boundingCollider))
            {
                walkDir = ManageCollision(player.world, walkDir);
                vel *= 0.5f;
            }

            //>>Collision<<
            bool staticColission = false;
            foreach (PlayScene.EnemySpawner spawner in parentScene.levelSpawnerList)
            {
                if (spawner.isActive == false) continue;
                foreach (Enemy entity in spawner.enemyList)
                {
                    if (boundingCollider.Intersects(entity.boundingCollider))
                    {
                        walkDir = ManageCollision(entity.world, walkDir);
                    }
                }
            }
            foreach (Level.StaticCollisionMesh mesh in parentScene.thisLevel.StaticCollisionMeshes)
            {
                if (Vector3.Distance(mesh.Position, pos) < 400)
                {
                    if (boundingCollider.Intersects(mesh.collider))
                    {
                        StaticMeshCollsion(mesh.collider);
                        staticColission = true;
                        walkDir = Vector3.Zero;
                        vel *= 0.5f;
                    }
                }
            }
            foreach (Level.InteractiveDoor mesh in parentScene.thisLevel.Doors)
            {
                if (Vector3.Distance(mesh.Position, pos) < 400)
                {
                    if (boundingCollider.Intersects(mesh.leftCollider))
                    {
                        StaticMeshCollsion(mesh.leftCollider);
                        staticColission = true;
                        walkDir = Vector3.Zero;
                        vel *= 0.5f;
                    }
                    if (boundingCollider.Intersects(mesh.rightCollider))
                    {
                        StaticMeshCollsion(mesh.rightCollider);
                        staticColission = true;
                        walkDir = Vector3.Zero;
                        vel *= 0.5f;
                    }
                }
            }



            //Cache position
            if (Vector3.Distance(pos, cachedPosition[6]) > 1f && !staticColission)
            {
                cachedPosition.Add(pos); //set last position
                cachedPosition.RemoveAt(0);
            }

            pos += walkDir * vel;

            //Fall
            foreach (Level.FloorTile floor in parentScene.thisLevel.FloorTiles)
            {
                if (Vector3.Distance(floor.Position, pos) < 600)
                {
                    if (boundingCollider.Intersects(floor.collider))
                    {
                        if (fallSpeed == 0)
                        {
                            pos -= 3 * world.Down;
                        }
                        pos -= fallSpeed*world.Down;
                        fallSpeed = 0;
                        onGround = true;
                        break;
                    }
                    onGround = false;
                }
            }

            Fall();
        }
        #endregion

        protected virtual void Fall()
        {
            if (!onGround) fallSpeed += parentScene.parent.gravity;
            pos.Y -= fallSpeed;

            // Once on the ground dont fall more
            if (onGround)
            {
                fallSpeed = 0;
            }

        }


        public Rectangle resourceBarUpdate(Rectangle bar, Vector2 screen, Vector2 scale)
        {
            int nWid;
            Rectangle newRect;

            //set width of bar based on current stat
            nWid = (int)Math.Round(health / maxHealth * (0.226f * screen.X));
            //define new rectangle for bar
            newRect = new Rectangle(
                (int)Math.Round(.387f * screen.X),
                (int)Math.Round(.892f * screen.Y),
                nWid,
                (int)Math.Round(bar.Height * scale.Y));

            return newRect;
        }


        #region <<Animations & Rendering>>

        public void Draw(GameTime gametime, Camera cam)
        {
            doRender = boundingCollider.Intersects(cam.renderSpace);

            //animation updates
            if (parentScene.parent.currentScene != "PAUSE" && doRender)
            {
                foreach (SkinModel an in animations)
                {
                    if (an != null && an.loopAnimation == true)
                    {
                        an.Update(gametime);
                    }
                }
            }

            MakeLocal();
            BlendAnims(gametime);

            //dont render stuff out of view
            if (!doRender) return;

            for (int i = 0; i < animations[0].meshes.Length; i++)
            {
                animations[0].DrawMesh(i, cam, world);
            }
        }

        //places idle animation on global mesh reference
        protected void MakeLocal()
        {
            //reset weights for blending
            SetAnimBlends(1);
            animations[0].UpdateBlendAnim(animations, 0, 1, 1, animWeights);   //idle -> walkBack
        }

        protected virtual void BlendAnims(GameTime gameTime)
        {
            //placeholder for reference by children classes
        }

        //set specific playerAnimWeights
        protected void SetAnimBlends(int start, int end, float val)
        {
            for (int i = start; i <= end; i++)
            {
                animWeights[i] = val;
            }
        }

        //Set all AnimWeights
        protected void SetAnimBlends(int val)
        {
            for (int i = 0; i <= animWeights.Length - 1; i++)
            {
                animWeights[i] = val;
            }
        }

        #endregion

        #region Collision Management
        public Vector3 ManageCollision(Matrix targetPos, Vector3 currentDir)
        {
            float tarRotY = (float)(Math.Atan2(targetPos.Translation.X - Pos.X, targetPos.Translation.Z - Pos.Z));
            Matrix away = Matrix.CreateScale(1f) *
                Matrix.CreateRotationY(tarRotY) *
                Matrix.CreateTranslation(pos);
            if (away.Backward == currentDir) return Vector3.Zero;
            currentDir -= away.Backward;
            currentDir.Normalize();
            return currentDir;
        }

        private bool StaticMeshCollsion(Shapes collisionbox)
        {
            if (boundingCollider.Intersects(collisionbox))
            {
                timeColiding++;
                if (timeColiding > 5)
                {
                    pos = cachedPosition[0];
                    timeColiding = 0;
                    return true;
                }
                Vector3 newDir = boundingCollider.Collisiondirection(collisionbox);
                if (newDir.X + newDir.Y < 0.4) { pos = cachedPosition[6]; return true; }
                if (float.IsNaN(newDir.X)) { Debug.WriteLine("broken"); pos = cachedPosition[3]; return true; }
                pos += newDir * vel;
                vel *= 0.5f;
            }
            return false;
        }

        #endregion
        ////////////////////
        ///GETTER/SETTERS///
        ////////////////////
        #region Getters / Setters
        public Vector3 Pos
        {
            get { return pos; }
            set { pos = value; }
        }

        public Vector3 Rot
        {
            get { return rot; }
            set { Rot = value; }
        }

        public int Height
        {
            get { return height; }
            set { height = value; }
        }
        public bool TimeToDestroy
        {
            get { return timeToDestroy; }
            set { timeToDestroy = value; }
        }

        public float Health
        {
            get { return health; }
            set { health = value; }
        }
        #endregion
    }
}
