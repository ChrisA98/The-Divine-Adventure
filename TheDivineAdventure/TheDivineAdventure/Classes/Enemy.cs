using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;

namespace TheDivineAdventure
{
    public class Enemy
    {
        ///////////////
        ///VARIABLES///
        ///////////////
        // Constant / Readonly
        // Make sure that the role and height have the same index
        // (EX: DEMON is at index 0 of ROLES, while DEMON_HEIGHT is also at index 0 of HEIGHTS)
        public static readonly string[] ROLES = { "DEMON", "HELLHOUND", "GOBLIN", "SKELETON" };
        private const int DEMON_HEIGHT = 21;
        private const int HELLHOUND_HEIGHT = 13;
        private const int GOBLIN_HEIGHT = 0;
        private const int SKELETON_HEIGHT = 0;
        private static readonly int[] HEIGHTS = { DEMON_HEIGHT, HELLHOUND_HEIGHT, GOBLIN_HEIGHT, SKELETON_HEIGHT };

        // Info
        private string role;
        private int height;
        public List<Attack> projList = new List<Attack>();
        private float health;
        private double speedFactor;
        private Boolean ranged;
        private bool timeToDestroy;
        private PlayScene parentScene;

        //random spawning
        Random randX = new Random();
        Random randZ = new Random();

        // Movement
        private Vector3 pos;
        private float rot;
        private Vector3 enemyDir;

        // Timer
        private float maxTime; //time between shots
        private float timer;


        // Sound
        private List<SoundEffect> soundEffects;
        public static float volume;

        /////////////////
        ///CONSTRUCTOR///
        /////////////////
        public Enemy(List<SoundEffect> s, string r, Vector3 pPos, PlayScene parent)
        {
            maxTime = 2.5f;
            soundEffects = s;
            role = r;
            height = HEIGHTS[Array.IndexOf(ROLES, role)];
            pos = new Vector3((float)randX.Next(0, 40), 0 - height, (float)randZ.Next((int)pPos.Z + 200, (int)pPos.Z + 500));
            parentScene = parent;

            //adjust orientation and enemy values for health/speed
            rot = 180f;
            switch (role)
            {
                case "RED_DEMON":
                    health = 100;
                    speedFactor = 2;
                    ranged = true;
                    break;
                case "BLUE_DEMON":
                    health = 50;
                    speedFactor = 2;
                    ranged = false;
                    break;
                case "GREEN_DEMON":
                    health = 100;
                    speedFactor = 1;
                    ranged = false;
                    break;
                case "YELLOW_DEMON":
                    health = 100;
                    speedFactor = 1;
                    ranged = true;
                    break;
                default:
                    health = 100;
                    speedFactor = 1;
                    ranged = false;
                    break;

            }
        }



        ///////////////
        ///FUNCTIONS///
        ///////////////


        public void Update(float dt, Player player)
        {
            //handle spawning before moving and shooting
            //spawns under the level and rises up
            if (Pos.Y < Height)
                Pos += new Vector3(0, 2f, 0);
            if (pos.Y >= height)
            {
                facePlayer(player);
                Move(dt, player);
                Shoot(dt);
            }
            if (health <= 0)
            {
                PlayScene.score += 100;
                timeToDestroy = true;
            }

        }

        public void facePlayer(Player player)
        {
            //change enemies orientation, face to the player
            rot = (float)(Math.Atan2(player.Pos.X - pos.X, player.Pos.Z - pos.Z));
            //rot *= -1;
        }

        private void Move(float dt, Player player)
        {
            // Move forward
            if (Vector3.Distance(player.Pos, pos) > 15 && ranged == false)
            {
                pos += Vector3.Transform(
                        Vector3.Backward,
                        Matrix.CreateRotationY(rot)) * (float)speedFactor;
            }
            else
            {
                //ranged enemy needs to keep at a distance if possible!
                if (Vector3.Distance(player.Pos, pos) > 150)
                {
                    pos += Vector3.Transform(
                        Vector3.Backward,
                        Matrix.CreateRotationY(rot)) * (float)speedFactor;
                }
                else if (Vector3.Distance(player.Pos, pos) < 150)
                {
                    //try and backup if possible
                    //meaning not clipping out of the map (temp values are used for now!)
                    if (this.pos.X > -100 && this.pos.X < 100 && this.pos.Z > 0 && this.pos.Z < 3500)
                        pos -= Vector3.Transform(
                            Vector3.Backward,
                            Matrix.CreateRotationY(rot)) * (float)speedFactor;
                }
            }
        }


        private void Shoot(float dt)
        {
            if (timer > 0)
            {
                timer = timer - dt;
            }
            else
            {
                soundEffects[0].Play(volume: volume, pitch: 0.0f, pan: 0.0f);
                enemyDir = Vector3.Transform(Vector3.Backward,
                Matrix.CreateRotationY(rot));

                AttackPattern.singleProj(this.Pos, enemyDir, 15f, 50, projList, parentScene.Camera);
                timer = maxTime;

            }

            foreach (Attack p in projList)
            {
                if (p.TimeToDestroy)
                {
                    projList.Remove(p);
                    break;
                }
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

        public float Rot
        {
            get { return rot; }
            set { Rot = value; }
        }

        public int Height
        {
            get { return height; }
            set { height = value; }
        }

        public string Role
        {
            get { return role; }
            set { role = value; }
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
    }
}
