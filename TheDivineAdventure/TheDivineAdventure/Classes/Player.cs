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
        private const int WARRIOR_WIDTH = 25;
        private const int ROGUE_WIDTH = 20;
        private const int MAGE_WIDTH = 20;
        private const int CLERIC_WIDTH = 20;
        private static readonly int[] WIDTHS = { WARRIOR_WIDTH, ROGUE_WIDTH, MAGE_WIDTH, CLERIC_WIDTH };

        // Info
        public string role;
        public int height, width;
        public List<Attack> projList = new List<Attack>();
        private Game1 parent;
        GraphicsDevice gpu;

        // Position
        private Vector3 pos;
        private Vector3 rot;
        private Vector3 lastRot;

        //Movement
        public int sensitivity; //player control sensitivity
        public float walkAccel, walkMax;
        public float runMod;
        public float vel;
        public Vector3 walkDir, driftDir; //direction player is walking and was walking
        public bool isWalk;     //check if player has begun walking
        public bool isDrifting; //check if player is slowing down from walking
        public int FORWARD = 0, RIGHT = 1, BACKWARD = 2, LEFT = 3;
        public bool[] animWalkDir = new bool[4];
        public float outsideVelocity;   //velocity imparted by outside sources
        public Vector3 outsideForceDir; //directional forces from outside player
        private List<Vector3> cachedPosition;
        private int timeColiding;


        // Jumping
        private float fallSpeed = 0;
        public bool jumping = false;
        public float jumpSpeed;
        private float minHeight;
        public int jumpDel;
        public bool onGround;

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

        //Collision Info
        public CapsuleCollider boundingCollider;
        public bool nearInteractable;

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
            sensitivity = int.Parse(parent.settings[7, 1]);

            // Set player position
            height = HEIGHTS[Array.IndexOf(ROLES, role)];
            width = WIDTHS[Array.IndexOf(ROLES, role)];
            pos = new Vector3(0, 40, 0);
            cachedPosition = new List<Vector3>();
            cachedPosition.Add(pos);
            cachedPosition.Add(pos);
            cachedPosition.Add(pos);
            cachedPosition.Add(pos);
            cachedPosition.Add(pos);
            cachedPosition.Add(pos);
            cachedPosition.Add(pos);
            timeColiding = 0;
            rot = new Vector3(0, -5, 0);
            vel = 0;
            walkDir = Vector3.Zero;
            minHeight = pos.Y;
            jumpDel = 0;
            onGround = true;

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

            //world info
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
            switch (role)
            {
                case "WARRIOR":
                    //Capsule Collider
                    boundingCollider = new CapsuleCollider(26, 8, pos, rot, gpu, Color.Blue);
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
                    projSpeed = 0f;
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
                    att1CastTime = 30f;
                    att2CastTime = 20f;
                    break;
                case "ROGUE":
                    //Capsule Collider
                    boundingCollider = new CapsuleCollider(24, 8, pos, rot, gpu, Color.Blue, 13);
                    isCaster = false;
                    walkAccel = 0.35f;
                    walkMax = 1.5f;
                    runMod = 2.0f;
                    jumpSpeed = 2f;
                    healthMax = 230;
                    health = healthMax;
                    secondaryMax = 200;
                    secondary = secondaryMax;
                    secondaryRegenRate = 0.18f;
                    projSpeed = 0f;
                    maxGlobal = 0.13f;
                    maxAttTime = 0.13f;
                    maxSpec1Time = 0.3f;
                    maxSpec2Time = 0.5f;
                    maxSpec3Time = 0.5f;
                    attCost = 10;
                    spec1Cost = 20;
                    spec2Cost = 30;
                    spec3Cost = 50;
                    runCost = 0.2f;
                    attDmg1 = 70f;
                    attDmg2 = 40f;
                    att1CastTime = 15f;
                    att2CastTime = 20f;
                    break;
                case "MAGE":
                    //Capsule Collider
                    boundingCollider = new CapsuleCollider(28, 8, pos, rot, gpu, Color.Blue, 16);
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
                    projSpeed = 15f;
                    maxGlobal = 0.25f;
                    maxAttTime = 0.75f;
                    maxSpec1Time = 0.5f;
                    maxSpec2Time = 0.5f;
                    maxSpec3Time = 0.5f;
                    attCost = 10;
                    spec1Cost = 100;
                    spec2Cost = 30;
                    spec3Cost = 50;
                    runCost = 0.2f;
                    attDmg1 = 50f;
                    attDmg2 = 25f;
                    att1CastTime = 23f;
                    att2CastTime = 4f;
                    break;
                case "CLERIC":
                    //Capsule Collider
                    boundingCollider = new CapsuleCollider(26, 8, pos, rot, gpu, Color.Blue, 13);
                    isCaster = true;
                    walkAccel = 0.2f;
                    walkMax = 1.4f;
                    runMod = 2f;
                    jumpSpeed = 2f;
                    healthMax = 300;
                    health = healthMax;
                    secondaryMax = 100;
                    secondary = secondaryMax;
                    secondaryRegenRate = 0.25f;
                    projSpeed = 20f;
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
        public void Update(float dt, Camera cam, GameTime gameTime)
        {
            // Variables
            currMouseState = Mouse.GetState();
            curKeyboardState = Keyboard.GetState();

            // Regular Gameplay
            Move(dt);
            Abilities(dt, cam, gameTime);

            #region Interact

            nearInteractable = false;
            //check doors
            foreach (Level.InteractiveDoor door in parent.playScene.thisLevel.Doors)
            {
                if (Vector3.Distance(door.Position, pos) < 400)
                {
                    if (door.isLocked || !door.active) continue;
                    if (boundingCollider.Intersects(door.interactBox))
                    {
                        nearInteractable = true;
                        if (Keyboard.GetState().IsKeyDown(Keys.F) && parent.lastKeyboard.IsKeyUp(Keys.F))
                        {
                            door.Open(gameTime);
                            break;
                        }
                    }
                }
            }

            //check activators
            foreach (Level.Activator activator in parent.playScene.thisLevel.Activators)
            {
                if (Vector3.Distance(activator.Position, pos) < 30)
                {
                    if (!activator.active) continue;
                    if (boundingCollider.Intersects(activator.interactiveBox))
                    {
                        nearInteractable = true;
                        if (Keyboard.GetState().IsKeyDown(Keys.F) && parent.lastKeyboard.IsKeyUp(Keys.F))
                        {
                            activator.Activate(parent.playScene.thisLevel, gameTime);
                            break;
                        }
                    }
                }
            }
            //check triggers
            foreach (Level.SpawnerTriggerBox trigger in parent.playScene.thisLevel.Triggers)
            {
                if (Vector3.Distance(trigger.Position, pos) < 150)
                {
                    if (boundingCollider.Intersects(trigger.interactiveBox))
                    {
                        trigger.Activate(gameTime);
                    }
                }
            }
            //check triggers
            foreach (Level.MissionTriggerBox trigger in parent.playScene.thisLevel.MissionTriggers)
            {
                if (Vector3.Distance(trigger.Position, pos) < 150)
                {
                    if (boundingCollider.Intersects(trigger.interactiveBox))
                    {
                        trigger.Activate(parent.playScene.thisLevel, gameTime);
                        break;
                    }
                }
            }
            #endregion


            //Debugging
            //DebugMode(dt);


            Mouse.SetPosition(gpu.Viewport.Width / 2, gpu.Viewport.Height / 2);

            previousMouseState = Mouse.GetState(); ;
            prevKeyboardState = curKeyboardState;

            boundingCollider.Update(pos);
        }


        public void Damage(float amt, float force, Vector3 Direction)
        {
            health -= amt;
            outsideVelocity += force;
            outsideForceDir -= Direction;
            outsideForceDir.Normalize();
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
                rot += new Vector3(deltaY, -deltaX, 0) * sensitivity *  dt;

                //Clamp roatations
                if (rot.X > 360) rot.X -= 360;
                if (rot.X < -360) rot.X += 360;
                if (rot.Y > 360) rot.Y -= 360;
                if (rot.Y < -360) rot.Y += 360;
            }


            #region -- Player Walking --

            //uses specific keybinds (perhaps change to allow player mapping of controls in the future)

            //resets walk if move buttons aren't pressed
            if(Keyboard.GetState().IsKeyUp(Keys.W) &&
                Keyboard.GetState().IsKeyUp(Keys.A) &&
                Keyboard.GetState().IsKeyUp(Keys.S) &&
                Keyboard.GetState().IsKeyUp(Keys.D))
            {
                isWalk = false;
            }


            if (isWalk == true && isDrifting == false)
            {
                for (int i = 0; i < 4; i++) animWalkDir[i] = false;
            }

            if (vel > 0.00f) isDrifting = true;
            else isDrifting = false;
            
            foreach (Keys k in Keyboard.GetState().GetPressedKeys())    //get player movement vector
            {
                if (k == Keys.LeftShift) continue;      //don't read srpint button here
                if (k == Keys.W)
                {
                    walkDir += world.Backward;
                    isWalk = true;
                    animWalkDir[FORWARD] = true;
                    isDrifting = false;
                    continue;
                }
                if (k == Keys.D)
                {
                    walkDir += world.Left;
                    isWalk = true;
                    animWalkDir[RIGHT] = true;
                    isDrifting = false;
                    continue; 
                }
                if (k == Keys.S) 
                {
                    walkDir += world.Forward;
                    isWalk = true;
                    animWalkDir[BACKWARD] = true;
                    isDrifting = false;
                    continue;
                }
                if (k == Keys.A) 
                {
                    walkDir += world.Right;
                    isWalk = true;
                    animWalkDir[LEFT] = true;
                    isDrifting = false;
                    continue;
                }
            }
            if (walkDir == Vector3.Zero) isWalk = false;    //not walking if player cancels out movement

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



            #region <<COLLISION CHECKING>>
            bool FloorStuck = false;
            bool staticCollision = false;
            //floor collision
            foreach (Level.FloorTile floor in parent.playScene.thisLevel.FloorTiles)
            {
                if (Vector3.Distance(floor.Position, pos) < 800)
                {
                    if (boundingCollider.Intersects(floor.collider))
                    {
                        if (fallSpeed == 0)
                        {
                            FloorStuck = true;
                            Pos -= 2f * world.Down;
                        }
                        pos -= fallSpeed * world.Down;
                        fallSpeed = 0;
                        if (jumpDel > 18)
                        {
                            jumpDel = 0;
                            jumping = false;
                        }
                        onGround = true;
                        break;
                    }
                }
                onGround = false;
            }
            //Enemy collision
            foreach (PlayScene.EnemySpawner spawner in parent.playScene.levelSpawnerList)
            {
                if (spawner.isActive == false) continue;
                foreach (Enemy entity in spawner.enemyList)
                {
                    if (boundingCollider.Intersects(entity.boundingCollider))
                    {
                        if (Math.Abs(entity.boundingCollider.Position.Y - boundingCollider.Position.Y) > 3) continue;
                        if (isDrifting) driftDir =  Vector3.Zero;
                        else walkDir = ManageCollision(entity.world, walkDir);
                    }
                }
            }
            //static mesh collision
            foreach (Level.StaticCollisionMesh mesh in parent.playScene.thisLevel.StaticCollisionMeshes)
            {
                if (Vector3.Distance(mesh.Position, pos) < 600)
                {
                    if (StaticMeshCollsion(mesh.collider)) {staticCollision = true;  continue;}
                }
            }
            //door collision
            foreach (Level.InteractiveDoor mesh in parent.playScene.thisLevel.Doors)
            {
                if (Vector3.Distance(mesh.Position, pos) < 400)
                {
                    if (StaticMeshCollsion(mesh.leftCollider)) { staticCollision = true; continue; }
                    if (StaticMeshCollsion(mesh.rightCollider)) { staticCollision = true; continue; }
                }
            }
            //death box collision
            foreach (Level.DeathBox mesh in parent.playScene.thisLevel.DeathBoxes)
            {
                if (Vector3.Distance(mesh.Position, pos) < 400)
                {
                    if (boundingCollider.Intersects(mesh.collider)) health = 0; //kills in death floor
                }
            }
            #endregion

            if (isWalk)    //update while running
            {
                if (!isExhausted && Keyboard.GetState().IsKeyDown(Keys.LeftShift)
                && !animWalkDir[BACKWARD]
                && !animWalkDir[LEFT]
                && !animWalkDir[RIGHT]
                && !staticCollision)
                {
                    if (vel <= walkMax * runMod) vel += walkAccel;   //update velocity
                    secondary -= runMod * 0.2f;    //drain stamina

                    pos += walkDir * vel;   //move player
                    driftDir = walkDir;     //catch current walk direction for slow down drift
                    walkDir = Vector3.Zero; //reset player move vector
                }
                else
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
            }
            else
            {
                if (vel > 0.08)
                {
                    vel *= 0.9f;  //deccelerate slowly
                    pos += driftDir * vel;
                } 
                else vel = 0;               //stop
            }

            //Handle outside forces movement
            if (outsideVelocity >= 0)
            {
                pos -= outsideForceDir * outsideVelocity;
                world = Matrix.CreateScale(1f) *
                           Matrix.CreateRotationY(MathHelper.ToRadians(Rot.Y)) *
                           Matrix.CreateTranslation(Pos);
                outsideVelocity -= walkAccel;
                if (outsideVelocity < 0) outsideVelocity = 0;
            }
            if (outsideVelocity <= 0) outsideForceDir = Vector3.Zero;


            #endregion

            Fall();

            // Initiate jump
            if (Keyboard.GetState().IsKeyDown(Keys.Space) && onGround)
            {
                jumping = true;
            }

            // Calculate jump
            if (jumping)
            {
                jumpDel++;
                if (jumpDel > 18)
                {
                    pos += world.Up*5;
                    fallSpeed = jumpSpeed * -1;
                    jumpDel = 0;
                    jumping = false;
                }
            }

            //regen Stamina
            if (secondary < secondaryMax)
            {
                secondary += secondaryRegenRate;
            }
            else if (secondary >= secondaryMax/4)
            {
                isExhausted = false;
            }

            if (secondary <= 0)
            {
                isExhausted = true;
                secondary = 0;
            }

            //reset pos if falling of world
            if (pos.Y <= -500) pos = new Vector3(0, 70, 0);

            //Cache position
            if (Vector3.Distance(pos, cachedPosition[6]) > 1 && !staticCollision)
            {
                cachedPosition.Add(pos); //set last position
                cachedPosition.RemoveAt(0);
            }

            //Update world
            if (isWalk)
            {
                // only rotate model when walking
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
            if (Rot.X > 80)
                rot.X = 80;
            if (Rot.X < -70)
                rot.X = -70;

            //control facing
            head = Matrix.CreateScale(1f) *
                        Matrix.CreateRotationX(MathHelper.ToRadians(Rot.X)) *
                        Matrix.CreateRotationY(MathHelper.ToRadians(Rot.Y)) *
                        Matrix.CreateTranslation(Pos);
        }

        //process gravity
        private void Fall()
        {
            if (!onGround)
            {
                fallSpeed += parent.gravity;
                pos.Y -= fallSpeed;
            }

        }

        private void Abilities(float dt, Camera cam, GameTime gt)
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

                        //rotate player to attack where looking
                        world = Matrix.CreateScale(1f) *
                           Matrix.CreateRotationY(MathHelper.ToRadians(Rot.Y)) *
                           Matrix.CreateTranslation(Pos);
                        lastRot = Rot;

                        switch (role)
                        {
                            case "WARRIOR":
                                soundEffects[0].Play(volume: volume, pitch: 0.0f, pan: 0.0f);
                                AttackPattern.singleMel(Pos + head.Backward * 15, head.Backward, rot, attDmg1, projList, cam);
                                break;
                            case "ROGUE":
                                soundEffects[0].Play(volume: volume, pitch: 0.0f, pan: 0.0f);
                                AttackPattern.RogueMain(Pos + head.Backward * 10, head.Backward, rot, attDmg1, projList, cam);
                                break;
                            case "MAGE":
                                soundEffects[1].Play(volume: volume, pitch: 0.0f, pan: 0.0f);
                                AttackPattern.MageMain(world, pos + ((head.Backward + world.Left * 0.05f) * 100), projSpeed, attDmg1, projList, cam);
                                break;
                            case "CLERIC":
                                soundEffects[1].Play(volume: volume, pitch: 0.0f, pan: 0.0f);
                                AttackPattern.singleProj(Pos + (world.Up * 4) + (world.Left * 6), pos+((head.Backward+world.Left*0.05f) * 100), projSpeed, attDmg1, projList, cam);
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

                        //rotate player to attack where looking
                        world = Matrix.CreateScale(1f) *
                           Matrix.CreateRotationY(MathHelper.ToRadians(Rot.Y)) *
                           Matrix.CreateTranslation(Pos);
                        lastRot = Rot;

                        switch (this.role)
                        {
                            case "WARRIOR":
                                soundEffects[2].Play(volume: volume, pitch: 0.0f, pan: 0.0f);
                                AttackPattern.WarriorHeavy(Pos, world.Backward, rot, attDmg2, projList, cam); ;
                                break;
                            case "ROGUE":
                                soundEffects[2].Play(volume: volume, pitch: 0.0f, pan: 0.0f);
                                AttackPattern.RogueHeavy(Pos, world.Backward, rot, attDmg2, projList, cam);
                                break;
                            case "MAGE":
                                soundEffects[1].Play(volume: volume, pitch: 0.0f, pan: 0.0f);
                                soundEffects[1].Play(volume: volume, pitch: 0.0f, pan: 0.0f);
                                AttackPattern.MageAlt(world, projSpeed/4, attDmg2, projList, cam);
                                break;
                            case "CLERIC":
                                soundEffects[1].Play(volume: volume, pitch: 0.0f, pan: 0.0f);
                                soundEffects[1].Play(volume: volume, pitch: 0.0f, pan: 0.0f);
                                AttackPattern.tripleProj(world, pos + (head.Backward * 100), projSpeed, attDmg2, projList, cam);
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
                    switch (role)
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
                        case "CLERIC":
                            spec3Timer = maxSpec3Time;
                            Ray testForward = new Ray(pos, head.Backward);
                            bool collision = false;
                            foreach (Level.StaticCollisionMesh mesh in parent.playScene.thisLevel.StaticCollisionMeshes)
                            {
                                if(Vector3.Distance(pos,mesh.Position) <= 200)
                                if (mesh.collider.Intersects(testForward) != null)
                                    collision = true;
                            }
                            foreach (Level.InteractiveDoor mesh in parent.playScene.thisLevel.Doors)
                            {
                                if (Vector3.Distance(pos, mesh.Position) <= 230)
                                {
                                    if (mesh.leftCollider.Intersects(testForward) != null)
                                        collision = true;
                                    if (mesh.rightCollider.Intersects(testForward) != null)
                                        collision = true;
                                }
                            }
                            foreach (Level.SpawnerTriggerBox mesh in parent.playScene.thisLevel.Triggers)
                            {
                                if (Vector3.Distance(pos, mesh.Position) <= 230)
                                {
                                    if (mesh.interactiveBox.Intersects(testForward) != null)
                                        mesh.Activate(gt);
                                }
                            }
                            foreach (Level.MissionTriggerBox mesh in parent.playScene.thisLevel.MissionTriggers)
                            {
                                if (Vector3.Distance(pos, mesh.Position) <= 230)
                                {
                                    if (mesh.collider.Intersects(testForward) != null)
                                        mesh.Activate(parent.playScene.thisLevel,gt);
                                }
                            }
                            if (!collision)
                            {
                                soundEffects[4].Play(volume: volume, pitch: 0.0f, pan: 0.0f);
                                pos += head.Backward * 200;
                                secondary -= spec2Cost;
                            }
                            break;
                    }
                    spec2Timer = maxSpec2Time;
                }
            }
            if (spec3Timer > 0)
            {
                spec3Timer -= dt;
            }
            else
            {
                if (curKeyboardState.IsKeyDown(Keys.E)
                    && prevKeyboardState.IsKeyUp(Keys.E)
                    && secondary >= spec3Cost
                    && !isExhausted)
                {
                    switch (role)
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
            walkAccel = 0.35f;
            walkMax = 1.5f;
            runMod = 2.0f;
            jumpSpeed = 2f;
            healthMax = 230;
            health = healthMax;
            secondaryMax = 200;
            secondary = secondaryMax;
            secondaryRegenRate = 0.18f;
            projSpeed = 10f;
            maxGlobal = 0.13f;
            maxAttTime = 0.13f;
            maxSpec1Time = 0.3f;
            maxSpec2Time = 0.5f;
            maxSpec3Time = 0.5f;
            attCost = 10;
            spec1Cost = 20;
            spec2Cost = 30;
            spec3Cost = 50;
            runCost = 0.2f;
            attDmg1 = 70f;
            attDmg2 = 40f;
            att1CastTime = 15f;
            att2CastTime = 20f;
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

        //manage collision with other entities
        public Vector3 ManageCollision(Matrix targetPos, Vector3 currentDir)
        {
            Vector3 tarDir = targetPos.Translation - pos;
            if (tarDir == currentDir) return Vector3.Zero;
            currentDir -= tarDir;
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
                    pos = cachedPosition[5];
                    timeColiding = 0;
                    return true;
                }
                Vector3 newDir = boundingCollider.Collisiondirection(collisionbox);
                if (newDir.X+newDir.Y < 0.4) { pos = cachedPosition[6]; return true; }
                if (float.IsNaN(newDir.X)) { Debug.WriteLine("broken"); pos = cachedPosition[3]; return true; }
                pos += newDir* vel * outsideForceDir;
                if (isDrifting) driftDir = Vector3.Zero;
                if (Vector3.Dot(newDir, walkDir) > .3f)
                {
                    walkDir = Vector3.Zero;
                    vel *= 0.5f;
                }
                outsideForceDir = Vector3.Zero;
                outsideVelocity = 0;
            }
            return false;
        }

        ////////////////////
        ///GETTER/SETTERS///
        ////////////////////
        #region  Getters / Setters
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
        #endregion
    }
}
