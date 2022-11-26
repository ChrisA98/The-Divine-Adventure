using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using TheDivineAdventure;

namespace TheDivineAdventure
{
    public class Attack
    {
        ///////////////
        ///VARIABLES///
        ///////////////
        // Constant / Readonly

        // Info
        private bool isMelee, timeToDestroy; //destroys object when true
        private float damage;
        private float force;
        private Camera cam;
        private Shapes hitbox;
        public int projScale;

        // Movement
        public Matrix cameraLookAt;
        private Vector3 initPos, pos;
        private Vector3 vel;
        private float speed;

        // Timer
        public float timer;        //how long the projectile stays active

        // Sound

        //Lighting
        public Color color;

        #region <<Constructors>>

        // -- Ranged Attack --
        public Attack(Vector3 origin, Vector3 target, float pSpeed, float damageAmt, Camera cam, Color col,  float force_ = 1, int scale = 1, int duration = 1)
        {
            // Set the projectiles initial position
            initPos = origin;
            pos = initPos;

            // Set the projectiles destintation
            vel = target - origin;
            vel.Normalize();

            //define hitbox cube
            hitbox = new Shapes(cam.gpu, Color.Red, Vector3.Zero, origin + target, this.GetType());
            hitbox.DefineCuboid(Vector3.One);

            projScale = scale;

            isMelee = false;
            speed = pSpeed * 50;
            vel *= speed;

            // Timer stats
            timer = duration;
            timeToDestroy = false;

            //define damage amount
            damage = damageAmt;
            force = force_;

            //set color
            color = col;
        }

        // -- Melee Attack --
        public Attack(Vector3 origin, Vector3 target, Vector3 rot, float damageAmt, Vector3 attackScale, Camera cam_, float force_ = 1)
        {
            // Set the projectiles initial position
            initPos = origin;
            pos = initPos;


            // Set the projectiles destintation
            vel = target - origin;
            vel = Vector3.Normalize(vel);

            cam = cam_;

            //define hitbox cube
            hitbox = new Shapes(cam.gpu, Color.Red, rot, target+origin, GetType());
            hitbox.DefineCuboid(attackScale);
            hitbox.Position = pos;
            hitbox.Rotation = rot;


            isMelee = true;     //set attack type

            // Timer stats
            timer = 0.1f;
            timeToDestroy = false;

            damage = damageAmt;     //define damage amount
            force = force_;
        }

        #endregion

        ///////////////
        ///FUNCTIONS///
        ///////////////
        public void Update(float dt, Player player, PlayScene parent)
        {
            if (timer <= 0f) { timeToDestroy = true; return; }
            if (CheckCollision(player))
            {
                player.Damage(damage,force,(player.Pos- initPos));
                timeToDestroy = true;
                return;
            }
            foreach(Level.StaticCollisionMesh mesh in parent.thisLevel.StaticCollisionMeshes)
            {
                if(CheckCollision(mesh.collider)) { timeToDestroy = true; return; }
            }
            foreach (Level.InteractiveDoor mesh in parent.thisLevel.Doors)
            {
                if (CheckCollision(mesh.leftCollider)) { timeToDestroy = true; return; }
                if (CheckCollision(mesh.rightCollider)) { timeToDestroy = true; return; }
            }
            hitbox.Position = pos;
            pos += vel * dt;
            timer -= dt;
        }

        public void Update(float dt, PlayScene.EnemySpawner[] spawnerList, Boss boss, PlayScene parent)
        {
            if (timer <= 0f) timeToDestroy = true;
            foreach (PlayScene.EnemySpawner spawner in spawnerList)
            {
                if (spawner.isActive == false) continue;
                foreach (Enemy e in spawner.enemyList)
                {
                    if (CheckCollision(e))
                    {
                        e.Damage(damage, force);
                        timeToDestroy = true;
                    }
                }
            }
            foreach (Enemy e in parent.worldEnemyList)
            {
                if (CheckCollision(e))
                {
                    e.Damage(damage, force);
                    timeToDestroy = true;
                }
            }
            if (CheckCollision(boss))
            {
                boss.Damage(damage);
                timeToDestroy = true;
            }
            foreach (Level.StaticCollisionMesh mesh in parent.thisLevel.StaticCollisionMeshes)
            {
                if (CheckCollision(mesh.collider)) { timeToDestroy = true; return; }
            }
            foreach (Level.InteractiveDoor mesh in parent.thisLevel.Doors)
            {
                if (CheckCollision(mesh.leftCollider)) { timeToDestroy = true; return; }
                if (CheckCollision(mesh.rightCollider)) { timeToDestroy = true; return; }
            }
            hitbox.Position = pos;
            pos += vel * dt;
            timer -= dt;
        }

        #region Collision Checks
        private bool CheckCollision(Player player)
        {
            if (isMelee)
            {
                if (hitbox.Intersects(player.boundingCollider))
                {
                    return true;
                }
                return false;
            }
            else
            {
                if (player.boundingCollider.Intersects(boundingSphere))
                {
                    return true;
                }
                return false;
            }
        }

        private bool CheckCollision(Enemy enemy)
        {
            if (isMelee)
            {
                if (hitbox.Intersects(enemy.boundingCollider))
                {
                    return true;
                }
                return false;
            }
            else
            {
                if (enemy.boundingCollider.Intersects(boundingSphere))
                {
                    return true;
                }
                return false;
            }
        }
        
        private bool CheckCollision(Boss boss)
        {
            if (boss == null) return false;
            if (isMelee)
            {
                if (hitbox.Intersects(boss.boundingCollider))
                {
                    return true;
                }
                return false;
            }
            else
            {
                if (boss.boundingCollider.Intersects(boundingSphere))
                {
                    return true;
                }
                return false;
            }
        }

        private bool CheckCollision(Shapes actor)
        {
            if (isMelee)
            {
                if (hitbox.Intersects(actor))
                {
                    return true;
                }
                return false;
            }
            else
            {
                if (actor.Intersects(boundingSphere))
                {
                    return true;
                }
                return false;
            }
        }
        #endregion
        
        ////////////////////
        ///GETTER/SETTERS///
        ////////////////////
        public Vector3 Pos
        {
            get { return pos; }
            set { pos = value; }
        }

        public bool TimeToDestroy
        {
            get { return timeToDestroy; }
            set { timeToDestroy = value; }
        }

        public bool IsMelee
        {
            get { return isMelee; }
            set { isMelee = value; }
        }

        private BoundingSphere boundingSphere
        {
            get { return new BoundingSphere(pos, 2.7f*projScale); }
        }
        public Shapes HitBox
        {
            get { return hitbox; }
        }
    }
}
