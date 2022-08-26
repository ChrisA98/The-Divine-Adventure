using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace TheDivineAdventure
{
    class Player
    {
        ///////////////
        ///VARIABLES///
        ///////////////
        // Constant / Readonly
        // Make sure that the role, height, and width have the same index
        // (EX: WARRIOR is at index 0 of ROLES, while WARRIOR_HEIGHT is also at index 0 of HEIGHTS)
        public static readonly string[] ROLES = { "WARRIOR", "ROGUE", "MAGE", "CLERIC" };
        private const int WARRIOR_HEIGHT = 13;
        private const int ROGUE_HEIGHT = 13;
        private const int MAGE_HEIGHT = 13;
        private const int CLERIC_HEIGHT = 13;
        private static readonly int[] HEIGHTS = { WARRIOR_HEIGHT, ROGUE_HEIGHT, MAGE_HEIGHT, CLERIC_HEIGHT };
        private const int WARRIOR_WIDTH = 9;
        private const int ROGUE_WIDTH = 9;
        private const int MAGE_WIDTH = 9;
        private const int CLERIC_WIDTH = 9;
        private static readonly int[] WIDTHS = { WARRIOR_HEIGHT, ROGUE_HEIGHT, MAGE_HEIGHT, CLERIC_HEIGHT };

        // Info
        public string role;
        private int height, width;
        public List<Attack> projList = new List<Attack>();
        private Game1 parent;
        GraphicsDevice gpu;

        // Movement
        private Vector3 pos;
        private Vector3 rot;
        public float speed, initSpeed, runSpeed;


        // Jumping
        private float fallSpeed = 0;
        private bool jumping = false;
        public float jumpSpeed;
        private float minHeight;

        // Sound
        private List<SoundEffect> soundEffects;
        public static float volume;

        // KeyboardState
        private KeyboardState prevKeyboardState;
        private KeyboardState curKeyboardState;

        //Health and secondary stat
        public float health, secondary, secondaryRegenRate, runCost;
        public int healthMax, secondaryMax;
        public int attCost, spec1Cost, spec2Cost, spec3Cost;
        private float projSpeed;
        private bool isExhausted;
        //swaps stamina for mana when true
        private bool isCaster;

        //attack stats
        private float attDmg1, attDmg2;

        private bool atEnd;

        //world matrix
        public Matrix world;
        //head  matrix
        public Matrix head;


        // Mouse
        private MouseState currMouseState;
        private MouseState previousMouseState;

        // Timer
        private float maxAttTime, maxSpec1Time, maxSpec2Time, maxSpec3Time; //time between shots
        private float attTimer, spec1Timer, spec2Timer, spec3Timer;

        /////////////////
        ///CONSTRUCTOR///
        /////////////////
        public Player(List<SoundEffect> s, string r, Game1 parent)
        {

            // Set gpu
            gpu = parent.GraphicsDevice;


            // Imported values
            soundEffects = s;
            role = r;
            this.parent = parent;

            // Set player position
            height = HEIGHTS[Array.IndexOf(ROLES, role)];
            width = WIDTHS[Array.IndexOf(ROLES, role)];
            pos = new Vector3(0, height, 0);
            rot = new Vector3(0, -5, 0);
            minHeight = pos.Y;

            // Prepare mouse state
            previousMouseState = Mouse.GetState();

            attTimer = 0f;
            spec1Timer = 0f;
            spec2Timer = 0f;
            spec3Timer = 0f;

            isExhausted = false;

            world = Matrix.CreateScale(1f) *
                        Matrix.CreateRotationY(MathHelper.ToRadians(Rot.Y)) *
                        Matrix.CreateTranslation(Pos);
            head = Matrix.CreateScale(1f) *
                        Matrix.CreateRotationX(MathHelper.ToRadians(Rot.X)) *
                        Matrix.CreateRotationY(MathHelper.ToRadians(Rot.Y)) *
                        Matrix.CreateTranslation(Pos);

            // Set player stats
            switch (this.role)
            {
                case "WARRIOR":
                    isCaster = false;
                    initSpeed = 50f;
                    speed = initSpeed;
                    runSpeed = 2f;
                    jumpSpeed = 2f;
                    healthMax = 300;
                    health = healthMax;
                    secondaryMax = 100;
                    secondary = secondaryMax;
                    secondaryRegenRate = 0.15f;
                    projSpeed = 0f;
                    maxAttTime = 0.5f;
                    maxSpec1Time = 0.5f;
                    maxSpec2Time = 0.5f;
                    maxSpec3Time = 0.5f;
                    attCost = 10;
                    spec1Cost = 20;
                    spec2Cost = 30;
                    spec3Cost = secondaryMax;
                    runCost = 0.2f;
                    attDmg1 = 500f;
                    attDmg2 = 250f;
                    break;
                case "ROGUE":
                    isCaster = false;
                    initSpeed = 90f;
                    speed = initSpeed;
                    runSpeed = 3f;
                    jumpSpeed = 3f;
                    healthMax = 100;
                    health = healthMax;
                    secondaryMax = 300;
                    secondary = secondaryMax;
                    secondaryRegenRate = 1f;
                    projSpeed = 0f;
                    maxAttTime = 0.1f;
                    maxSpec1Time = 0.5f;
                    maxSpec2Time = 0.5f;
                    maxSpec3Time = 0.5f;
                    attCost = 1;
                    spec1Cost = 20;
                    spec2Cost = 30;
                    spec3Cost = secondaryMax;
                    runCost = 2f;
                    attDmg1 = 500f;
                    attDmg2 = 250f;
                    break;
                case "MAGE":
                    isCaster = true;
                    initSpeed = 80f;
                    speed = initSpeed;
                    runSpeed = 1f;
                    jumpSpeed = 1.5f;
                    healthMax = 100;
                    health = healthMax;
                    secondaryMax = 300;
                    secondary = secondaryMax;
                    secondaryRegenRate = 0.23f;
                    projSpeed = 5f;
                    maxAttTime = 0.5f;
                    maxSpec1Time = 0.5f;
                    maxSpec2Time = 0.5f;
                    maxSpec3Time = 0.5f;
                    attCost = 10;
                    spec1Cost = 20;
                    spec2Cost = 30;
                    spec3Cost = 50;
                    runCost = 0f;
                    attDmg1 = 50f;
                    attDmg2 = 25f;
                    break;
                case "CLERIC":
                    isCaster = true;
                    initSpeed = 55f;
                    speed = initSpeed;
                    runSpeed = 1f;
                    jumpSpeed = 2f;
                    healthMax = 300;
                    health = healthMax;
                    secondaryMax = 100;
                    secondary = secondaryMax;
                    secondaryRegenRate = 0.25f;
                    projSpeed = 15f;
                    maxAttTime = 0.5f;
                    maxSpec1Time = 0.5f;
                    maxSpec2Time = 0.5f;
                    maxSpec3Time = 0.5f;
                    attCost = 10;
                    spec1Cost = 20;
                    spec2Cost = 30;
                    spec3Cost = 50;
                    runCost = 0f;
                    attDmg1 = 50f;
                    attDmg2 = 25f;
                    break;
            }

            //accommodate for changes caused by deltaTime
        }



        ///////////////
        ///FUNCTIONS///
        ///////////////
        public void Update(float dt, Camera cam)
        {

            // Variables
            currMouseState = Mouse.GetState();
            curKeyboardState = Keyboard.GetState();

            // Regular Gameplay
            Move(dt);
            Abilities(dt, cam);

            // Debugging
            //DebugMode();
            Mouse.SetPosition(gpu.Viewport.Width / 2, gpu.Viewport.Height / 2);

            previousMouseState = Mouse.GetState(); ;
            prevKeyboardState = curKeyboardState;

        }

        private void Move(float dt)
        {

            // Handle mouse movement
            float deltaX;
            float deltaY;

            // Only handle mouse movement code if the mouse has moved (the state has changed)
            if (currMouseState != previousMouseState)
            {
                // Cache mouse location
                deltaX = (currMouseState.X - previousMouseState.X)*(parent.currentScreenScale.X);
                deltaY = (currMouseState.Y - previousMouseState.Y)*(parent.currentScreenScale.Y);

                // Applies the rotation (we are only rotating on the X and Y axes)
                rot += new Vector3(deltaY, -deltaX, 0) * 15 *  dt;
            }

            // Move Faster
            if (Keyboard.GetState().IsKeyDown(Keys.LeftShift))
            {
                if (!isExhausted)
                {
                    speed = initSpeed * runSpeed;

                    //expend resource
                    secondary -= runCost;
                }
                else
                {
                    //stop running when exhausted
                    speed = initSpeed;
                }
            }
            else
            {
                //Stop running when shift is released
                speed = initSpeed;
            }


            if (this.pos.Z >= 3505)
            {
                atEnd = true;
            }

            // Move forward
            if (Keyboard.GetState().IsKeyDown(Keys.W))
            {
                pos += world.Backward * speed * dt;
            }

            // Move back
            if (Keyboard.GetState().IsKeyDown(Keys.S))
            {
                pos -= world.Backward * speed * dt;
            }

            // Move left
            if (Keyboard.GetState().IsKeyDown(Keys.A))
            {
                pos -= world.Left * speed * dt;
            }


            // Move right
            if (Keyboard.GetState().IsKeyDown(Keys.D))
            {
                pos -= world.Right * speed * dt;
            }



            // Initiate jump
            if (!jumping && Keyboard.GetState().IsKeyDown(Keys.Space))
            {
                jumping = true;
                fallSpeed = jumpSpeed*-1;
            }

            // Calculate jump
            if (jumping)
                Jump(dt);

            //regen Stamina
            if (secondary < secondaryMax)
            {
                secondary += secondaryRegenRate;
            }
            else if (secondary >= secondaryMax)
            {
                isExhausted = false;
            }

            if (secondary <= 0)
            {
                isExhausted = true;
            }


            //Update world
            world = Matrix.CreateScale(1f) *
                        Matrix.CreateRotationY(MathHelper.ToRadians(Rot.Y)) *
                        Matrix.CreateTranslation(Pos);

            head = Matrix.CreateScale(1f) *
                        Matrix.CreateRotationX(MathHelper.ToRadians(Rot.X)) *
                        Matrix.CreateRotationY(MathHelper.ToRadians(Rot.Y)) *
                        Matrix.CreateTranslation(Pos);
        }

        private void Jump(float dt)
        {
            pos.Y -= fallSpeed;
            fallSpeed += parent.gravity;

            // Once on the ground, stop jumping
            if (pos.Y <= minHeight)
            {
                pos.Y = minHeight;
                jumping = false;
            }
        }

        private void Abilities(float dt, Camera cam)
        {
            

            if (attTimer > 0)
            {
                attTimer = attTimer - dt;
            }
            else
            {
                if (currMouseState.LeftButton == ButtonState.Pressed
                    && previousMouseState.LeftButton != ButtonState.Pressed
                    && secondary >= attCost)
                {
                    switch (this.isCaster)
                    {
                        case false:
                            soundEffects[0].Play(volume: volume, pitch: 0.0f, pan: 0.0f);
                            AttackPattern.singleMel(this.Pos + head.Backward*10, head.Backward, this.projSpeed, attDmg1, this.projList);
                            break;
                        case true:
                            soundEffects[1].Play(volume: volume, pitch: 0.0f, pan: 0.0f);
                            AttackPattern.singleProj(this.Pos, head.Backward*100 + new Vector3(0,5,0),  this.projSpeed, attDmg1, this.projList);
                            break;
                    }
                    attTimer = maxAttTime;

                    //expend resource
                    secondary -= attCost;
                }
            }
            if (spec1Timer > 0)
            {
                spec1Timer = spec1Timer - dt;
            }
            else
            {
                if (currMouseState.RightButton == ButtonState.Pressed
                    && previousMouseState.RightButton != ButtonState.Pressed
                    && secondary >= spec1Cost
                    && !isExhausted)
                {
                    switch (this.role)
                    {
                        case "WARRIOR":
                            soundEffects[2].Play(volume: volume, pitch: 0.0f, pan: 0.0f);
                            AttackPattern.tripleMel(this.Pos, world.Backward, this.projSpeed, attDmg2, this.projList); ;
                            break;
                        case "ROGUE":
                            soundEffects[2].Play(volume: volume, pitch: 0.0f, pan: 0.0f);
                            AttackPattern.tripleMel(this.Pos, world.Backward, this.projSpeed, attDmg2, this.projList);
                            break;
                        case "MAGE":
                            soundEffects[1].Play(volume: volume, pitch: 0.0f, pan: 0.0f);
                            soundEffects[1].Play(volume: volume, pitch: 0.0f, pan: 0.0f);
                            AttackPattern.quinProj(this.Pos, head.Backward * 100 + new Vector3(0, 10, 0), this.projSpeed, attDmg2, this.projList);
                            break;
                        case "CLERIC":
                            soundEffects[1].Play(volume: volume, pitch: 0.0f, pan: 0.0f);
                            soundEffects[1].Play(volume: volume, pitch: 0.0f, pan: 0.0f);
                            AttackPattern.tripleProj(this.Pos, head.Backward * 1000 + new Vector3(0, 50, 0), this.projSpeed, attDmg2, this.projList);
                            break;
                    }

                    spec1Timer = maxSpec1Time;

                    //expend resource
                    secondary -= spec1Cost;
                }
            }
            if (spec2Timer > 0)
            {
                spec2Timer = spec2Timer - dt;
            }
            else
            {
                if (curKeyboardState.IsKeyDown(Keys.Q)
                    && prevKeyboardState.IsKeyUp(Keys.Q)
                    && secondary >= spec2Cost
                    && !isExhausted)
                {
                    switch (this.role)
                    {
                        case "WARRIOR":
                            if (health - 25 > 0 && secondary + 50 < secondaryMax)
                            {
                                soundEffects[5].Play(volume: volume, pitch: 0.0f, pan: 0.0f);
                                health -= 25;
                                secondary += 50;
                            }
                            else if (health - 25 > 0)
                            {
                                soundEffects[5].Play(volume: volume, pitch: 0.0f, pan: 0.0f);
                                health -= 25;
                                secondary = secondaryMax;
                            }
                            break;
                        case "ROGUE":
                            if (health - 50 > 0 && secondary + 50 < secondaryMax)
                            {
                                soundEffects[5].Play(volume: volume, pitch: 0.0f, pan: 0.0f);
                                health -= 50;
                                secondary += 100;
                            }
                            else if (health - 50 > 0)
                            {
                                soundEffects[5].Play(volume: volume, pitch: 0.0f, pan: 0.0f);
                                health -= 50;
                                secondary = secondaryMax;
                            }
                            break;
                        case "MAGE":
                            if (pos.Z < 4700 - 400)
                            {
                                soundEffects[4].Play(volume: volume, pitch: 0.0f, pan: 0.0f);
                                spec3Timer = maxSpec3Time;
                                this.pos.Z += 400;
                                secondary -= spec2Cost;
                            }
                            break;
                        case "CLERIC":
                            if (pos.Z < 4700 - 200)
                            {
                                soundEffects[4].Play(volume: volume, pitch: 0.0f, pan: 0.0f);
                                spec3Timer = maxSpec3Time;
                                this.pos.Z += 200;
                                secondary -= spec2Cost;
                            }
                            break;
                    }

                    spec2Timer = maxSpec2Time;
                }
            }
            if (spec3Timer > 0)
            {
                spec3Timer = spec3Timer - dt;
            }
            else
            {
                if (curKeyboardState.IsKeyDown(Keys.E)
                    && prevKeyboardState.IsKeyUp(Keys.E)
                    && secondary >= spec3Cost
                    && !isExhausted)
                {
                    switch (this.role)
                    {
                        case "WARRIOR":
                        case "ROGUE":
                            if (health != healthMax)
                            {
                                soundEffects[6].Play(volume: volume, pitch: 0.0f, pan: 0.0f);
                                health = healthMax;
                                secondary = 0;
                                isExhausted = true;
                            }
                            spec3Timer = maxSpec3Time;
                            break;
                        case "MAGE":
                            if (health + 25 < healthMax)
                            {
                                soundEffects[3].Play(volume: volume, pitch: 0.0f, pan: 0.0f);
                                health += 25;
                                secondary -= spec3Cost;
                            }
                            else
                            {
                                soundEffects[3].Play(volume: volume, pitch: 0.0f, pan: 0.0f);
                                health = healthMax;
                                secondary -= spec3Cost;
                            }

                            break;
                        case "CLERIC":
                            soundEffects[3].Play(volume: volume, pitch: 0.0f, pan: 0.0f);
                            if (health + 50 < healthMax)
                            {
                                health += 50;
                                secondary -= spec3Cost;
                            }
                            else
                            {
                                secondary -= spec3Cost;
                                health = healthMax;
                            }
                    break;
                    }
                   
                }
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



        /////////////////////////
        ///DEBUGGING FUNCTIONS///
        /////////////////////////
        private void DebugMode(float dt)
        {
            // Move Faster
            if (Keyboard.GetState().IsKeyDown(Keys.LeftShift))
                speed *= 3;

            //Move left, right, forward, and back
            if (Keyboard.GetState().IsKeyDown(Keys.Left) || Keyboard.GetState().IsKeyDown(Keys.A))
                this.pos += new Vector3(1, 0, 0) * speed * dt;
            if (Keyboard.GetState().IsKeyDown(Keys.Right) || Keyboard.GetState().IsKeyDown(Keys.D))
                this.pos -= new Vector3(1, 0, 0) * speed * dt;
            if (Keyboard.GetState().IsKeyDown(Keys.Up) || Keyboard.GetState().IsKeyDown(Keys.W))
                this.pos += new Vector3(0, 0, 1) * speed * dt;
            if (Keyboard.GetState().IsKeyDown(Keys.Down) || Keyboard.GetState().IsKeyDown(Keys.S))
                this.pos -= new Vector3(0, 0, 1) * speed * dt;

            // Rotate player left and right
            if (Keyboard.GetState().IsKeyDown(Keys.Q))
                this.rot += new Vector3(0, 1, 0) * speed * dt;
            if (Keyboard.GetState().IsKeyDown(Keys.E))
                this.rot -= new Vector3(0, 1, 0) * speed * dt;

            // Float Up and Down
            if (Keyboard.GetState().IsKeyDown(Keys.Space))
                this.pos += new Vector3(0, 1, 0) * speed * dt;
            if (Keyboard.GetState().IsKeyDown(Keys.LeftControl))
                this.pos -= new Vector3(0, 1, 0) * speed * dt;

            // Test the damage mechanic
            if (Keyboard.GetState().IsKeyDown(Keys.U))
                health -= 3;
            // test the secondary resource mechanic
            if (Keyboard.GetState().IsKeyDown(Keys.P))
                secondary -= 3;

            speed = initSpeed * 5f;
        }

        //update health and stamina bar
        public Rectangle resourceBarUpdate(bool isHealth, Rectangle bar, Vector2 screen, Vector2 scale)
        {
            int nWid;
            Rectangle newRect;

            //get length of bar proportional to desired bar and current screen width
            if (isHealth)
            {
                //set width of bar based on current stat
                nWid = (int)System.Math.Round(health / healthMax * (0.202f * screen.X));
                //define new rectangle for bar
                newRect = new Rectangle(
                    (int)System.Math.Round(0.099f * screen.X),
                    (int)System.Math.Round(0.044f * screen.Y), nWid, (int)System.Math.Round(bar.Height * scale.Y));
            }
            else
            {
                //set width of bar based on current stat
                nWid = (int)System.Math.Round(secondary / secondaryMax * (0.202f * screen.X));
                //define new rectangle for bar
                newRect = new Rectangle(
                    (int)System.Math.Round(0.088f * screen.X),
                    (int)System.Math.Round(0.099f * screen.Y), nWid, (int)System.Math.Round(bar.Height * scale.Y));
            }

            return newRect;
        }

        ////////////////////
        ///GETTER/SETTERS///
        ////////////////////
        public Vector3 Pos
        {
            get { return pos; }
            set { pos = value; }
        }
        public Vector3 Rot
        {
            get { return rot; }
            set { rot = value; }
        }

        public int Height
        {
            get { return height; }
            set { height = value; }
        }

        public bool IsCaster
        {
            get { return isCaster; }
            set { isCaster = value; }
        }

        public float Health
        {
            get { return health; }
            set { health = value; }
        }
    }
}
