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
    class Minotaur : Enemy
    {
        //Charge Info
        private float chargeDistance;
        private float chargeTimer, chargeDelay;
        private bool isCharging;

        //Constructor ----------
        public Minotaur(List<SoundEffect> s, string role_, Vector3 spawnLoc, PlayScene parent, SkinModel model, ContentManager content) : base(s, role_, spawnLoc, parent, model, content)
        {
            //Capsule Collider
            boundingCollider = new CapsuleCollider(37, 18, pos, rot, parent.gpu, Color.White);
            animWeights = new float[100];

            health = 200;

            vel = 0;
            speedMax = 0.9f;
            runMod = 2.8f;
            accel = .01f;
            chargeDistance = 175;
            prefDistance = 20;

            attackRange = 30;
            attackSpeed = 150;
            attackDamage = 25f;
            attackLength = 40f;
            maxAttackRange = 250f;
            chargeDelay = 0;

            animations[IDLE] = loader.Load("MOD_Minotaur/ANIM_Minotaur_Idle.fbx", "MOD_Minotaur", true, 4, skinFx, rescale: 3.2f);
            animations[WALK] = loader.Load("MOD_Minotaur/ANIM_Minotaur_Walk.fbx", "MOD_Minotaur", true, 4, skinFx, rescale: 2.2f);
            animations[RUN] = loader.Load("MOD_Minotaur/ANIM_Minotaur_Run.fbx", "MOD_Minotaur", true, 4, skinFx, rescale: 2.2f);
            animations[RUN].playSpeed = 2;
            animations[ATTACKM] = loader.Load("MOD_Minotaur/ANIM_Minotaur_Attack.fbx", "MOD_Minotaur", true, 4, skinFx, rescale: 2.2f);
            animations[ATTACKM].loopAnimation = false;
        }

        //Update Function
        public override void Update(float dt, GameTime gametime, Camera cam)
        {
            base.Update(dt, gametime, cam);
        }

        //AIProcessing
        protected override void AIProcessing(Camera cam)
        {
            if (isCharging == true) {ChargeAttack(cam); return; }
            if (!isAttacking)
            {
                Move();
                FacePlayer();
            }
            if (attackTimer <= attackSpeed) { attackTimer += 1f; return; }
            if (Vector3.Distance(player.Pos, world.Translation) < attackRange)
            {
                Attack(true, cam);
            }
            if (Vector3.Distance(player.Pos, world.Translation) >= chargeDistance)
            {
                isCharging = true;
            }
        }


        //Attack Function
        protected override void Attack(bool isMelee, Camera cam)
        {
            isAttacking = true;
            if (isMelee)
            {
                isMeleeAttacking = true;
                if (attDurTimer < attackLength) { attDurTimer += 1; return; }
                soundEffects[1].Play(volume: volume, pitch: 0.0f, pan: 0.0f);
                AttackPattern.MinotaurMain(world.Translation + world.Backward * 15, world.Backward, rot, attackDamage, projList, cam);

                foreach (Attack p in projList)
                {
                    if (p.TimeToDestroy)
                    {
                        projList.Remove(p);
                        break;
                    }
                }

            }
            base.Attack(isMelee, cam);
        }

        //Charge Attack
        private void ChargeAttack(Camera cam)
        {
            Vector3 walkDir = world.Backward;
            if (chargeTimer < chargeDelay)
            {
                FacePlayer();
                chargeTimer += 1;
                return;
            }
            else if (vel == 0)
            {
                //start run fast
                vel = 3;
            }
            else
            {
                if (vel <= runMod * speedMax*3) vel += accel * runMod*2;
                else vel = runMod * speedMax;
            }


            if (boundingCollider.Intersects(player.boundingCollider))
            {
                soundEffects[3].Play(volume: volume, pitch: 0.0f, pan: 0.0f);
                AttackPattern.MinotaurCharge(world.Translation + world.Backward * 15, world.Backward, rot, 65f, projList, cam, vel*2);
                player.outsideForceDir += (world.Down * 0.5f);
                player.outsideVelocity += 3;
                walkDir = ManageCollision(player.world, walkDir);
                vel = 0f;
                isCharging = false;
                chargeTimer = 0;
                attDurTimer = 0;
                attackTimer = 0;
            }

            //Fall
            foreach (Level.FloorTile floor in parentScene.thisLevel.FloorTiles)
            {
                if (Vector3.Distance(floor.Position, pos) < 600)
                {
                    if (boundingCollider.Intersects(floor.collider))
                    {
                        pos -= fallSpeed*world.Down;
                        fallSpeed = 0;
                        onGround = true;
                        break;
                    }
                    onGround = false;
                }
            }

            Fall();

            bool staticColission = false;

            foreach (Level.StaticCollisionMesh mesh in parentScene.thisLevel.StaticCollisionMeshes)
            {
                if (Vector3.Distance(mesh.Position, pos) < 400)
                {
                    if (boundingCollider.Intersects(mesh.collider))
                    {
                        walkDir = boundingCollider.Collisiondirection(mesh.collider);
                        staticColission = true;
                        vel = 0f;
                        isCharging = false;
                        chargeTimer = 0;
                        attDurTimer = 0;
                        attackTimer = 0;
                    }
                }
            }
            foreach (Level.InteractiveDoor mesh in parentScene.thisLevel.Doors)

            {
                if (Vector3.Distance(mesh.Position, pos) < 400)
                {
                    if (boundingCollider.Intersects(mesh.leftCollider))
                    {
                        walkDir = boundingCollider.Collisiondirection(mesh.leftCollider);
                        staticColission = true;
                        vel = 0f;
                        isCharging = false;
                        chargeTimer = 0;
                        attDurTimer = 0;
                        attackTimer = 0;
                    }

                    if (boundingCollider.Intersects(mesh.leftCollider))
                    {
                        walkDir = boundingCollider.Collisiondirection(mesh.rightCollider);
                        staticColission = true;
                        vel = 0f;
                        isCharging = false;
                        chargeTimer = 0;
                        attDurTimer = 0;
                        attackTimer = 0;
                    }
                }
            }

            if (vel < 0) vel = 0;


            //Cache position
            if (Vector3.Distance(pos, cachedPosition[6]) > 8f && !staticColission)
            {
                cachedPosition.Add(pos); //set last position
                cachedPosition.RemoveAt(0);
            }
            pos += walkDir * vel;

        }

        //Blend Animations
        protected override void BlendAnims(GameTime gameTime)
        {
            //reset weights for blending
            SetAnimBlends(1);
            float percent;
            //walk/run blending
            percent = vel / speedMax;
            animations[BASE].UpdateBlendAnim(animations, BASE, WALK, percent, animWeights);   //idle -> walk

            percent = vel/runMod / speedMax;
            if (percent > 1) percent = 1;
            animations[BASE].UpdateBlendAnim(animations, BASE, RUN, percent, animWeights); ;   //walk -> run

            //Main Attack
            if (isAttacking && !animations[ATTACKM].animationRunning) animations[ATTACKM].BeginAnimation(0, gameTime); //start animation
            if (animations[ATTACKM].animationRunning)
            {
                animations[ATTACKM].Update(gameTime);
                percent = 1;
                animations[BASE].UpdateBlendAnim(animations, BASE, ATTACKM, percent, animWeights);   //base -> Main Attack
                if (animations[ATTACKM].currentAnimFrameTime > 0.3)
                {
                    isAttacking = false; //end attack
                    animations[BASE].BeginAnimation(0, gameTime);
                    animations[WALK].BeginAnimation(0, gameTime);
                    animations[RUN].BeginAnimation(0, gameTime);
                }

            }
        }


    }
}
