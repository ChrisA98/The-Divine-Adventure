using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace TheDivineAdventure
{
    public class Player
    {
        ///////////////
        ///VARIABLES///
        ///////////////
        // Constant / Readonly
        // Make sure that the role, height, and width have the same index
        // (EX: WARRIOR is at index 0 of ROLES, while WARRIOR_HEIGHT is also at index 0 of HEIGHTS)
        public static readonly string[] ROLES = { "WARRIOR", "ROGUE", "MAGE", "CLERIC" };
        private const int WARRIOR_HEIGHT = 1;
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

        // Position
        private Vector3 pos;
        private Vector3 rot;
        private Vector3 lastRot;

        //Movement
        public float walkAccel, walkMax;
        public float runMod;
        public float vel;
        public Vector3 walkDir, driftDir; //direction player is walking and was walking
        public bool isWalk;     //check if player has begun walking
        public int FORWARD = 0, RIGHT = 1, BACKWARD = 2, LEFT = 3;
        public bool[] animWalkDir = new bool[4];


        // Jumping
        private float fallSpeed = 0;
        public bool jumping = false;
        public float jumpSpeed;
        private float minHeight;
        public int jumpDel;

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
        private bool isCaster;  //swaps stamina for mana when true

        //attack stats
        private float attDmg1, attDmg2;
        //attack delay
        public float attackDelay;
        private float att1CastTime, att2CastTime;
        public bool[] isAttacking;

        private bool atEnd;

        //world matrix
        public Matrix world;
        //head  matrix | controls camera vector and shoot direction
        public Matrix head;


        // Mouse
        private MouseState currMouseState;
        private MouseState previousMouseState;

        // Timer
        private float globalTimer, maxGlobal, maxAttTime, maxSpec1Time, maxSpec2Time, maxSpec3Time; //time between shots
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
            vel = 0;
            walkDir = Vector3.Zero;
            minHeight = pos.Y;
            jumpDel = 0;

            // Prepare mouse state
            previousMouseState = Mouse.GetState();

            //timer instantiation
            globalTimer = 0f;
            attTimer = 0f;
            spec1Timer = 0f;
            spec2Timer = 0f;

            attackDelay = 0;            //delay for attack to sync with animation
            isAttacking = new bool[2];  //flags for attack animations

            isExhausted = false;

            world = Matrix.CreateScale(1f) *
                        Matrix.CreateRotationX(MathHelper.ToRadians(90)) *
                        Matrix.CreateRotationY(MathHelper.ToRadians(Rot.Y)) *
                        Matrix.CreateRotationX(MathHelper.ToRadians(0)) *
                        Matrix.CreateTranslation(Pos);
            head = Matrix.CreateScale(1f) *
                        Matrix.CreateRotationX(MathHelper.ToRadians(Rot.X)) *
                        Matrix.CreateRotationY(MathHelper.ToRadians(Rot.Y)) *
                        Matrix.CreateTranslation(Pos);

            #region --Character Stats --
            // Set player stats
            switch (this.role)
            {
                case "WARRIOR":
                    isCaster = false;
                    walkAccel = 0.15f;
                    walkMax = 1f;
                    runMod = 2.5f;
                    jumpSpeed = 1.8f;
                    healthMax = 230;
                    health = healthMax;
                    secondaryMax = 200;
                    secondary = secondaryMax;
                    secondaryRegenRate = 0.18f;
                    projSpeed = 10f;
                    maxGlobal = 0.25f;
                    maxAttTime = 0.75f;
                    maxSpec1Time = 0.5f;
                    maxSpec2Time = 0.5f;
                    maxSpec3Time = 0.5f;
                    attCost = 10;
                    spec1Cost = 20;
                    spec2Cost = 30;
                    spec3Cost = 50;
                    runCost = 0.2f;
                    attDmg1 = 400f;
                    attDmg2 = 50f;
                    att1CastTime = 30f;
                    att2CastTime = 20f;
                    break;
                case "ROGUE":
                    isCaster = false;
                    walkAccel = 0.2f;
                    walkMax = 1.5f;
                    runMod = 2f;
                    jumpSpeed = 2.5f;
                    healthMax = 300;
                    health = healthMax;
                    secondaryMax = 100;
                    secondary = secondaryMax;
                    secondaryRegenRate = 0.25f;
                    projSpeed = 15f;
                    maxGlobal = 0.25f;
                    maxAttTime = 0.5f;
                    maxSpec1Time = 0.25f;
                    maxSpec2Time = 0.5f;
                    maxSpec3Time = 0.5f;
                    attCost = 10;
                    spec1Cost = 20;
                    spec2Cost = 30;
                    spec3Cost = 50;
                    runCost = 0f;
                    attDmg1 = 50f;
                    attDmg2 = 25f;
                    att1CastTime = 23f;
                    att2CastTime = 4f;
                    break;
                case "MAGE":
                    isCaster = true;
                    walkAccel = 0.13f;
                    walkMax = 0.8f;
                    runMod = 2.5f;
                    jumpSpeed = 2f;
                    healthMax = 230;
                    health = healthMax;
                    secondaryMax = 200;
                    secondary = secondaryMax;
                    secondaryRegenRate = 0.18f;
                    projSpeed = 10f;
                    maxGlobal = 0.25f;
                    maxAttTime = 0.75f;
                    maxSpec1Time = 0.5f;
                    maxSpec2Time = 0.5f;
                    maxSpec3Time = 0.5f;
                    attCost = 10;
                    spec1Cost = 20;
                    spec2Cost = 30;
                    spec3Cost = 50;
                    runCost = 0.2f;
                    attDmg1 = 100f;
                    attDmg2 = 50f;
                    att1CastTime = 23f;
                    att2CastTime = 4f;
                    break;
                case "CLERIC":
                    isCaster = true;
                    walkAccel = 0.2f;
                    walkMax = 1f;
                    runMod = 2f;
                    jumpSpeed = 2f;
                    healthMax = 300;
                    health = healthMax;
                    secondaryMax = 100;
                    secondary = secondaryMax;
                    secondaryRegenRate = 0.25f;
                    projSpeed = 15f;
                    maxGlobal = 0.25f;
                    maxAttTime = 0.5f;
                    maxSpec1Time = 0.25f;
                    maxSpec2Time = 0.5f;
                    maxSpec3Time = 0.5f;
                    attCost = 10;
                    spec1Cost = 20;
                    spec2Cost = 30;
                    spec3Cost = 50;
                    runCost = 0f;
                    attDmg1 = 50f;
                    attDmg2 = 25f;
                    att1CastTime = 23f;
                    att2CastTime = 4f;
                    break;
            }
            #endregion -- Characer Stats --
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
            //DebugMode(dt);

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
                rot += new Vector3(deltaY, -deltaX, 0) * 25 *  dt;

                //Clamp roatations
                if (rot.X > 360) rot.X -= 360;
                if (rot.X < -360) rot.X += 360;
                if (rot.Y > 360) rot.Y -= 360;
                if (rot.Y < -360) rot.Y += 360;
            }


            #region -- Player Walking --

            //uses specific keybinds (perhaps change to allow player mapping of contrrols in the future)

            isWalk = false;
            for (int i=0; i<4;i++) animWalkDir[i] = false;
            foreach (Keys k in Keyboard.GetState().GetPressedKeys())    //get player movement vector
            {
                if (k == Keys.LeftShift) continue;      //don't read srpint button here
                if (k == Keys.W)
                {
                    walkDir += world.Backward;
                    isWalk = true;
                    animWalkDir[FORWARD] = true;
                    continue;
                }
                if (k == Keys.D)
                {
                    walkDir += world.Left;
                    isWalk = true;
                    animWalkDir[RIGHT] = true;
                    continue; 
                }
                if (k == Keys.S) 
                {
                    walkDir += world.Forward;
                    isWalk = true;
                    animWalkDir[BACKWARD] = true;
                    continue;
                }
                if (k == Keys.A) 
                {
                    walkDir += world.Right;
                    isWalk = true;
                    animWalkDir[LEFT] = true;
                    continue;
                }
            }
            if (walkDir == Vector3.Zero) isWalk = false;    //not walkiing i fplayer cancels out movement

            if (Keyboard.GetState().IsKeyDown(Keys.A) && Keyboard.GetState().IsKeyDown(Keys.D))
            {   //don't add side walk anims if mixing directions
                animWalkDir[LEFT] = false;
                animWalkDir[RIGHT] = false;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.W) && Keyboard.GetState().IsKeyDown(Keys.S))
            {   //don't add side walk anims if mixing directions
                animWalkDir[FORWARD] = false;
                animWalkDir[BACKWARD] = false;
            }

            walkDir.Y = 0; //clear y dir since player can't walk in y axis

            if (isWalk && Keyboard.GetState().IsKeyDown(Keys.LeftShift)
                && !animWalkDir[BACKWARD]
                && !animWalkDir[LEFT]
                && !animWalkDir[RIGHT])    //update while running
            {
                if (vel <= walkMax * runMod && !isExhausted) vel += walkAccel;   //update velocity
                pos += walkDir * vel;   //move player
                driftDir = walkDir; //catch current walk direction for slow down drift
                walkDir = Vector3.Zero; //reset player move vector
            }
            else if (isWalk)   //update while not running
            {
                if (vel < walkMax) vel += walkAccel;    //update velocity
                if (vel > walkMax) vel -= walkAccel;    //slow down if stopping sprinting
                if (isCaster)
                {   //slow casters down while casting
                    if (isAttacking[0]) vel = walkMax * 0.4f;
                    if (isAttacking[1]) vel = walkMax * 0.01f;
                }
                pos += (walkDir) * vel;  //move player 
                driftDir = walkDir;                     //catch current walk direction for slow down drift
                walkDir = Vector3.Zero;                 //reset player move vector
            }
            else
            {
                if (vel > 0.08)
                {
                    vel *= 0.95f;  //deccelerate slowly
                    pos += driftDir * vel;
                } 
                else vel = 0;               //stop
            }

            #endregion


            if (this.pos.Z >= 3505)
            {
                atEnd = true;
            }


            // Initiate jump
            if (!jumping && Keyboard.GetState().IsKeyDown(Keys.Space))
            {
                jumping = true;
                fallSpeed = jumpSpeed*-1;
            }

            // Calculate jump
            if (jumping)
            {
                jumpDel++;
                if(jumpDel > 14) Jump(dt);
            }

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
            // only rotate model when walking
            if (isWalk)
            {
                world = Matrix.CreateScale(1f) *
                           Matrix.CreateRotationY(MathHelper.ToRadians(Rot.Y)) *
                           Matrix.CreateTranslation(Pos);
                lastRot = Rot;
            }
            else
            {
                world = Matrix.CreateScale(1f) *
                           Matrix.CreateRotationY(MathHelper.ToRadians(lastRot.Y)) *
                           Matrix.CreateTranslation(Pos);
            }

            //clamp head rot (MAY NEED REDONE WITH LEVEL REVAMP)
            if (Rot.X > 73)
                rot.X = 73;
            if (Rot.X < -33)
                rot.X = -33;

            //control facing
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
                jumpDel = 0;
                jumping = false;
            }
        }

        private void Abilities(float dt, Camera cam)
        {
            if (globalTimer > 0)
            {
                globalTimer -= dt;       //check main attack timer
            }
            if (attTimer > 0)
            {
                attTimer = attTimer - dt;       //check main attack timer
            }
            else
            {
                if (currMouseState.LeftButton == ButtonState.Pressed
                    && previousMouseState.LeftButton != ButtonState.Pressed
                    && secondary >= attCost
                    && globalTimer <= 0)
                {//Begin attack animation when main attack is avaialable
                    isAttacking[0] = true;  //set main attack to true
                    attackDelay += 1;       //start attack
                    secondary -= attCost;   //expend resource
                }
                if (isAttacking[0])
                {
                    if (attackDelay < att1CastTime) attackDelay += 1;   //Advance attack timer
                    else
                    {
                        attackDelay = 0;
                        attTimer = maxAttTime;  //Reset main attack timer
                        globalTimer = maxGlobal;    //Reset Global Timer
                        switch (role)
                        {
                            case "WARRIOR":
                                soundEffects[0].Play(volume: volume, pitch: 0.0f, pan: 0.0f);
                                AttackPattern.singleMel(this.Pos + head.Backward * 10, head.Backward, attDmg1, this.projList);
                                break;
                            case "ROGUE":
                                soundEffects[0].Play(volume: volume, pitch: 0.0f, pan: 0.0f);
                                AttackPattern.singleMel(this.Pos + head.Backward * 10, head.Backward, attDmg1, this.projList);
                                break;
                            case "MAGE":
                                soundEffects[1].Play(volume: volume, pitch: 0.0f, pan: 0.0f);
                                AttackPattern.singleProj(this.Pos + world.Up * 10 + world.Left * 8, head.Backward * 100 + new Vector3(0, -15, 0), this.projSpeed, attDmg1, this.projList, cam);
                                break;
                            case "CLERIC":
                                soundEffects[1].Play(volume: volume, pitch: 0.0f, pan: 0.0f);
                                AttackPattern.singleProj(Pos + (world.Up * 4) + (world.Left * 6), (head.Backward * 100), projSpeed, attDmg1, projList, cam);
                                break;
                        }
                        isAttacking[0] = false;  //set main attack to false
                    }
                }
                

                    
            }//end main attack

            //secondary attack//
            if (spec1Timer > 0)
            {
                spec1Timer = spec1Timer - dt;
            }
            else
            {
                if (currMouseState.RightButton == ButtonState.Pressed
                    && previousMouseState.RightButton != ButtonState.Pressed
                    && secondary >= spec1Cost
                    && !isExhausted
                    && globalTimer <= 0)
                {//Begin attack animation when secondary attack is avaialable
                    isAttacking[1] = true;  //set main attack to true
                    attackDelay += 1;       //start attack
                    secondary -= spec1Cost;   //expend resource
                }
                if (isAttacking[1])
                {
                    if (attackDelay < att2CastTime) attackDelay += 1;   //Advance attack timer
                    else
                    {
                        attackDelay = 0;
                        spec1Timer = maxSpec1Time;  //Reset secondary attack timer
                        globalTimer = maxGlobal;    //Reset Global Action Timer
                        switch (this.role)
                        {
                            case "WARRIOR":
                                soundEffects[2].Play(volume: volume, pitch: 0.0f, pan: 0.0f);
                                AttackPattern.tripleMel(Pos, world.Backward, attDmg2, this.projList); ;
                                break;
                            case "ROGUE":
                                soundEffects[2].Play(volume: volume, pitch: 0.0f, pan: 0.0f);
                                AttackPattern.tripleMel(Pos, world.Backward, attDmg2, this.projList);
                                break;
                            case "MAGE":
                                soundEffects[1].Play(volume: volume, pitch: 0.0f, pan: 0.0f);
                                soundEffects[1].Play(volume: volume, pitch: 0.0f, pan: 0.0f);
                                AttackPattern.quinProj(world, head.Backward * 100, this.projSpeed, attDmg2, this.projList, cam);
                                break;
                            case "CLERIC":
                                soundEffects[1].Play(volume: volume, pitch: 0.0f, pan: 0.0f);
                                soundEffects[1].Play(volume: volume, pitch: 0.0f, pan: 0.0f);
                                AttackPattern.tripleProj(world, head.Backward * 100, this.projSpeed, attDmg2, this.projList, cam);
                                break;
                        }
                        isAttacking[1] = false;  //set main attack to false
                    }
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
            isCaster = false;
            walkAccel = 0.15f;
            walkMax = 1f;
            runMod = 2.5f;
            jumpSpeed = 1.8f;
            healthMax = 230;
            health = healthMax;
            secondaryMax = 200;
            secondary = secondaryMax;
            secondaryRegenRate = 0.18f;
            projSpeed = 10f;
            maxGlobal = 0.25f;
            maxAttTime = 0.75f;
            maxSpec1Time = 0.5f;
            maxSpec2Time = 0.5f;
            maxSpec3Time = 0.5f;
            attCost = 10;
            spec1Cost = 20;
            spec2Cost = 30;
            spec3Cost = 50;
            runCost = 0.2f;
            attDmg1 = 400f;
            attDmg2 = 50f;
            att1CastTime = 30f;
            att2CastTime = 24f;
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

        //Check movement status
        public bool isWalking()
        {
            if (vel > 0 && vel <= walkMax) return true;
            if (vel > walkMax) return false;
            return false;
        }
        public bool isRunning()
        {
            if (vel > walkMax) return true;
            return false;
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
