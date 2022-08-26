using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace TheDivineAdventure
{
    class Attack
    {
        ///////////////
        ///VARIABLES///
        ///////////////
        // Constant / Readonly

        // Info
        private bool isMelee, timeToDestroy; //destroys object when true
        private float damage;

        // Movement
        private Matrix cameraLookAt;
        private Vector3 initPos, pos, rot;
        private Vector3 dest, vel;
        private float distance;
        private float speed;

        // Timer
        public float timer;        //how long the projectile stays active


        // Sound

        /////////////////
        ///CONSTRUCTOR///
        /////////////////
        public Attack(Vector3 origin, Vector3 target, float pSpeed, float damageAmt)
        {
            // Set the projectiles initial position
            initPos = origin;
            pos = initPos;


            //cameraLookAt = origin;


            // Set the projectiles destintation
            dest.X = target.X - pos.X;
            dest.Y = target.Y - pos.Y;
            dest.Z = target.Z - pos.Z;

            // Find the distance between them
            distance = (float)Math.Sqrt(
                        target.X * target.X
                        + target.Y * target.Y
                        + target.Z * target.Z);

            // Determine the velocity on each axis using the distance
            if (pSpeed <= 0)
            {
                isMelee = true;
            }
            else
            {
                isMelee = false;
                speed = pSpeed * 50;
                vel.X = (target.X / distance) * speed;
                vel.Y = (target.Y / distance) * speed;
                vel.Z = (target.Z / distance) * speed;
            }

            // Timer stats
            if (isMelee)
                timer = 0.1f;
            else
                timer = 1f;
            timeToDestroy = false;

            //define damage amount
            damage = damageAmt;
        }

        ///////////////
        ///FUNCTIONS///
        ///////////////
        public void Update(float dt, Player player)
        {
            if (timer > 0f)
            {
                if (CheckCollision(player))
                {
                    player.Health -= damage;
                    timeToDestroy = true;
                }
                else
                {
                    pos.X += vel.X * dt;
                    pos.Y += vel.Y * dt;
                    pos.Z += vel.Z * dt;
                }

                timer = timer - dt;
            }
            else
            {
                timeToDestroy = true;
            }
        }

        public void Update(float dt, List<Enemy> enemyList)
        {
            if (timer > 0f)
            {
                foreach (Enemy e in enemyList)
                {
                    if (CheckCollision(e))
                    {
                        e.Health -= damage;
                        timeToDestroy = true;
                    }
                }
                pos.X += vel.X * dt;
                pos.Y += vel.Y * dt;
                pos.Z += vel.Z * dt;

                timer = timer - dt;
            }
            else
            {
                timeToDestroy = true;
            }
        }

        private bool CheckCollision(Player player)
        {
            if (isMelee)
            {
                return false;
            }
            else
            {
                if (this.boundingSphere.Intersects(new BoundingBox(
                    new Vector3(player.Pos.X - 5, player.Pos.Y - player.Height, player.Pos.Z - 5),
                    new Vector3(player.Pos.X + 5, player.Pos.Y + player.Height, player.Pos.Z + 5))))
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
                if (this.boundingBox.Intersects(new BoundingBox(
                    new Vector3(enemy.Pos.X - 5, enemy.Pos.Y - enemy.Height, enemy.Pos.Z - 5),
                    new Vector3(enemy.Pos.X + 5, enemy.Pos.Y + enemy.Height, enemy.Pos.Z + 5))))
                {
                    return true;
                }
                return false;
            }
            else
            {
                if (this.boundingSphere.Intersects(new BoundingBox(
                    new Vector3(enemy.Pos.X - 5, enemy.Pos.Y - enemy.Height - 5, enemy.Pos.Z - 10),
                    new Vector3(enemy.Pos.X + 5, enemy.Pos.Y + enemy.Height + 5, enemy.Pos.Z + 10))))
                {
                    return true;
                }
                return false;
            }
        }
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
            get { return new BoundingSphere(pos, 0.015f); }
        }

        private BoundingBox boundingBox
        {
            get { return new BoundingBox(pos - new Vector3(30, 500, 5), pos + new Vector3(30, 500, 10)); }
        }
    }
}
